using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave {
        public GameObject[] enemyPrefabs;
        public int enemyCount = 5;
        public float spawnInterval = 1f;
        public float waveDuration = 60f;
    }
    
    public Wave[] waves;
    public int timeBetweenWaves = 5;
    public AudioClip waveSpawnSFX;
    public TMP_Text waveText;
    public TMP_Text timerText;
    public TMP_Text newWaveText;
    public LevelManager levelManager;
    private Spawner[] zombieSpawners;
    private int currentWaveIndex = 0;
    private float waveTimer = 0f;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentWaveIndex = 0;
        zombieSpawners = FindObjectsByType<Spawner>(FindObjectsSortMode.None);
        timerText.text = "00:00";
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        StartCoroutine(SpawnWave());
    }

    // Update is called once per frame
    void Update()
    {
        if (waveTimer > 0) {
            waveTimer -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(waveTimer / 60);
            int seconds = Mathf.FloorToInt(waveTimer % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        } else {
            waveTimer = 0f;
        }
    }

    IEnumerator SpawnWave() {
        if (currentWaveIndex >= waves.Length) {
            Debug.Log("All waves completed!");
            levelManager.GameWonScreen();
            yield break;
        }

        while (currentWaveIndex < waves.Length) {
            // Wait constant time before starting new wave
            yield return new WaitForSeconds(timeBetweenWaves);

            Wave currentWave = waves[currentWaveIndex];
            UpdateWaveText();
            waveTimer = waves[currentWaveIndex].waveDuration;

            audioSource.clip = waveSpawnSFX;
            audioSource.volume = 1.0f;
            audioSource.Play();

            StartCoroutine(ShowNewWaveText());

            // Spawn enemies in the wave
            yield return StartCoroutine(SpawnEnemies(waves[currentWaveIndex]));
            // Wait until all enemies are dead to continue
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0 || waveTimer <= 0f);

            // If the wave ends before all enemies are dead, kill remaining enemies
            if (GameObject.FindGameObjectsWithTag("Enemy").Length > 0) {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                for (int i = 0; i < enemies.Length; i++) {
                    ZombieController zombie = enemies[i].GetComponent<ZombieController>();
                    if (zombie != null)
                    {
                        zombie.TakeDamage(1000f);
                    }
                }
            }

            currentWaveIndex++;
        }

        if (currentWaveIndex >= waves.Length) {
            levelManager.GameWonScreen();
        }
    }

    IEnumerator SpawnEnemies(Wave wave) {
        for (int i = 0; i < wave.enemyCount; i++) {
            Spawner spawner = zombieSpawners[Random.Range(0, zombieSpawners.Length)];
            GameObject enemyPrefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];
            SpawnEnemy(enemyPrefab, spawner.gameObject);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    void SpawnEnemy(GameObject enemyPrefab, GameObject spawner) {
        Instantiate(enemyPrefab, spawner.transform.position, spawner.transform.rotation);
    }


    void UpdateWaveText() {
        if (waveText) {
            waveText.text = "Wave: " + (currentWaveIndex + 1).ToString() + " / " + waves.Length;
        }
    }

    IEnumerator ShowNewWaveText() {
        newWaveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        newWaveText.gameObject.SetActive(false);
    }
}
