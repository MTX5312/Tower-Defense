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

    void Start()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < totalEnemies; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (pathWaypoints == null || pathWaypoints.Count == 0 || enemyPrefab == null)
        {
            Debug.LogWarning("Faltan los Waypoints o el Prefab en el Spawner.");
            return;
        }

        Vector3 spawnPosition = pathWaypoints[0].position;
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        newEnemy.transform.SetParent(transform, true);

        EnemyController pathScript = newEnemy.GetComponent<EnemyController>();
        if (pathScript != null)
        {
            pathScript.wayPoints = new List<Transform>(pathWaypoints);
        }
    }
}