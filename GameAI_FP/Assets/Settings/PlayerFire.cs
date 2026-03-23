using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject defaultProjectile;
    public float bulletRange = 20;
    public float fireRate;
    private float fireCooldown;
    public float projectileSpeed;
    public int startingAmmoCount = 15;
    public AudioClip NoAmmoSFX;
    [Header("Pistol Settings")]
    public AudioClip pistolFireSFX;
    private int currentAmmoCount;
    [Header("Shotgun Settings")]
    public bool hasShotgun;
    public AudioClip shotgunFireSFX;
    public AudioClip shotgunReloadSFX;
    public int shotgunPellets = 5;
    [Header("Reticle Settings")]
    public Image reticleImage;
    public float animationSpeed = 5f;
    GameObject currentProjectile;

    void Start()
    {
        currentAmmoCount = startingAmmoCount;
        fireCooldown = 0f;
        if (defaultProjectile)
        {
            currentProjectile = defaultProjectile;
        }

        hasShotgun = false;
    }

    void Update()
    {
        // Update cooldown
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        } else
        {
            fireCooldown = 0f;
        }

        if (currentAmmoCount > 0) {
            // Check for shooting input
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot(false);
                // Normal fire is one bullet
                currentAmmoCount--;
            }
            if (Input.GetButtonDown("Fire2"))
            {
                if (hasShotgun)
                {
                    // Shotgun fire is 3 bullets
                    if (currentAmmoCount >= 3)
                    {
                        Shoot(true);
                        currentAmmoCount -= 3;
                    }
                    else
                    {
                        // Not enough ammo for shotgun
                        Debug.Log("Not enough ammo for shotgun!");
                        AudioSource.PlayClipAtPoint(NoAmmoSFX, transform.position);

                    }
                }
            }
        } else {
            if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"))
            {
                // No ammo left, play no ammo sound
                Debug.Log("No ammo left!");
                AudioSource.PlayClipAtPoint(NoAmmoSFX, transform.position);
            }
        }
    }

    public void AddAmmo(int ammo)
    {
        currentAmmoCount += ammo;
    }

    public int CurrentAmmoCount() {
        return currentAmmoCount;
    }

    void Shoot(bool isShotgunFire)
    {
        if (currentProjectile)
        {
            if (isShotgunFire) // using shotgun
            {
                // Create multiple bullets in a proper shotgun spread pattern
                for (int i = 0; i < shotgunPellets; i++)
                {
                    GameObject spreadBullet = Instantiate(currentProjectile,
                    transform.position + transform.forward, transform.rotation);

                    Rigidbody rb = spreadBullet.GetComponent<Rigidbody>();

                    // Create a cone-shaped spread pattern
                    float spreadAngle = 15f; // Degrees of spread
                    Vector3 spreadDirection = transform.forward;
                    
                    // Calculate random angle within the cone
                    float angleX = Random.Range(-spreadAngle, spreadAngle);
                    float angleY = Random.Range(-spreadAngle, spreadAngle);
                    
                    // Apply the spread to the bullet direction
                    spreadDirection = Quaternion.Euler(angleX, angleY, 0) * spreadDirection;
                    
                    if (rb)
                    {
                        rb.AddForce(spreadDirection * projectileSpeed, ForceMode.VelocityChange);
                    }
                    
                    // Make the bullet face the direction it's moving
                    spreadBullet.transform.forward = spreadDirection;
                }

                // Play shotgun sound effect
                AudioSource.PlayClipAtPoint(shotgunFireSFX, transform.position);
                // Play shotgun reload sound effect
                AudioSource.PlayClipAtPoint(shotgunReloadSFX, transform.position);
            }
            else // normal fire
            {
                GameObject bullet = Instantiate(currentProjectile, transform.position + transform.forward, transform.rotation);
                AudioSource.PlayClipAtPoint(pistolFireSFX, transform.position);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                if (rb)
                {
                    rb.AddForce(transform.forward * projectileSpeed, ForceMode.VelocityChange);
                }
            }
        }

        // reset cooldown
        fireCooldown = fireRate;
    }
}
