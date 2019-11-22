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

    LivingEntity playerEntity;
    Transform playerTransform;

    int enemiesRemainingToSpawn;
    float nextSpawnTime;
    int enemiesRemaining;

    public Wave[] waves;
    public Enemy enemy;
    MapGenerator map;

    float timeBetweenCampingChecks = 2;
    float campThresholdDistance = 1.5f;
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    public event System.Action<int> OnNewWave;

    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerTransform = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerTransform.position;

        map = FindObjectOfType<MapGenerator>();
        NextWave();
        playerEntity.OnDead += OnPlayerDeath;
    }

    void NextWave()
    {
        currentWaveNumber++;
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemaining = currentWave.enemyCount;

            if (OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
        }
    }

    void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerTransform.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerTransform.position;
            }
        }

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

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping && !isDisabled)
        {
            spawnTile = map.GetTileFromPosition(playerTransform.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColour = tileMat.color;
        Color flashClour = Color.red;
        float spawnTimer = 0;
        
        
        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColour, flashClour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDead += OnEnemyDeath;

    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    void OnEnemyDeath()
    {
        enemiesRemaining--;
        if (enemiesRemaining <= 0)
            NextWave();
    }


}
