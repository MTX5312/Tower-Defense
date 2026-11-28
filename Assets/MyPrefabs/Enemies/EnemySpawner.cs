using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Nombre descriptivo opcional para leer mejor en el Inspector")]
    public string label;

    [Tooltip("Prefab del enemigo (debe tener EnemyController)")]
    public GameObject enemyPrefab;

    [Min(1)]
    public int count = 5;

    [Tooltip("Si es menor o igual a 0, usa el intervalo por defecto de la oleada")]
    [Min(0f)]
    public float customSpawnInterval = 0f;
}

[Serializable]
public class WaveDefinition
{
    [Tooltip("Nombre opcional de la oleada, ej: Ronda 1")]
    public string waveName = "Ronda";

    [Min(0.05f)]
    public float defaultSpawnInterval = 1.5f;

    [Tooltip("Tiempo de espera al terminar esta ronda antes de que empiece la siguiente")]
    [Min(0f)]
    public float timeAfterWave = 5f;

    public List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Inicio por Terreno AR")]
    [Tooltip("Si está activo, el contador no arranca hasta que WorldAnchor avise que se detectó el ImageTarget del terreno")]
    public bool iniciarAlDetectarTerreno = true;

    [Tooltip("Texto mostrado mientras se espera detectar el terreno")]
    public string textoEsperandoTerreno = "APUNTA AL TERRENO";

    [Header("Configuración de Rondas")]
    public List<WaveDefinition> waves = new List<WaveDefinition>();

    [Tooltip("Segundos de preparación antes de que comience la Ronda 1")]
    [Min(0f)]
    public float tiempoAntesPrimeraRonda = 20f;

    [Header("Condición de Fin de Ronda")]
    [Tooltip("Si está activo, espera a que mueran todos los enemigos antes de contar hacia la siguiente ronda")]
    public bool esperarAQueMueranEnemigos = true;

    [Header("UI Temporizador de Ronda")]
    [Tooltip("Texto TMP que muestra 'RONDA EN: X' y luego 'RONDA X'")]
    public TMP_Text roundTimerText;

    [Tooltip("Si está activo, muestra décimas de segundo (ej: RONDA EN: 19.4s). Si no, segundos enteros (ej: RONDA EN: 19s)")]
    public bool mostrarDecimas = true;

    [Header("Ruta del Camino")]
    public List<Transform> pathWaypoints = new List<Transform>();

    [Header("Castillo")]
    public CastleController castleController;

    private bool waveStarted = false;
    private Coroutine wavesCoroutine;
    private readonly List<EnemyController> enemigosGenerados = new List<EnemyController>();

    public int CurrentWaveIndex { get; private set; } = -1;
    public bool AllWavesFinished { get; private set; } = false;

    private void Awake()
    {
        BuscarCastillo();
    }

    private void Start()
    {
        if (!iniciarAlDetectarTerreno)
        {
            StartFirstWave();
        }
        else if (roundTimerText != null && !waveStarted)
        {
            roundTimerText.text = textoEsperandoTerreno;
        }
    }

    /// <summary>
    /// Llamado por WorldAnchor cuando se detecta el ImageTarget del terreno.
    /// Inicia el contador de preparación una sola vez.
    /// </summary>
    public void NotifyTerrainDetected()
    {
        if (waveStarted)
            return;

        StartFirstWave();
    }

    private void BuscarCastillo()
    {
        if (castleController == null && pathWaypoints != null && pathWaypoints.Count > 0)
        {
            Transform finalWaypoint = pathWaypoints[pathWaypoints.Count - 1];
            if (finalWaypoint != null)
            {
                castleController = finalWaypoint.GetComponentInParent<CastleController>();
            }
        }

        if (castleController == null)
        {
            castleController = FindAnyObjectByType<CastleController>();
        }
    }

    public void StartFirstWave()
    {
        if (waveStarted)
            return;

        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("EnemySpawner: no hay rondas configuradas en la lista 'waves'.");
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
            Debug.LogWarning("EnemySpawner: no se encontró el CastleController. Asignalo en el Inspector.");
        }

        waveStarted = true;
        AllWavesFinished = false;
        wavesCoroutine = StartCoroutine(RunAllWaves());
    }

    private IEnumerator RunAllWaves()
    {
        if (tiempoAntesPrimeraRonda > 0f)
        {
            yield return StartCoroutine(CuentaRegresivaRonda(tiempoAntesPrimeraRonda));
        }

        for (int w = 0; w < waves.Count; w++)
        {
            if (w > 0 && waves[w - 1] != null && waves[w - 1].timeAfterWave > 0f)
            {
                yield return StartCoroutine(CuentaRegresivaRonda(waves[w - 1].timeAfterWave));
            }

            CurrentWaveIndex = w;
            MostrarTituloRonda(w + 1);

            WaveDefinition currentWave = waves[w];

            if (currentWave == null || currentWave.entries == null || currentWave.entries.Count == 0)
            {
                Debug.LogWarning($"EnemySpawner: la ronda {w + 1} está vacía y se omitirá.");
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

                    bool isLastInWave =
                        (e == currentWave.entries.Count - 1) &&
                        (i == entry.count - 1);

                    if (!isLastInWave)
                    {
                        yield return new WaitForSeconds(interval);
                    }
                }
            }

            if (esperarAQueMueranEnemigos)
            {
                while (HayEnemigosVivos())
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }

        if (roundTimerText != null)
        {
            roundTimerText.text = "RONDAS COMPLETADAS";
        }

        AllWavesFinished = true;
        wavesCoroutine = null;
        Debug.Log("EnemySpawner: todas las rondas han terminado.");
    }

    private IEnumerator CuentaRegresivaRonda(float duracion)
    {
        float restante = duracion;

        while (restante > 0f)
        {
            ActualizarTextoCuentaRegresiva(restante);
            yield return null;
            restante -= Time.deltaTime;
        }

        ActualizarTextoCuentaRegresiva(0f);
    }

    private void ActualizarTextoCuentaRegresiva(float segundosRestantes)
    {
        if (roundTimerText == null)
            return;

        float valor = Mathf.Max(0f, segundosRestantes);
        string tiempoFormateado = mostrarDecimas
            ? $"{valor:0.0}s"
            : $"{Mathf.CeilToInt(valor)}s";

        roundTimerText.text = $"RONDA EN: {tiempoFormateado}";
    }

    private void MostrarTituloRonda(int numeroRonda)
    {
        if (roundTimerText == null)
            return;

        roundTimerText.text = $"RONDA {numeroRonda}";
    }

    private bool HayEnemigosVivos()
    {
        for (int i = enemigosGenerados.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = enemigosGenerados[i];
            if (enemy == null || enemy.IsDead)
            {
                enemigosGenerados.RemoveAt(i);
            }
        }

        return enemigosGenerados.Count > 0;
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
            enemigosGenerados.Add(pathScript);
        }
        else
        {
            Debug.LogWarning("El prefab del enemigo no tiene EnemyController: " + enemyPrefabToSpawn.name);
        }
    }
}