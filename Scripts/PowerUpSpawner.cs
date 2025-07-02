using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] powerUpPrefabs;

    [Header("Spawn Settings")]
    public Transform player;
    public float activationRadius = 10f;

    public float spawnRadius    = 3f;
    public float spawnInterval  = 10f;
    public int   maxActive      = 3;
    public LayerMask obstacleMask;

    private readonly List<GameObject> active = new();

    void Awake()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Start() => StartCoroutine(SpawnLoop());

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (PlayerInRange() && active.Count < maxActive)
                TrySpawnOne();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    bool PlayerInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(player.position, transform.position) <= activationRadius;
    }

    void TrySpawnOne()
    {
        if (powerUpPrefabs.Length == 0) return;

        Vector2 candidatePos = (Vector2)transform.position +
                               Random.insideUnitCircle.normalized * Random.Range(0.5f, spawnRadius);

        if (Physics2D.OverlapCircle(candidatePos, 0.3f, obstacleMask)) return;

        GameObject prefab   = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        GameObject instance = Instantiate(prefab, candidatePos, Quaternion.Euler(-90,0,0));

        if (instance.TryGetComponent(out PowerUpPickup pup))
            pup.SetOwningSpawner(this);

        active.Add(instance);
    }

    public void Deregister(GameObject go) => active.Remove(go);
}