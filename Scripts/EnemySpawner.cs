using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public Transform   player;
    public float       activationRadius = 10f;
    public GameObject[] enemyPrefabs;
    public float       spawnInterval = 1f;
    [HideInInspector]  public int totalEnemies = 10;

    [Header("Wave Settings")]
    public bool   useWaves = false;
    public int[]  enemiesPerWave;
    public float  waveDelay = 3f;

    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Transform  bossSpawnPoint;

    private static EnemySpawner activeSpawner = null; 

    private readonly List<GameObject> spawned = new();
    private int   currentWave = 0;
    private bool  spawning    = false;
    private bool  bossSpawned = false;

    void OnValidate()
    {
        if (useWaves && enemiesPerWave != null && enemiesPerWave.Length > 0)
        {
            totalEnemies = 0;
            foreach (int c in enemiesPerWave) totalEnemies += c;
        }
    }

    void Awake()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (spawning) return;
        if (player == null) return;
        if (Vector2.Distance(player.position, transform.position) > activationRadius) return;

        // ensure only one spawner runs at a time
        if (activeSpawner == null)
        {
            activeSpawner = this;
            spawning = true;

            if (useWaves && enemiesPerWave.Length > 0)
                StartCoroutine(SpawnWave(currentWave));
            else
                StartCoroutine(SpawnFreeMode());
        }
    }

    IEnumerator SpawnFreeMode()
    {
        for (int i = 0; i < totalEnemies; i++)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }

        yield return WaitUntilAllLocalEnemiesDead();

        if (!bossSpawned && bossPrefab != null)
            SpawnBoss();

        yield return WaitUntilAllLocalEnemiesDead();
        FinishSpawner();
    }

    IEnumerator SpawnWave(int waveIdx)
    {
        int count = enemiesPerWave[waveIdx];

        for (int i = 0; i < count; i++)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }

        yield return WaitUntilAllLocalEnemiesDead();

        currentWave++;

        if (currentWave < enemiesPerWave.Length)
        {
            yield return new WaitForSeconds(waveDelay);
            StartCoroutine(SpawnWave(currentWave));
        }
        else
        {
            if (!bossSpawned && bossPrefab != null)
                SpawnBoss();

            yield return WaitUntilAllLocalEnemiesDead();
            FinishSpawner();
        }
    }

    IEnumerator WaitUntilAllLocalEnemiesDead()
    {
        while (true)
        {
            spawned.RemoveAll(e => e == null);
            if (spawned.Count == 0) break;
            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector2 pos = (Vector2)transform.position
                    + Random.insideUnitCircle.normalized * 2f;

        GameObject e = Instantiate(prefab, pos, Quaternion.identity);
        spawned.Add(e);
        e.AddComponent<SpawnedBy>().Init(this);

        if (e.TryGetComponent<EnemyBase>(out var eb))
            eb.SetPlayer(player);
    }

    void SpawnBoss()
    {
        bossSpawned = true;
        Vector2 pos = bossSpawnPoint ? bossSpawnPoint.position : transform.position;

        GameObject b = Instantiate(bossPrefab, pos, Quaternion.identity);
        spawned.Add(b);
        b.AddComponent<SpawnedBy>().Init(this);

        if (b.TryGetComponent<EnemyBase>(out var eb))
            eb.SetPlayer(player);
    }

    void FinishSpawner()
    {
        activeSpawner = null;
        Destroy(gameObject);
    }

    public void Deregister(GameObject go) => spawned.Remove(go);
}

public class SpawnedBy : MonoBehaviour
{
    private EnemySpawner owner;
    public void Init(EnemySpawner sp) => owner = sp;
    void OnDestroy() { owner?.Deregister(gameObject); }
}