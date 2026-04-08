using UnityEngine;
using System.Collections;
using TMPro;

public class GunScript : MonoBehaviour
{
    [Header("State")]
    public bool isEquipped = false;

    [Header("Model")]
    public Transform gunModel;

    [Header("UI")]
    public TMP_Text ammoCountUI;
    public TMP_Text reserveAmmoUI;

    [Header("Gun Type")]
    public bool isFullAuto = false;
    public bool isShotgun = false;

    [Header("Shooting")]
    public float fireRate = 0.3f;
    public float range = 100f;
    public float damage = 25f;
    public Camera playerCamera;

    [Header("Ammo")]
    public int magSize = 30;
    public int reserveAmmo = 90;
    public AudioSource reloadSound;
    private int currentAmmo;
    private bool isReloading = false;
    private bool reloadReturning = false;

    [Header("Shotgun Settings")]
    public int pelletCount = 8;
    public float shotgunSpread = 0.1f;

    [Header("Shotgun Reload")]
    public float shellReloadTime = 0.5f;

    [Header("Recoil")]
    public float recoilAmount = 3f;
    public float returnSpeed = 8f;

    [Header("Camera Recoil (Full Auto)")]
    public bool useCameraRecoil = false;
    public float cameraRecoilUp = 0.4f;
    public float cameraRecoilSide = 0.3f;
    public AnimationCurve recoilPattern;

    [Header("Hip Fire")]
    public float hipFireSpread = 0.05f;

    [Header("ADS")]
    public Transform hipFirePoint;
    public Transform adsPoint;
    public float adsSpeed = 10f;

    [Header("Reload Position")]
    public Transform reloadPoint;
    public float reloadTransitionSpeed = 15f;

    [Header("Effects")]
    public AudioSource gunShotSound;
    public ParticleSystem muzzleFlash;

    [Header("Bullet Trail")]
    public bool showBulletTrail = true;
    public float trailDuration = 0.1f;
    public Color trailColor = Color.red;

    private Quaternion originalRotation;
    private float nextFireTime = 0f;
    private float recoilTimer = 0f;
    private MouseLook mouseLook;

