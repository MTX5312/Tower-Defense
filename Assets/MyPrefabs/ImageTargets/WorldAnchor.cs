using UnityEngine;
using Vuforia;

public class WorldAnchor : MonoBehaviour
{
    [Header("Prefab completo del nivel")]
    [SerializeField] private GameObject nivelPrefab;

    [Header("Ajuste respecto al ImageTarget")]
    [SerializeField] private Vector3 offsetPosicionLocal = Vector3.zero;
    [SerializeField] private Vector3 offsetRotacionLocal = Vector3.zero;

    [Header("Comportamiento")]
    [SerializeField] private bool desactivarImageTargetDespuesDeGenerar = true;

    private ObserverBehaviour imageTarget;
    private GameObject nivelInstanciado;
    private bool escenarioGenerado = false;

    private void Awake()
    {
        imageTarget = GetComponent<ObserverBehaviour>();
    }

    private void OnEnable()
    {
        if (imageTarget == null)
            imageTarget = GetComponent<ObserverBehaviour>();

        if (imageTarget != null)
        {
            imageTarget.OnTargetStatusChanged += OnTargetStatusChanged;
        }
        else
        {
            Debug.LogError("WorldAnchor necesita un ObserverBehaviour " + "en el mismo GameObject.");
        }
    }

    private void OnDisable()
    {
        if (imageTarget != null)
        {
            imageTarget.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(
        ObserverBehaviour behaviour,
        TargetStatus targetStatus)
    {
        if (escenarioGenerado)
            return;

        bool imagenReconocida =
            targetStatus.Status == Status.TRACKED ||
            targetStatus.Status == Status.EXTENDED_TRACKED;

        if (!imagenReconocida)
            return;

        GenerarEscenario();
    }

    private void GenerarEscenario()
    {
        if (nivelPrefab == null)
        {
            Debug.LogError(
                "WorldAnchor: falta asignar el prefab NivelCompleto."
            );
            return;
        }

        Vector3 posicionInicial =
            transform.position +
            transform.rotation * offsetPosicionLocal;

        Quaternion rotacionInicial =
            transform.rotation *
            Quaternion.Euler(offsetRotacionLocal);

        nivelInstanciado = Instantiate(nivelPrefab, posicionInicial, rotacionInicial);

        if (nivelInstanciado == null)
        {
            Debug.LogError("No se pudo generar el escenario.");
            return;
        }

        escenarioGenerado = true;

        RefNivelPrefabs referencias = nivelInstanciado.GetComponent<RefNivelPrefabs>();

        Transform terrenoParaTorres = nivelInstanciado.transform;

        EnemySpawner spawner = null;

        if (referencias != null)
        {
            if (referencias.terrenoPrincipal != null)
                terrenoParaTorres = referencias.terrenoPrincipal;

            spawner = referencias.enemySpawner;
        }

        if (spawner == null)
        {
            spawner =
                nivelInstanciado.GetComponentInChildren<EnemySpawner>(true);
        }

        TowerBuilder[] constructores = FindObjectsByType<TowerBuilder>(FindObjectsSortMode.None);

        foreach (TowerBuilder constructor in constructores)
        {
            constructor.terrenoPrincipal = terrenoParaTorres;
        }

        Debug.Log("Escenario generado y referencias de torres configuradas.");

        if (spawner != null)
        {
            spawner.StartFirstWave();
        }
        else
        {
            Debug.LogError(
                "El nivel fue generado, pero no se encontró EnemySpawner."
            );
        }

        // El escenario ya está fuera de la jerarquía del ImageTarget.
        if (desactivarImageTargetDespuesDeGenerar)
        {
            gameObject.SetActive(false);
        }
    }
}