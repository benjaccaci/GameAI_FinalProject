using UnityEngine;
using UnityEngine.AI;

public class KingZombieBehavior : ZombieVariantBehavior
{
    public float startingHealth = 500f;

    [Header("Retreat Behavior")]
    public float preferredDistance = 18f;
    public float retreatTriggerDistance = 12f;

    private NavMeshAgent agent;
    private Transform player;
    private AudioSource audioSource;
    private bool isAlone = false;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (controller != null)
        {
            controller.maxHealth = startingHealth;
        }
    }

    public override bool OverridesChase() => !isAlone;
    public override bool OverridesAttack() => !isAlone;

    public override void OnIdle()
    {
        RefreshAloneStatus();
    }

    public override void OnChasing()
    {
        RefreshAloneStatus();

        if (!isAlone)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist < retreatTriggerDistance)
            {
                RetreatFromPlayer();
            }
            else
            {
                agent.ResetPath();
            }
        }
    }

    public override void OnAttacking()
    {
        RefreshAloneStatus();

        if (!isAlone)
        {
            RetreatFromPlayer();
        }
    }

    public override void OnInvestigating()
    {
        RefreshAloneStatus();
    }

    public override void OnSearching()
    {
        RefreshAloneStatus();
    }

    public override void OnDeath()
    {
        Debug.Log("King zombie killed");
    }

    void RefreshAloneStatus()
    {
        if (isAlone) return;

        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Enemy");
        int livingCount = 0;
        foreach (var z in zombies)
        {
            if (z == gameObject) continue;
            if (z.activeInHierarchy)
                livingCount++;
        }

        if (livingCount == 0)
        {
            isAlone = true;
            Debug.Log("King zombie now alone");
        }
    }

    void RetreatFromPlayer()
    {
        Vector3 awayDir = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + awayDir * preferredDistance;

        if (NavMesh.SamplePosition(retreatTarget, out NavMeshHit hit, preferredDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, retreatTriggerDistance);
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
    }
}