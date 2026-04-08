using UnityEngine;
using System.Collections;

// this is the fast exploding zombie which moves fast and explodes with a damage radius when its close enough to the player
public class FastExplodingZombieBehavior : ZombieVariantBehavior
{
    [Header("Speed")]
    // different movespeed than the default in zombiecontroller
    public float fastMoveSpeed = 7f;
    [Header("Explosion")]
    // range where the fuse of the explosion happens (like when the explosion starts happen)
    public float fuseRange = 3f;
    // time it takes for the zombie to explode after fuse is lit
    private float fuseTime = 2f;
    
    // radius of damage
    public float explosionRadius = 5f;
    // amount of damage dealt
    public float explosionDamage = 80f;
    // vfx prefab if we wanna add that
    public GameObject explosionVFXPrefab;
    private bool fuseStarted = false;
    private bool hasExploded = false;
    private Transform player;
    public bool IsFusing => fuseStarted;

    [Header("Audio")]
    public AudioClip fuseLitClip;
    public AudioClip fuseCountdownClip;
    public AudioClip explosionClip;
    private AudioSource audioSource;



    protected override void Awake()
    {
        base.Awake();
        Debug.Log("FastExplodingZombie spawned");
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (fuseCountdownClip != null)
            fuseTime = fuseCountdownClip.length;
        // apply the faster speed to the nav mesh agent
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.speed = fastMoveSpeed;
    }

    public override void OnChasing()
    {
        if (fuseStarted || hasExploded || player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= fuseRange)
        {
            StartCoroutine(Fuse());
        }
    }

    public override void OnAttacking()
    {
        // if the player is in the attack range but the fuse hasnt started then trigger it -- safe guard in case the fuse doesnt start
        if (!fuseStarted && !hasExploded)
            StartCoroutine(Fuse());
    }

    IEnumerator Fuse()
    {
        fuseStarted = true;
        Debug.Log(gameObject.name + "fuse lit");
        // fuse lit sound for 1.5 seconds
        if (fuseLitClip != null)
        {
            audioSource.clip = fuseLitClip;
            audioSource.Play();
        }
        yield return new WaitForSeconds(1.5f);
        audioSource.Stop();
        // fuse countdown sound, same amount of time as the countdown
        if (fuseCountdownClip != null)
        {
            audioSource.clip = fuseCountdownClip;
            audioSource.Play();
        }
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        if (explosionVFXPrefab) {
            GameObject vfx = Instantiate(explosionVFXPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * 2f;
            // keep vfx alive
            Destroy(vfx, 3f);
        }
        // player takes damage if in the radius
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(explosionDamage);
            }
        }
        Debug.Log(gameObject.name + "exploded");
        // explosion sound effect
        if (explosionClip != null)
            AudioSource.PlayClipAtPoint(explosionClip, transform.position);
        // kill zombie
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 0.1f);
    }

    public override void OnDeath()
    {
        // zombie explodes when it dies if it hasnt exploded before it died
        if (!hasExploded)
            Explode();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fuseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}