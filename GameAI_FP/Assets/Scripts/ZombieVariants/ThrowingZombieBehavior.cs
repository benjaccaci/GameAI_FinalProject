using UnityEngine;
using System.Collections;

// throwing zombie that stays a distance from the player and throws projectiles
public class ThrowingZombieBehavior : ZombieVariantBehavior
{
    [Header("Preferred Range")]
    // closest the player can get to the zombie before it backs away
    public float preferredMinRange = 5f;
    // farthest the player can be from the zombie before it gets closer
    public float preferredMaxRange = 12f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    // throw point (empty child gameobject)
    public Transform throwOrigin;
    // force of the throw
    public float throwForce = 15f;
    // time (seconds) before the next throw
    public float throwCooldown = 2.5f;
    // damage each projectile does
    public float projectileDamage = 25f;
    private float nextThrowTime = 0f;
    private Transform player;
    private UnityEngine.AI.NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void OnChasing()
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < preferredMinRange)
        {
            // zombie backs away if its too close to the player
            Vector3 fleeDir = (transform.position - player.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDir * preferredMinRange;
            agent.SetDestination(fleeTarget);
        }
        else if (dist >= preferredMinRange && dist <= preferredMaxRange)
        {
            // if the player is in the range then the zombie stops moving and throws projectile
            agent.ResetPath();
            TryThrow();
        }
        // using default chaseplayer from zombie controller if the player is too far and the zombie needs to move closer
    }

    public override void OnAttacking()
    {
        // throws instead of punching like default
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < preferredMinRange)
        {
            // back away from attack range
            Vector3 fleeDir = (transform.position - player.position).normalized;
            agent.SetDestination(transform.position + fleeDir * preferredMinRange);
        }
        else
        {
            TryThrow();
        }
    }

    void TryThrow()
    {
        if (Time.time < nextThrowTime) return;
        if (projectilePrefab == null)
        {
            Debug.LogWarning(gameObject.name + ": no projectile prefab assigned");
            return;
        }
        nextThrowTime = Time.time + throwCooldown;
        Transform origin = throwOrigin != null ? throwOrigin : transform;
        GameObject proj = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        // ignore collision with the zombie
        Physics.IgnoreCollision(proj.GetComponent<Collider>(), GetComponent<Collider>());
        // aim at player
        Vector3 targetPos = player.position + Vector3.up * 0.5f;
        Vector3 dir = (targetPos - origin.position).normalized;
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = true;
            rb.AddForce(dir * throwForce, ForceMode.Impulse);
        }
        // damage is passed to projectile (if it has damage script)
        MolotovProjectile mp = proj.GetComponent<MolotovProjectile>();
        if (mp != null) mp.damage = projectileDamage;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredMinRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, preferredMaxRange);
    }
}