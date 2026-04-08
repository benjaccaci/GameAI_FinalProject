using UnityEngine;
using System.Collections;

// fire zone on the ground if the molotov hits the ground
public class FireZone : MonoBehaviour
{
    // damage per second in the zome
    public float damagePerSecond = 20f;
    // how long fire lasts
    public float duration = 5f;
    // radius of zone
    public float radius = 2f;
    private float timer = 0f;
    private float damageTickTimer = 0f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        timer += Time.deltaTime;
        damageTickTimer += Time.deltaTime;
        // deal damage if player in range
        if (damageTickTimer >= 1f)
        {
            damageTickTimer = 0f;
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= radius)
                {
                    PlayerHealth ph = player.GetComponent<PlayerHealth>();
                    if (ph != null) ph.TakeDamage(damagePerSecond);
                }
            }
        }
        // destroy after time is up
        if (timer >= duration)
            Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}