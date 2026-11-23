using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Nombre")]
    public string label;

    [Tooltip("Enemigo")]
    public GameObject enemyPrefab;

    [Min(1)]
    public int count = 5;

    [Tooltip("Si es menor o igual a 0, usa el intervalo por defecto")]
    [Min(0f)]
    public float customSpawnInterval = 0f;
}

[Serializable]
public class WaveDefinition
{
    [Tooltip("Nombre de oleada")]
    public string waveName = "Wave";

    [Min(0.05f)]
    public float defaultSpawnInterval = 1.5f;

    [Tooltip("Tiempo para siguiente oleada")]
    [Min(0f)]
    public float timeAfterWave = 3f;

    public List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Oleadas")]
    public List<WaveDefinition> waves = new List<WaveDefinition>();

    [Header("Ruta del Camino")]
    public List<Transform> pathWaypoints = new List<Transform>();

    [Header("Castillo")]
    public CastleController castleController;

    private bool waveStarted = false;
    private Coroutine wavesCoroutine;

    public int CurrentWaveIndex { get; private set; } = -1;
    public bool AllWavesFinished { get; private set; } = false;

    private void Awake()
    {
        BuscarCastillo();
    }

    private void BuscarCastillo()
    {
        if (castleController != null)
            return;

        if (pathWaypoints == null || pathWaypoints.Count == 0)
            return;

        Transform finalWaypoint = pathWaypoints[pathWaypoints.Count - 1];

        if (finalWaypoint != null)
        {
            castleController = finalWaypoint.GetComponentInParent<CastleController>();
        }
    }

    public void StartFirstWave()
    {
        if (waveStarted)
            return;

        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("EnemySpawner: no hay oleadas configuradas en la lista 'waves'.");
            return;
        }

        if (pathWaypoints == null || pathWaypoints.Count == 0 || pathWaypoints[0] == null)
        {
            Debug.LogError("EnemySpawner: faltan los waypoints o el primer waypoint.");
            return;
        }

        BuscarCastillo();

        if (castleController == null)
        {
            Debug.LogWarning("No se encontró el CastleController. Asigná el castillo en el Inspector del Spawner.");
        }

        waveStarted = true;
        AllWavesFinished = false;
        wavesCoroutine = StartCoroutine(RunAllWaves());
    }

    private IEnumerator RunAllWaves()
    {
        for (int w = 0; w < waves.Count; w++)
        {
            CurrentWaveIndex = w;
            WaveDefinition currentWave = waves[w];

            if (currentWave == null || currentWave.entries == null || currentWave.entries.Count == 0)
            {
                Debug.LogWarning($"EnemySpawner: la oleada {w + 1} está vacía y se omitirá.");
                continue;
            }

            for (int e = 0; e < currentWave.entries.Count; e++)
            {
                EnemySpawnEntry entry = currentWave.entries[e];
                if (entry == null || entry.enemyPrefab == null || entry.count <= 0)
                    continue;

                float interval = entry.customSpawnInterval > 0f
                    ? entry.customSpawnInterval
                    : Mathf.Max(0.05f, currentWave.defaultSpawnInterval);

                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyPrefab);

                    bool isLastOverall =
                        (w == waves.Count - 1) &&
                        (e == currentWave.entries.Count - 1) &&
                        (i == entry.count - 1);

                    if (!isLastOverall)
                    {
                        yield return new WaitForSeconds(interval);
                    }
                }
            }

            if (w < waves.Count - 1 && currentWave.timeAfterWave > 0f)
            {
                yield return new WaitForSeconds(currentWave.timeAfterWave);
            }
        }

        AllWavesFinished = true;
        wavesCoroutine = null;
        Debug.Log("EnemySpawner: todas las oleadas configuradas han terminado de generarse.");
    }

    private void SpawnEnemy(GameObject enemyPrefabToSpawn)
    {
        if (enemyPrefabToSpawn == null ||
            pathWaypoints == null ||
            pathWaypoints.Count == 0 ||
            pathWaypoints[0] == null)
        {
            Debug.LogWarning("No se pudo generar el enemigo: faltan el prefab o los waypoints.");
            return;
        }

        Vector3 spawnPosition = pathWaypoints[0].position;

        GameObject newEnemy = Instantiate(
            enemyPrefabToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        newEnemy.transform.SetParent(transform, true);

        EnemyController pathScript = newEnemy.GetComponent<EnemyController>();

        if (pathScript != null)
        {
            pathScript.wayPoints = new List<Transform>(pathWaypoints);
            pathScript.castle = castleController;
        }
        else
        {
            Debug.LogWarning("El prefab del enemigo no tiene EnemyController: " + enemyPrefabToSpawn.name);
        }
    }
}