using UnityEngine;
// behavior script for the projectile that the throwing zombie throws
public class ZombieProjectile : MonoBehaviour
{
    // throwingzombiebehavior sets this when it spawns
    public float damage = 25f;
    // time before projectile gets destroyed from level (so it doesnt stay forever)
    public float lifetime = 5f;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}