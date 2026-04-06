using UnityEngine;

public class GunSwap : MonoBehaviour
{
    public Camera playerCamera;
    public Transform gunHolder;
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;
    public AudioClip pickupSound;

    private GameObject currentGun;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            Debug.Log("E pressed");

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, pickupRange))
            {
                Debug.Log("Ray hit: " + hit.collider.gameObject.name);

                GunPickup pickup = hit.collider.GetComponent<GunPickup>();

                if (pickup != null)
                {
                    Debug.Log("GunPickup found, equipping: " + pickup.gunPrefab.name);
                    audioSource.clip = pickupSound;
                    audioSource.volume = 1.0f;
                    audioSource.Play();
                    EquipGun(pickup.gunPrefab);
                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    Debug.Log("No GunPickup script on: " + hit.collider.gameObject.name);
                }
            }
            else
            {
                Debug.Log("Ray hit nothing within range: " + pickupRange);
            }
        }
    }

    void EquipGun(GameObject gunPrefab)
    {
        // Destroy current gun if holding one
        if (currentGun != null)
            Destroy(currentGun);

        // Spawn new gun under the gun holder
        currentGun = Instantiate(gunPrefab, gunHolder);
        currentGun.transform.localPosition = Vector3.zero;
        currentGun.transform.localRotation = Quaternion.identity;
    }
}