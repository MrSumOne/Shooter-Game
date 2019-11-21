using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
    }

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    float nextSpawnTime;
    int enemiesRemaining;

    public Wave[] waves;
    public Enemy enemy;
    MapGenerator map;

    private void Start()
    {
        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void NextWave()
    {
        currentWaveNumber++;
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemaining = currentWave.enemyCount;
        }
    }

    private void Update()
    {
        if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
        {
            enemiesRemainingToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

            StartCoroutine(SpawnEnemy());
        }
    
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform randomTile = map.GetRandomOpenTile();
        Material tileMat = randomTile.GetComponent<Renderer>().material;
        Color initialColour = tileMat.color;
        Color flashClour = Color.red;
        float spawnTimer = 0;
        
        
        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColour, flashClour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            Debug.Log(tileMat.color);
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDead += OnEnemyDeath;

    }

    void OnEnemyDeath()
    {
        enemiesRemaining--;
        if (enemiesRemaining <= 0)
            NextWave();
    }


}
