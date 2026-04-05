using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject inGameUIScreen;
    public GameObject deathScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
 
    }

    public void DeathScreen()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        inGameUIScreen.SetActive(false);
        deathScreen.SetActive(true);
    }

    public void RestartLevel() {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        inGameUIScreen.SetActive(true);
        deathScreen.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
