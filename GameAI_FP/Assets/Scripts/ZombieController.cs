using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
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

    [Header("Detection")]
    public float detectionRange = 15f;

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

    private ZombieVariantBehavior variant;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<ZombieSight>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;

        variant = GetComponent<ZombieVariantBehavior>();
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            FastExplodingZombieBehavior exploder = variant as FastExplodingZombieBehavior;
            if (exploder == null || !exploder.IsFusing)
                Attack();
            variant?.OnAttacking();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
            variant?.OnChasing();
        }
        else
        {
            Idle();
            variant?.OnIdle();
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
        if (agent.isActiveAndEnabled)
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
        variant?.OnDamaged();
        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("die");
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        variant?.OnDeath();
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