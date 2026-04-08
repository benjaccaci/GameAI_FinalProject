using UnityEngine;

// molotov that the throwing zombie throws, does damage when it hits the player and makes a small fire area on the ground
public class MolotovProjectile : MonoBehaviour
{
    // impact damage
    public float damage = 15f;
    // fire radius
    public float impactRadius = 2f;
    // autobreaks if it doesnt hit
    public float lifetime = 5f;
    public AudioClip breakSound;
    public GameObject fireZonePrefab;
    public GameObject impactVFXPrefab;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        // instant damage to player when it hits
        Collider[] hits = Physics.OverlapSphere(transform.position, impactRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage);
            }
        }
        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        // impact vfx
        if (impactVFXPrefab)
            Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
        // fire on ground
        if (fireZonePrefab)
        {
            // find ground position
            Vector3 spawnPos = transform.position;
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
                spawnPos = hit.point;
            Instantiate(fireZonePrefab, spawnPos, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}