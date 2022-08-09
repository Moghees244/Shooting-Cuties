using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;
    LivingEntity playerEntity;
    Transform PlayerT;
    Wave currentWave;
    int currentWaveNumber;
    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;
    float timeBetweenCampingChecks = 2;
    float nextCampingCheckTime;
    float campThresholdDistance = 1.5f;
    Vector3 campingPositionOld;
    bool isCamping;
    bool isDisabled;
    public event System.Action<int> onNewWave;
    MapGenerator map;

    void Start()
    {
        playerEntity = FindObjectOfType<PlayerController>();
        PlayerT = playerEntity.transform;

        map = FindObjectOfType<MapGenerator>();
        nextCampingCheckTime = timeBetweenCampingChecks + Time.time;
        campingPositionOld = PlayerT.position;
        playerEntity.OnDeath += onPlayerDeath;

        nextWave();
    }

    private void Update()
    {
        if (!isDisabled)
        {
            if (Time.time >= nextCampingCheckTime)
            {
                nextCampingCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping =
                    (Vector3.Distance(PlayerT.position, campingPositionOld))
                    < campThresholdDistance;
                campingPositionOld = PlayerT.position;
            }

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.SpawnTimeDiff;

                StartCoroutine(SpawnEnemy());
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform randomTile = map.getRandomOpenTile();

        if (isCamping)
        {
            randomTile = map.getTileFromPosition(PlayerT.position);
        }

        Material tileMaterial = randomTile.GetComponent<Renderer>().material;
        Color initialColor = tileMaterial.color;

        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMaterial.color = Color.Lerp(
                initialColor,
                flashColor,
                Mathf.PingPong(spawnTimer * tileFlashSpeed, 1)
            );

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spwanedEnemy =
            Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spwanedEnemy.OnDeath += OnEnemyDeath;

        spwanedEnemy.setCharacteristics(
            currentWave.moveSpeed,
            currentWave.hitsToKill,
            currentWave.enemyHealth,
            currentWave.skinColor
        );
    }

    void onPlayerDeath()
    {
        isDisabled = true;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;

        if (enemiesRemainingAlive == 0)
        {
            playerEntity.resetHealth();
            nextWave();
        }
    }

    void resetPlayerPosition()
    {
        PlayerT.position = map.getTileFromPosition(Vector3.zero).position + Vector3.up * 3;
        playerEntity.startingHealth = 100;
    }

    void nextWave()
    {
        if (currentWaveNumber == 5)
        {
            SceneManager.LoadScene("Win");
        }

        if (currentWaveNumber > 0)
            AudioManager.instance.playSound("LevelComplete", Vector3.zero);

        currentWaveNumber++;

        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (onNewWave != null)
            {
                onNewWave(currentWaveNumber);
            }

            resetPlayerPosition();
        }
    }

    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float SpawnTimeDiff;

        public float moveSpeed;
        public int hitsToKill;
        public float enemyHealth;
        public Color skinColor;
    }
}
