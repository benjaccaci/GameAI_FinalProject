using UnityEngine;

public class GunScript : MonoBehaviour
{
    [Header("Gun Type")]
    public bool isFullAuto = false;
    public bool isShotgun = false;

    [Header("Shooting")]
    public float fireRate = 0.3f;
    public float range = 100f;
    public float damage = 25f;
    public Camera playerCamera;

    [Header("Shotgun Settings")]
    public int pelletCount = 8;
    public float shotgunSpread = 0.1f;

    [Header("Recoil")]
    public float recoilAmount = 30f;
    public float recoilSpeed = 15f;
    public float returnSpeed = 5f;

    [Header("Camera Recoil (Full Auto)")]
    public bool useCameraRecoil = false;
    public float cameraRecoilUp = 0.5f;
    public float cameraRecoilSide = 0.1f;
    public AnimationCurve recoilPattern;

    [Header("Hip Fire")]
    public float hipFireSpread = 0.05f;

    [Header("ADS")]
    public Transform hipFirePoint;
    public Transform adsPoint;
    public float adsSpeed = 10f;

    [Header("Effects")]
    public AudioSource gunShotSound;
    public ParticleSystem muzzleFlash;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private float nextFireTime = 0f;
    private float recoilTimer = 0f;
    private MouseLook mouseLook;

void Start()
{
    originalRotation = transform.localRotation;
    targetRotation = originalRotation;

    if (playerCamera == null)
        playerCamera = Camera.main;

    mouseLook = playerCamera.GetComponentInParent<MouseLook>();
}

    void Update()
    {
        // ADS / Hip fire
        if (Input.GetMouseButton(1))
            transform.position = Vector3.Lerp(transform.position, adsPoint.position, adsSpeed * Time.deltaTime);
        else
            transform.position = Vector3.Lerp(transform.position, hipFirePoint.position, adsSpeed * Time.deltaTime);

        // Shooting
        bool fireInput = isFullAuto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (fireInput && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            targetRotation = originalRotation * Quaternion.Euler(-recoilAmount, 0f, 0f);

            if (gunShotSound != null)
                gunShotSound.Play();

            if (isShotgun)
            {
                for (int i = 0; i < pelletCount; i++)
                    Shoot(shotgunSpread);
            }
            else
            {
                Shoot(0f);
            }

            if (muzzleFlash != null)
                muzzleFlash.Play();

            // Camera recoil
            if (useCameraRecoil && mouseLook != null)
            {
                float patternValue = recoilPattern != null ? recoilPattern.Evaluate(recoilTimer) : 0f;
                float vertical = cameraRecoilUp;
                float horizontal = patternValue * cameraRecoilSide;
                mouseLook.AddRecoil(vertical, horizontal);
                recoilTimer += fireRate;
            }
        }

        // Reset recoil timer when not shooting
        if (isFullAuto && !Input.GetMouseButton(0))
            recoilTimer = 0f;

        // Recoil
        if (targetRotation != originalRotation)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, recoilSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.5f)
                targetRotation = originalRotation;
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, returnSpeed * Time.deltaTime);
        }
    }

    void Shoot(float extraSpread)
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Add hip fire spread + any extra spread (shotgun)
        float totalSpread = extraSpread;
        if (!Input.GetMouseButton(1))
            totalSpread += hipFireSpread;

        if (totalSpread > 0f)
        {
            Vector3 spread = new Vector3(
                Random.Range(-totalSpread, totalSpread),
                Random.Range(-totalSpread, totalSpread),
                0f
            );
            ray.direction = (ray.direction + spread).normalized;
        }

        RaycastHit hit;
        Vector3 endPoint = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            endPoint = hit.point;

                ZombieController zombie = hit.collider.GetComponent<ZombieController>();
                if (zombie != null)
                    zombie.TakeDamage(damage);
        }

        DrawTrail(ray.origin, endPoint);
    }

    void DrawTrail(Vector3 start, Vector3 end)
    {

        // GameObject trail = new GameObject("BulletTrail");
        // LineRenderer lr = trail.AddComponent<LineRenderer>();
        // lr.startWidth = 0.01f;
        // lr.endWidth = 0.01f;
        // lr.positionCount = 2;
        // lr.SetPosition(0, start);
        // lr.SetPosition(1, end);
        // lr.material = new Material(Shader.Find("Unlit/Color"));
        // lr.material.color = Color.red;
    }
}