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
    }
    
    public Wave[] waves;
    public int timeBetweenWaves = 5;
    public TMP_Text waveText;
    private Spawner[] zombieSpawners;
    private int currentWaveIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentWaveIndex = 0;
        zombieSpawners = FindObjectsByType<Spawner>(FindObjectsSortMode.None);

        StartCoroutine(SpawnWave());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnWave() {
        if (currentWaveIndex >= waves.Length) {
            Debug.Log("All waves completed!");
            yield break;
        }

        while (currentWaveIndex < waves.Length) {
            Wave currentWave = waves[currentWaveIndex];
            UpdateWaveText();
            // Wait constant time before starting new wave
            yield return new WaitForSeconds(timeBetweenWaves);
            // Spawn enemies in the wave
            yield return StartCoroutine(SpawnEnemies(waves[currentWaveIndex]));
            // Wait until all enemies are dead to continue
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

            currentWaveIndex++;
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
            waveText.text = (currentWaveIndex + 1).ToString() + " / " + waves.Length;
        }
    }
}