    void Start()
    {
        if (gunModel == null)
            gunModel = transform;

        originalRotation = gunModel.localRotation;
        currentAmmo = magSize;

        if (ammoCountUI != null && reserveAmmoUI != null && isEquipped)
        {
            ammoCountUI.text = currentAmmo.ToString();
            reserveAmmoUI.text = reserveAmmo.ToString();
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        mouseLook = playerCamera.GetComponentInParent<MouseLook>();
    }

    void OnEnable()
    {
        if (gunModel != null)
            originalRotation = gunModel.localRotation;
    }

    void Update()
    {
        if (!isEquipped) return;

        if (ammoCountUI != null && reserveAmmoUI != null)
        {
            ammoCountUI.text = currentAmmo.ToString();
            reserveAmmoUI.text = reserveAmmo.ToString();
        }

        if (isReloading)
        {
            if (reloadReturning)
            {
                gunModel.position = Vector3.Lerp(gunModel.position, hipFirePoint.position, reloadTransitionSpeed * Time.deltaTime);
                gunModel.localRotation = Quaternion.Slerp(gunModel.localRotation, originalRotation, reloadTransitionSpeed * Time.deltaTime);
            }
            else if (reloadPoint != null)
            {
                gunModel.position = Vector3.Lerp(gunModel.position, reloadPoint.position, reloadTransitionSpeed * Time.deltaTime);
                gunModel.localRotation = Quaternion.Slerp(gunModel.localRotation, reloadPoint.localRotation, reloadTransitionSpeed * Time.deltaTime);
            }
            else
            {
                gunModel.position = Vector3.Lerp(gunModel.position, hipFirePoint.position, reloadTransitionSpeed * Time.deltaTime);
            }
            return;
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magSize && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // Auto reload on empty
        if (currentAmmo <= 0 && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // ADS / Hip fire
        if (Input.GetMouseButton(1))
            gunModel.position = Vector3.Lerp(gunModel.position, adsPoint.position, adsSpeed * Time.deltaTime);
        else
            gunModel.position = Vector3.Lerp(gunModel.position, hipFirePoint.position, adsSpeed * Time.deltaTime);

        // Shooting
        bool fireInput = isFullAuto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (fireInput && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;

            if (ammoCountUI != null && reserveAmmoUI != null)
            {
                ammoCountUI.text = currentAmmo.ToString();
                reserveAmmoUI.text = reserveAmmo.ToString();
            }

            Debug.Log("Ammo: " + currentAmmo + "/" + magSize + " | Reserve: " + reserveAmmo);

            // Snap to recoil — only rotate on local X axis
            gunModel.localRotation = originalRotation * Quaternion.Euler(-recoilAmount, 0f, 0f);

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

        // Smooth recovery back to original rotation
        gunModel.localRotation = Quaternion.Slerp(gunModel.localRotation, originalRotation, returnSpeed * Time.deltaTime);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        reloadReturning = false;
        Debug.Log("Reloading...");

        int ammoNeeded = magSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        if (isShotgun)
        {
            float totalReloadTime = ammoToReload * shellReloadTime;

            if (reloadSound != null)
            {
                reloadSound.Play();

                float returnTime = 0.3f;
                yield return new WaitForSeconds(totalReloadTime - returnTime);

                reloadReturning = true;
                yield return new WaitForSeconds(returnTime);

                reloadSound.Stop();
            }
            else
            {
                yield return new WaitForSeconds(totalReloadTime);
            }

            currentAmmo += ammoToReload;
            reserveAmmo -= ammoToReload;

            if (ammoCountUI != null && reserveAmmoUI != null)
            {
                ammoCountUI.text = currentAmmo.ToString();
                reserveAmmoUI.text = reserveAmmo.ToString();
            }
        }
        else
        {
            if (reloadSound != null)
            {
                reloadSound.Play();
                float clipLength = reloadSound.clip.length;
                float returnTime = 0.3f;

                yield return new WaitForSeconds(clipLength - returnTime);

                reloadReturning = true;
                yield return new WaitForSeconds(returnTime);
            }
            else
            {
                yield return new WaitForSeconds(1.2f);
                reloadReturning = true;
                yield return new WaitForSeconds(0.3f);
            }

            currentAmmo += ammoToReload;
            reserveAmmo -= ammoToReload;

            if (ammoCountUI != null && reserveAmmoUI != null)
            {
                ammoCountUI.text = currentAmmo.ToString();
                reserveAmmoUI.text = reserveAmmo.ToString();
            }
        }

        Debug.Log("Reloaded! Ammo: " + currentAmmo + "/" + magSize + " | Reserve: " + reserveAmmo);
        isReloading = false;
        reloadReturning = false;
    }

    void Shoot(float extraSpread)
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

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
            if (zombie == null)
                zombie = hit.collider.GetComponentInParent<ZombieController>();

            if (zombie != null)
                zombie.TakeDamage(damage);
        }
    }

    public void AddAmmo(int amount)
    {
        reserveAmmo += amount;
        if (reserveAmmo < 0)
            reserveAmmo = 0;

        if (isEquipped && ammoCountUI != null && reserveAmmoUI != null)
        {
            ammoCountUI.text = currentAmmo.ToString();
            reserveAmmoUI.text = reserveAmmo.ToString();
        }
    }

    IEnumerator DrawTrail(Vector3 start, Vector3 end)
    {
    GameObject trail = new GameObject("BulletTrail");
    LineRenderer lr = trail.AddComponent<LineRenderer>();
    lr.startWidth = 0.01f;
    lr.endWidth = 0.01f;
    lr.positionCount = 2;
    lr.SetPosition(0, start);
    lr.SetPosition(1, end);
    lr.material = new Material(Shader.Find("Unlit/Color"));
    lr.material.color = trailColor;

    yield return new WaitForSeconds(trailDuration);
    Destroy(trail);
    }
}