using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieController : MonoBehaviour
{
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

    [Header("Jump")]
    public float jumpDuration = 0.5f;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip[] hitSounds;

    private Animator anim;
    private NavMeshAgent agent;
    private Transform player;
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private bool isJumping = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Idle();
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
        isJumping = true;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("jump");

        yield return new WaitForSeconds(jumpDuration);

        agent.CompleteOffMeshLink();
        agent.speed = moveSpeed;
        isJumping = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Zombie took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);

        if (hitSounds.Length > 0 && audioSource != null)
            audioSource.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length)]);

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("die");
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 3f);
    }
}