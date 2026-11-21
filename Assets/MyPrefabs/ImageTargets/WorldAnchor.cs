using UnityEngine;
using Vuforia;

public class WorldAnchor : MonoBehaviour
{
    [Header("Prefab completo del nivel")]
    [SerializeField] private GameObject nivelPrefab;

    [Header("Ajuste respecto al ImageTarget")]
    [SerializeField] private Vector3 offsetPosicionLocal = Vector3.zero;
    [SerializeField] private Vector3 offsetRotacionLocal = Vector3.zero;

    private ObserverBehaviour imageTarget;
    private GameObject nivelInstanciado;
    private AnchorBehaviour anclajeVuforia;
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
            return;
        }

        if (VuforiaBehaviour.Instance == null)
        {
            return;
        }

        Vector3 posicionInicial =
            transform.position +
            transform.rotation * offsetPosicionLocal;

        Quaternion rotacionInicial =
            transform.rotation *
            Quaternion.Euler(offsetRotacionLocal);

        anclajeVuforia =
            VuforiaBehaviour.Instance.ObserverFactory
                .CreateAnchorBehaviour(
                    "PocketSiege_NivelAnchor",
                    posicionInicial,
                    rotacionInicial
                );

        if (anclajeVuforia == null)
        {
            return;
        }

        nivelInstanciado = Instantiate(
            nivelPrefab,
            posicionInicial,
            rotacionInicial
        );

        if (nivelInstanciado == null)
        {
            anclajeVuforia.UnconfigureAnchor();
            anclajeVuforia = null;

            return;
        }

        nivelInstanciado.transform.SetParent(anclajeVuforia.transform, true);

        escenarioGenerado = true;

        RefNivelPrefabs referencias = nivelInstanciado.GetComponent<RefNivelPrefabs>();

        Transform terrenoParaTorres = nivelInstanciado.transform;
        EnemySpawner spawner = null;

        if (referencias != null)
        {
            if (referencias.terrenoPrincipal != null) terrenoParaTorres = referencias.terrenoPrincipal;

            spawner = referencias.enemySpawner;
        }

        if (spawner == null)
        {
            spawner = nivelInstanciado.GetComponentInChildren<EnemySpawner>(true);
        }

        TowerBuilder[] constructores = FindObjectsByType<TowerBuilder>(FindObjectsSortMode.None);

        foreach (TowerBuilder constructor in constructores)
        {
            constructor.terrenoPrincipal = terrenoParaTorres;
        }

        if (spawner != null)
        {
            spawner.StartFirstWave();
        }
    }
}