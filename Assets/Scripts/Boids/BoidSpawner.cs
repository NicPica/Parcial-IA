using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoidSpawner : MonoBehaviour
{
    public static BoidSpawner Instance { get; private set; }

    [SerializeField] private Boid boidPrefab;
    [SerializeField] private int initialBoidCount = 6;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);
    [SerializeField] private float respawnDelay = 3f;

    private readonly List<Boid> allBoids = new List<Boid>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < initialBoidCount; i++)
        {
            Vector3 pos = GetRandomPositionInArea();
            Boid boid = Instantiate(boidPrefab, pos, Quaternion.identity, transform);
            allBoids.Add(boid);
        }
    }

    private Vector3 GetRandomPositionInArea()
    {
        return transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            0f,
            Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
        );
    }

    public void ScheduleRespawn(Boid boid)
    {
        StartCoroutine(RespawnRoutine(boid));
    }

    private IEnumerator RespawnRoutine(Boid boid)
    {
        yield return new WaitForSeconds(respawnDelay);

        boid.transform.position = GetRandomPositionInArea();
        boid.ResetState();
        boid.gameObject.SetActive(true);
    }
}