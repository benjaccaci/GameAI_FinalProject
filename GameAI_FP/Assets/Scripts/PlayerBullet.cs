using UnityEngine;
using UnityEngine.UI;

public class PlayerBullet : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject defaultProjectile;
    public float projectileSpeed = 100;
    public float bulletRange = 20;
    // public AudioClip bulletSFX;

    [Header("Reticle Settings")]
    public Image reticleImage;
    // public Color targetColorDementor;
    public float animationSpeed = 5f;

    // Color originalReticleColor;
    // Vector3 originalReticleScale;

    GameObject currentProjectile;

    // Color currentReticleColor;

    void Start()
    {
        // originalReticleColor = reticleImage.color;
        // originalReticleScale = reticleImage.transform.localScale;

        if(defaultProjectile)
        {
            currentProjectile = defaultProjectile;
        }
        // currentReticleColor = Color.yellow;
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }
    // void FixedUpdate()
    // {
    //     if(!reticleImage)
    //     {
    //         return;
    //     }
    //     InteractiveEffect();
    // }

    void Shoot()
    {
        if (currentProjectile)
        {
            GameObject bullet = Instantiate(currentProjectile,
            transform.position, transform.rotation);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if(rb)
            {
                rb.AddForce(transform.forward * projectileSpeed, ForceMode.VelocityChange);
            }
            // if(spellSFX)
            // {
            //     AudioSource.PlayClipAtPoint(spellSFX,transform.position);
            // }
            // bullet.transform.SetParent(transform);
        }
    }

    void InteractiveEffect()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, bulletRange))
        {
            Debug.Log("Hit something " + hit.collider.name);
            if (hit.collider.CompareTag("Enemy"))
            {
                currentProjectile = defaultProjectile;
                // ReticleAnimation(originalReticleScale / 2, targetColorDementor, animationSpeed);
            }
        }
        else 
        {
            // currentProjectile = defaultProjectile;
            // UpdateReticleColor();
            // ReticleAnimation(originalReticleScale / 2, targetColorDementor, animationSpeed);
        }
    }
    // void ReticleAnimation(Vector3 targetScale, Color targetColor, float speed)
    // {
    //     var step = speed * Time.deltaTime;
    //     reticleImage.color = Color.Lerp(reticleImage.color, targetColor, step);
    //     reticleImage.transform.localScale = 
    //             Vector3.Lerp(reticleImage.transform.localScale, targetScale, step);
    // }

    // void UpdateReticleColor()
    // {
    //     currentReticleColor = currentProjectile.GetComponent<Renderer>().sharedMaterial.color;
    // }


}
