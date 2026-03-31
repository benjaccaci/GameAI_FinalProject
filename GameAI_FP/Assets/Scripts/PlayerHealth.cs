using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float playerHealth;
    public Slider healthSlider;
    int maxHealth = 100;

    void Start()
    {
        if (gameObject.CompareTag("Player"))
        {
            playerHealth = 100;
        }
        UpdateSlider();
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
        Destroy(gameObject);
        // LevelManager.instance.RestartLevel();
    }

    void UpdateSlider() {
        if (healthSlider) {
            healthSlider.value = playerHealth;
        }
    }
}
