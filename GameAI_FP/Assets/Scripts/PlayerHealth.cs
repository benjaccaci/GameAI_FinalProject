using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float playerHealth;
    public Slider healthSlider;
    public Image bloodSplatterEffect;
    public Color bloodSplatterColor;
    public AudioClip damageSFX;
    public int maxHealth = 100;
    private AudioSource sfxAudioSource;
    private LevelManager levelManager;

    void Start()
    {
        if (gameObject.CompareTag("Player"))
        {
            playerHealth = 100;
        }
        UpdateSlider();
        levelManager = FindFirstObjectByType<LevelManager>();
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;
    }

    public void TakeDamage(float damage)
    {
        playerHealth -= damage;
        Debug.Log("Player took " + damage + " damage. HP: " + playerHealth + "/" + maxHealth);
        if (playerHealth <= 0)
        {
            Die();
        }
        UpdateSlider();

    
        sfxAudioSource.clip = damageSFX;
        sfxAudioSource.volume = 1.0f;
        sfxAudioSource.Play();

        bloodSplatterEffect.CrossFadeAlpha(1f, 0f, false);
        bloodSplatterEffect.color = bloodSplatterColor;
        bloodSplatterEffect.CrossFadeAlpha(0f, 0.3f, false);
    }

    public void AddHealth(int health) {
        playerHealth += health;
        if (playerHealth >= maxHealth) {
            playerHealth = maxHealth;
        }
        UpdateSlider();
    }
    
    void Die()
    {
        Debug.Log("Player died");
        // Destroy(gameObject);
        Invoke("ShowDeathScreen", 1.5f);
    }

    void ShowDeathScreen()
    {
    levelManager.DeathScreen();
    }

    void UpdateSlider() {
        if (healthSlider) {
            healthSlider.value = playerHealth;
        }
    }
}
