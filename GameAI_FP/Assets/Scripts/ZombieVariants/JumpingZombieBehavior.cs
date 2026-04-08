using UnityEngine;
using System.Collections;

// the jumping zombie behavior
// zombie jumps to attack the player if theyre within the jump range and this requires a rigidbody on the gameobject

public class JumpingZombieBehavior : ZombieVariantBehavior
{
    [Header("Jump Attack")]
    // distance where zombie jumps
    public float jumpRange = 6f;
    // force of the jump
    public float jumpForce = 8f;
    // time between jump attacks (seconds)
    public float jumpCooldown = 3f;
    // damage each jump deals when landed
    public float jumpDamage = 30f;
    private Rigidbody rb;
    private float nextJumpTime = 0f;
    private bool isLeaping = false;
    private Transform player;
    public bool IsLeaping => isLeaping;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogWarning(gameObject.name + "JumpingZombieBehavior requires a rigidbody");
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void OnChasing()
    {
        TryLeap();
    }

    public override void OnAttacking()
    {
        TryLeap();
    }

    void TryLeap()
    {
        if (player == null || isLeaping) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= jumpRange && Time.time >= nextJumpTime)
        {
            StartCoroutine(LeapAtPlayer());
        }
    }

    IEnumerator LeapAtPlayer()
    {
        nextJumpTime = Time.time + jumpCooldown;
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        var anim = GetComponent<Animator>();
        // disables the navmesh for a sec so the zombie moves
        if (agent) agent.enabled = false;
        // jump animation
        if (anim) anim.SetTrigger("jump");
        // jump toward player with arc
        Vector3 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce((dir + Vector3.up * 1.5f) * jumpForce, ForceMode.Impulse);
        // wait before checking to see if the zombie landed
        yield return new WaitForSeconds(0.4f);
        // wait until grounded
        float timeout = 2f;
        while (!IsGrounded() && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        // jump damage
        Collider[] hits = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(jumpDamage);
            }
        }
        if (agent) agent.enabled = true;
        isLeaping = false;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.5f);
    }
}