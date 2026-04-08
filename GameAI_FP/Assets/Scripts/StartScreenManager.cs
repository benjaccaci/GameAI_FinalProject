using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
