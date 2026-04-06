using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject inGameUIScreen;
    public GameObject deathScreen;
    public GameObject gameWonScreen;
    public AudioClip gameWonSFX;
    public AudioClip deathSFX;
    private AudioSource sfxAudioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sfxAudioSource = GetComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;
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

        sfxAudioSource.clip = deathSFX;
        sfxAudioSource.volume = 1.0f;
        sfxAudioSource.Play();
    }

    public void GameWonScreen()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        inGameUIScreen.SetActive(false);
        gameWonScreen.SetActive(true);

        sfxAudioSource.clip = gameWonSFX;
        sfxAudioSource.volume = 1.0f;
        sfxAudioSource.Play();
    }

    public void RestartLevel() {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        inGameUIScreen.SetActive(true);
        deathScreen.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
