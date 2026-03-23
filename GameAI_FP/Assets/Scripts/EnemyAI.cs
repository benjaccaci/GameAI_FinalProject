using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform[] waypoints = null;
    private int currentWaypointIndex = 0;
    public enum EnemyState { Patrol, Attack, Pursue, Die };
    [SerializeField] private EnemyState currentState;
    public float movementSpeed = 3.5f;
    private NavMeshAgent agent = null;

    Animator animator;
    int animState;

    [Header("Rotation Setttings")]
    public Transform head;
    public float angularSpeed = 30f;
    public float detectionRange = 10f;

    [Header("Attack Settings")]
    public bool canAttack;
    public GameObject bulletPrefab;
    public int projectileSpeed = 20;
    public AudioClip bulletFireSFX;
    public GameObject bulletFireVFX;
    public Transform FXSpawnPoint;
    public Transform bulletFirePoint;
    Transform target;
    public float minimumDistanceToPlayer = 2f;
    public float fireRate = 2f;
    float fireCooldown = 0;

    [Header("Die Settings")]
    public int health = 100;
    public GameObject destroyPrefab;
    bool IsEnemyDead;
    [Header("UI + Object Settings")]
    public Slider healthBar;
    public GameObject healthPackPrefab;
    public GameObject ammoPackPrefab;

    void Start()
    {
        
         if (!animator) 
         {
            animator = GetComponent<Animator>();
            animState = 1;
            animator.SetInteger("animState" , animState);
         }

        if (!target)
        {
            target = GameObject.FindWithTag("Player").transform;
        }

        if (!bulletPrefab)
        {
            bulletPrefab = Resources.Load<GameObject>("Prefabs/EnemyBullet");
        }

        if (!bulletFireSFX)
        {
            Debug.LogWarning(gameObject.name + ": No bullet fire SFX assigned!");
            return;
        }

        if (!bulletFireVFX)
        {
            Debug.LogWarning(gameObject.name + ": No bullet fire VFX assigned!");
            return;
        }

        if (health <= 0)
        { // uninitialized health
            health = 100;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning(gameObject.name + ": No waypoints assigned!");
            return;
        }

        if (healthBar)
        {
            healthBar.maxValue = health;
            healthBar.value = health;
        }

        // FSM settings
        currentState = EnemyState.Patrol;
        agent = gameObject.GetComponent<NavMeshAgent>();

        if (!agent)
        {
            Debug.LogWarning(gameObject.name + ": No NavMeshAgent found!");
            return;
        }
        agent.speed = movementSpeed;
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.angularSpeed = angularSpeed;
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Navigate();
                break;
            case EnemyState.Attack:
                if (canAttack)
                {
                    Attack();
                }
                else
                {
                    Debug.LogWarning(gameObject.name + ": Attack - No attack - patrol instead");
                    currentState = EnemyState.Patrol;
                }
                break;
            case EnemyState.Pursue:
                Pursue();
                break;
            case EnemyState.Die:
                Debug.Log(gameObject.name + ": Dying...");
                Die();
                break;
        }
    }

    void Navigate()
    {
        // In patrol mode, the enemy will move between waypoints
        if (IsEnemyDead || currentState != EnemyState.Patrol)
            return;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Debug.Log(gameObject.name + ": Navigate - Reached waypoint " + currentWaypointIndex);
            // Check if the enemy is close to the player before switching to attack mode
            AdvanceToNextWaypoint();
            return;
        }

        if (canAttack)
        {
            FindPlayer();
            return;
        }

        // Play patrol animation
        animState = 1;
        animator.SetInteger("animState" , animState);
    }

    void Attack()
    {
        if (IsEnemyDead|| currentState != EnemyState.Attack) // Died while attacking
            return;

        agent.ResetPath();
        agent.isStopped = true; // Stop the NavMesh agent to prevent it from moving while attacking
        agent.updateRotation = false; // Disable rotation to prevent the agent from rotating towards the target
        agent.updatePosition = false; // Disable position updates to prevent the agent from moving while attacking
        transform.LookAt(target);

        // Debug.Log(gameObject.name + ": Attack - Attacking Player...");
        // no target or out of range
        if (!target || Vector3.Distance(transform.position, target.position) > detectionRange)
        {
            // Debug.Log(gameObject.name + ": Attack - No target or out of range, switching to pursue");
            currentState = EnemyState.Pursue;
            return;
        }

        // Rotate to face target
        // turret.LookAt(target);
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        head.rotation = Quaternion.Slerp(head.rotation, lookRotation, angularSpeed * Time.deltaTime);

        if (fireCooldown <= 0)
        {
            // StartCoroutine(AttackCoroutine());
            ShootProjectile();
            fireCooldown = 1f / fireRate;
        }

        fireCooldown -= Time.deltaTime;

        // Play attack animation
        animState = 3;
        animator.SetInteger("animState", animState);
        return;
    }

    void FindPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
        Transform nearestPlayer = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                float distanceToEnemy = Vector3.Distance(transform.position, collider.transform.position);

                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestPlayer = collider.transform;
                }
            }
        }

        if (nearestPlayer)
        {
            target = nearestPlayer;
            // Debug.Log("Player detected: " + target.name);
            currentState = EnemyState.Attack;
            return;
        }
        else
        {
            // Debug.Log("No player detected, continuing patrol...");
            // currentState = EnemyState.Patrol;
            return;
        }
    }

    void Pursue()
    {
        if (IsEnemyDead || currentState != EnemyState.Pursue) // Died while pursuing
            return;

        // agent.ResetPath(); // Reset the NavMesh agent's path
        agent.isStopped = false; // Resume the NavMesh agent
        agent.updateRotation = true; // Enable rotation to allow the agent to rotate towards the target
        agent.updatePosition = true; // Disable position updates to prevent the agent from moving while attacking
        agent.SetDestination(target.position); // Move towards the player

        // Play pursue animation
        animState = 2;
        animator.SetInteger("animState", animState);
        if (Vector3.Distance(transform.position, target.position) <= detectionRange)
        {
            // Debug.Log(gameObject.name + ": Pursue - Close enough to attack, still moving to player...");
            currentState = EnemyState.Attack;
            return;
        }
        return;
    }

    // Move to the next waypoint in the array
    // Code reference: https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavAgentPatrol.html
    void AdvanceToNextWaypoint()
    {
        if (IsEnemyDead)
            return;
        if (waypoints.Length == 0)
            // Debug.LogWarning(gameObject.name + ": No waypoints assigned!");
            return;
        if (currentState != EnemyState.Patrol)
            return;

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        // Debug.Log(gameObject.name + ": Moving to waypoint " + currentWaypointIndex);
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        return;
    }

    void ShootProjectile()
    {
        if (IsEnemyDead) // Died while shooting
            return;

        // Play effects
        Quaternion FXRotation = Quaternion.Euler(0, -180, 0); // adjust VFX rotation
        Instantiate(bulletFireVFX, FXSpawnPoint.position, FXRotation);
        AudioSource.PlayClipAtPoint(bulletFireSFX, FXSpawnPoint.position);

        // Fire the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletFirePoint.position + bulletFirePoint.forward, bulletFirePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.AddForce(transform.forward * projectileSpeed, ForceMode.VelocityChange);
        }
        bullet.transform.SetParent(bulletFirePoint);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (healthBar)
        {
            healthBar.value = health;
        }

        if (health <= 0)
        {
            currentState = EnemyState.Die;
        }
    }

    void Die()
    {
        if (IsEnemyDead)
            return;

        // Play death animation
        animState = 4;
        animator.SetInteger("animState", animState);
        Debug.Log("Enemy Dead...");
        IsEnemyDead = true;
        agent.isStopped = true;

        int chosenDrop = Random.Range(0,2);
        if (chosenDrop == 0) {
            Instantiate(healthPackPrefab, transform.position, Quaternion.identity);
        } else {
            Instantiate(ammoPackPrefab, transform.position, Quaternion.identity);
        }

        if (destroyPrefab)
            Instantiate(destroyPrefab, transform.position, transform.rotation);
        Destroy(gameObject, 3);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minimumDistanceToPlayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                Gizmos.DrawRay(transform.position, waypoints[i].position - transform.position);
            }
        }
    }
}
