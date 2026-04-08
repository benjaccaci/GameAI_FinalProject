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
    private PlayerMoney playerMoney;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        playerMoney = GetComponent<PlayerMoney>();

        if (gunHolder == null)
        {
            GameObject holder = GameObject.FindGameObjectWithTag("GunHolder");
            if (holder != null)
                gunHolder = holder.transform;
            else
                Debug.LogError("No object with tag 'GunHolder' found!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, pickupRange))
            {
                if (hit.collider.CompareTag("Gun"))
                {
                    GameObject gunObject = hit.collider.gameObject;

                    GunScript gunScript = gunObject.GetComponent<GunScript>();
                    if (gunScript == null)
                        gunScript = gunObject.GetComponentInParent<GunScript>();

                    if (gunScript != null)
                    {
                        // Check price
                        GunPrice gunPrice = gunScript.GetComponent<GunPrice>();
                        if (gunPrice == null)
                            gunPrice = gunScript.GetComponentInChildren<GunPrice>();

                        if (gunPrice != null && gunPrice.price > 0)
                        {
                            if (playerMoney != null && playerMoney.SpendMoney(gunPrice.price))
                            {
                                Debug.Log("Bought " + gunScript.gameObject.name + " for $" + gunPrice.price);
                                audioSource.clip = pickupSound;
                                audioSource.volume = 1.0f;
                                audioSource.Play();
                                EquipGun(gunScript.gameObject);
                            }
                            else
                            {
                                Debug.Log("Not enough money! Need $" + gunPrice.price);
                            }
                        }
                        else
                        {
                            // Free gun, just pick it up
                            audioSource.clip = pickupSound;
                            audioSource.volume = 1.0f;
                            audioSource.Play();
                            EquipGun(gunScript.gameObject);
                        }
                    }
                    else
                    {
                        Debug.LogError("No GunScript found on: " + gunObject.name);
                    }
                }
            }
        }
    }

    void EquipGun(GameObject newGun)
    {
        if (currentGun != null)
        {
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

        GunScript gunScript = currentGun.GetComponent<GunScript>();
        if (gunScript == null)
            gunScript = currentGun.GetComponentInChildren<GunScript>();

        if (gunScript != null)
            gunScript.isEquipped = true;
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