using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public int rotateSpeed = 50;
    public int healthAmount = 20;
    public AudioClip pickupSFX;
    private AudioSource sfxAudioSource;
    private bool hasBeenTriggered = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && !hasBeenTriggered)
            {
                playerHealth.AddHealth(healthAmount);
                hasBeenTriggered = true;
                sfxAudioSource.clip = pickupSFX;
                sfxAudioSource.volume = 1.0f;
                sfxAudioSource.Play();
                Destroy(gameObject, pickupSFX.length);
            }
        }
    }
}
