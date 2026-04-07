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

        if (gunHolder == null)
        {
            GameObject holder = GameObject.FindGameObjectWithTag("GunHolder");
            if (holder != null)
                gunHolder = holder.transform;
            else
                Debug.LogError("No object with tag 'GunHolder' found!");
        }

        Debug.Log("GunHolder: " + (gunHolder != null ? GetPath(gunHolder) : "NOT FOUND"));
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
                Debug.Log("Ray hit: " + hit.collider.gameObject.name + " Tag: " + hit.collider.tag);

                if (hit.collider.CompareTag("Gun"))
                {
                    GameObject gunObject = hit.collider.gameObject;

                    GunScript gunScript = gunObject.GetComponent<GunScript>();
                    if (gunScript == null)
                        gunScript = gunObject.GetComponentInParent<GunScript>();

                    if (gunScript != null)
                    {
                        Debug.Log("GunScript found on: " + gunScript.gameObject.name);
                        audioSource.clip = pickupSound;
                        audioSource.volume = 1.0f;
                        audioSource.Play();
                        EquipGun(gunScript.gameObject);
                    }
                    else
                    {
                        Debug.LogError("No GunScript found anywhere on: " + gunObject.name);
                    }
                }
                else
                {
                    Debug.Log("Not a gun. Name: " + hit.collider.gameObject.name + " Tag: " + hit.collider.tag);
                }
            }
            else
            {
                Debug.Log("Ray hit nothing within range: " + pickupRange);
            }
        }
    }

    void EquipGun(GameObject newGun)
    {
        Debug.Log("--- EQUIP START ---");
        Debug.Log("Equipping: " + newGun.name);

        // Drop current gun back into the world
        if (currentGun != null)
        {
            Debug.Log("Dropping current gun: " + currentGun.name);
            currentGun.transform.SetParent(null);

            GunScript oldGunScript = currentGun.GetComponent<GunScript>();
            if (oldGunScript == null)
                oldGunScript = currentGun.GetComponentInChildren<GunScript>();
            if (oldGunScript != null)
                oldGunScript.isEquipped = false;

            Collider col = currentGun.GetComponent<Collider>();
            if (col == null)
                col = currentGun.GetComponentInChildren<Collider>();
            if (col != null)
                col.enabled = true;

            Rigidbody rb = currentGun.GetComponent<Rigidbody>();
            if (rb == null)
                rb = currentGun.AddComponent<Rigidbody>();
            rb.isKinematic = false;
        }

        // Pick up new gun
        currentGun = newGun;

        Rigidbody newRb = currentGun.GetComponent<Rigidbody>();
        if (newRb != null)
            Destroy(newRb);

        Collider newCol = currentGun.GetComponent<Collider>();
        if (newCol == null)
            newCol = currentGun.GetComponentInChildren<Collider>();
        if (newCol != null)
            newCol.enabled = false;

        currentGun.transform.SetParent(gunHolder);
        currentGun.transform.localPosition = Vector3.zero;
        currentGun.transform.localRotation = Quaternion.identity;
        currentGun.transform.localScale = Vector3.one;

        Debug.Log("Parent after: " + (currentGun.transform.parent != null ? currentGun.transform.parent.name : "FAILED"));
        Debug.Log("World pos: " + currentGun.transform.position);

        GunScript gunScript = currentGun.GetComponent<GunScript>();
        if (gunScript == null)
            gunScript = currentGun.GetComponentInChildren<GunScript>();

        if (gunScript != null)
        {
            gunScript.isEquipped = true;
            Debug.Log("GunScript enabled on: " + gunScript.gameObject.name);
        }
        else
        {
            Debug.LogError("No GunScript found after parenting!");
        }

        Debug.Log("--- EQUIP END ---");
    }

    string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}