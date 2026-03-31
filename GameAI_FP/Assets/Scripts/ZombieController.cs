using UnityEngine;
using System.Collections;

public class ZombieController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform target;

    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 20f;

    [Header("Detection")]
    public float detectionRange = 15f;

    private Animator anim;
    private Transform player;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip[] hitSounds;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

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
    }

    void ChasePlayer()
    {
        anim.SetBool("isWalking", true);

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 5f * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    void Attack()
    {
        anim.SetBool("isWalking", false);

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            anim.SetTrigger("attack");
            StartCoroutine(DealDamageAfterDelay(0.8f));
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

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Zombie took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);
        if (audioSource != null && hitSounds.Length > 0)
        {
            int index = Random.Range(0, hitSounds.Length);
            audioSource.PlayOneShot(hitSounds[index]);
        }

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("die");
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 10f);
    }
}