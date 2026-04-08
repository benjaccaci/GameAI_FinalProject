using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    public AudioClip audioClip;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioClip;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainLevel");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
