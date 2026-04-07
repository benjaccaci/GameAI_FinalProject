using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ZombieController : MonoBehaviour
{
    public enum ZombieState
    {
        Idle,
        Chase,
        Attack,
        Investigate,
        Search
    }

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 20f;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip[] hitSounds;

    [Header("Loot Drops")]
    public GameObject healthPackPrefab;
    public GameObject ammoPackPrefab;
    public GameObject coinPrefab;

    // --- State ---
    public ZombieState CurrentState { get; private set; } = ZombieState.Idle;

    private Animator anim;
    private NavMeshAgent agent;
    private Transform player;
    private ZombieSight sight;
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private bool isJumping = false;

    // Search state
    private float searchTimer = 0f;
    private bool hasSearchDestination = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<ZombieSight>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;
    }

    void Update()
    {
        if (isDead) return;

        if (agent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(HandleJump());
            return;
        }

        if (isJumping) return;

        UpdateState();
        ExecuteState();
    }

    void UpdateState()
    {
        // Sight-based transitions take priority
        if (sight.HasTarget)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                TransitionTo(ZombieState.Attack);
            }
            else
            {
                TransitionTo(ZombieState.Chase);
            }
            return;
        }

        // If we just lost the target or received an alert, investigate
        if (sight.IsAlerted && CurrentState != ZombieState.Investigate && CurrentState != ZombieState.Search)
        {
            TransitionTo(ZombieState.Investigate);
            return;
        }

        // If investigating and arrived at destination
        if (CurrentState == ZombieState.Investigate)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                TransitionTo(ZombieState.Search);
            }
            return;
        }

        // If searching and timer expired
        if (CurrentState == ZombieState.Search)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                TransitionTo(ZombieState.Idle);
            }
            return;
        }
    }

    void TransitionTo(ZombieState newState)
    {
        if (CurrentState == newState) return;

        // Exit current state
        switch (CurrentState)
        {
            case ZombieState.Search:
                hasSearchDestination = false;
                break;
        }

        CurrentState = newState;

        // Enter new state
        switch (newState)
        {
            case ZombieState.Idle:
                sight.ClearAlertState();
                break;
            case ZombieState.Investigate:
                agent.SetDestination(sight.LastKnownPosition);
                anim.SetBool("isWalking", true);
                break;
            case ZombieState.Search:
                sight.BeginSearch();
                searchTimer = sight.config.searchDuration;
                hasSearchDestination = false;
                break;
        }
    }

    void ExecuteState()
    {
        switch (CurrentState)
        {
            case ZombieState.Idle:
                Idle();
                break;
            case ZombieState.Chase:
                ChasePlayer();
                break;
            case ZombieState.Attack:
                Attack();
                break;
            case ZombieState.Investigate:
                anim.SetBool("isWalking", true);
                break;
            case ZombieState.Search:
                SearchArea();
                break;
        }
    }

    void Idle()
    {
        anim.SetBool("isWalking", false);
        agent.ResetPath();
    }

    void ChasePlayer()
    {
        anim.SetBool("isWalking", true);
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        anim.SetBool("isWalking", false);
        agent.ResetPath();

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            anim.SetTrigger("attack");
            StartCoroutine(DealDamageAfterDelay(0.5f));
        }
    }

    void SearchArea()
    {
        anim.SetBool("isWalking", true);

        if (!hasSearchDestination || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            Vector3 wanderPoint = sight.GetSearchWanderPoint();
            agent.SetDestination(wanderPoint);
            hasSearchDestination = true;
        }
    }

    IEnumerator DealDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    IEnumerator HandleJump()
    {
        agent.updatePosition = false;
        isJumping = true;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("jump");
        var info = anim.GetAnimatorTransitionInfo(0);
        yield return new WaitForSeconds(info.duration);

        agent.CompleteOffMeshLink();
        agent.speed = moveSpeed;
        isJumping = false;
        agent.updatePosition = true;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Zombie took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);

        if (hitSounds.Length > 0 && audioSource != null)
            audioSource.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length)]);

        // Notify sight system about damage
        sight.OnDamageTaken();

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("die");
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        DropLoot();
        Destroy(gameObject, 3f);
    }

    void DropLoot()
    {
        int randomValue = Random.Range(0, 100);
        if (randomValue < 33)
        {
            Instantiate(healthPackPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        else if (randomValue < 67)
        {
            Instantiate(ammoPackPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        else if (randomValue > 67)
        {
            Instantiate(coinPrefab, transform.position + Vector3.up, coinPrefab.transform.rotation);
        }
    }
}
