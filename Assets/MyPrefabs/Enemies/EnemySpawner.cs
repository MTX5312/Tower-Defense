using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Oleada")]
    public GameObject enemyPrefab;
    public float spawnInterval = 1.5f;
    public int totalEnemies = 10;

    [Header("Ruta del Camino")]
    public List<Transform> pathWaypoints = new List<Transform>();

    [Header("Castillo")]
    public CastleController castleController;

    private bool waveStarted = false;
    private Coroutine spawnCoroutine;

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

        Transform finalWaypoint =
            pathWaypoints[pathWaypoints.Count - 1];

        if (finalWaypoint != null)
        {
            castleController =
                finalWaypoint.GetComponentInParent<CastleController>();
        }
    }

    public void StartFirstWave()
    {
        if (waveStarted)
            return;

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: falta asignar el prefab del enemigo.");
            return;
        }

        if (pathWaypoints == null ||
            pathWaypoints.Count == 0 ||
            pathWaypoints[0] == null)
        {
            Debug.LogError("EnemySpawner: faltan los waypoints o el primer waypoint.");
            return;
        }

        BuscarCastillo();

        if (castleController == null)
        {
            Debug.LogWarning("No se encontró el CastleController. " + "Asigná el castillo en el Inspector del Spawner.");
        }

        waveStarted = true;
        spawnCoroutine = StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < totalEnemies; i++)
        {
            SpawnEnemy();

            if (i < totalEnemies - 1)
                yield return new WaitForSeconds(spawnInterval);
        }

        spawnCoroutine = null;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null ||
            pathWaypoints == null ||
            pathWaypoints.Count == 0 ||
            pathWaypoints[0] == null)
        {
            Debug.LogWarning("No se pudo generar el enemigo: " + "faltan el prefab o los waypoints.");
            return;
        }

        Vector3 spawnPosition = pathWaypoints[0].position;

        GameObject newEnemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        newEnemy.transform.SetParent(transform, true);

        EnemyController pathScript =
            newEnemy.GetComponent<EnemyController>();

        if (pathScript != null)
        {
            pathScript.wayPoints =
                new List<Transform>(pathWaypoints);

            pathScript.castle = castleController;
        }
        else
        {
            Debug.LogWarning(
                "El prefab del enemigo no tiene EnemyController."
            );
        }
    }
}