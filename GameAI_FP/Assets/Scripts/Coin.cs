using UnityEngine;

public class Coin : MonoBehaviour
{
    public int rotateSpeed = 50;
    public int moneyAmount = 20;
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
            PlayerMoney playerMoney = other.GetComponent<PlayerMoney>();
            if (playerMoney != null && !hasBeenTriggered)
            {
                playerMoney.AddMoney(moneyAmount);
                hasBeenTriggered = true;
                sfxAudioSource.clip = pickupSFX;
                sfxAudioSource.volume = 1.0f;
                sfxAudioSource.Play();
                Destroy(gameObject, pickupSFX.length);
            }
        }
    }
}
