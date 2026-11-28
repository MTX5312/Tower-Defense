using UnityEngine;
using Vuforia;

public class WorldAnchor : MonoBehaviour
{
    [Header("Target del Terreno")]
    [SerializeField] private ObserverBehaviour imageTarget;

    [Header("Prefab / Raíz del Nivel")]
    [SerializeField] private GameObject nivelPrefab;
    [SerializeField] private GameObject nivelInstanciado;

    [Header("Ajuste respecto al ImageTarget")]
    [SerializeField] private Vector3 offsetPosicionLocal = Vector3.zero;
    [SerializeField] private Vector3 offsetRotacionLocal = Vector3.zero;
    [SerializeField] private bool bloquearPitchYRoll = true;
    [SerializeField] private bool fijarTrasDetectar = true;

    [Header("Referencias del Nivel")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private Transform terrenoPrincipal;

    private bool terrenoDetectadoUnaVez = false;
    private bool ancladoDefinitivamente = false;

    private void Awake()
    {
        if (imageTarget == null)
        {
            imageTarget = GetComponent<ObserverBehaviour>();
        }
    }

    private void OnEnable()
    {
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

    private void Start()
    {
        if (imageTarget != null && IsTrackedStatus(imageTarget.TargetStatus.Status))
        {
            HandleTerrainDetected();
        }
    }

    private void Update()
    {
        if (imageTarget == null || ancladoDefinitivamente)
            return;

        if (IsTrackedStatus(imageTarget.TargetStatus.Status))
        {
            HandleTerrainDetected();
            PosicionarNivel();

            if (fijarTrasDetectar)
            {
                ancladoDefinitivamente = true;
            }
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (IsTrackedStatus(targetStatus.Status))
        {
            HandleTerrainDetected();

            if (!ancladoDefinitivamente)
            {
                PosicionarNivel();
                if (fijarTrasDetectar)
                {
                    ancladoDefinitivamente = true;
                }
            }
        }
    }

    private bool IsTrackedStatus(Status status)
    {
        return status == Status.TRACKED || status == Status.EXTENDED_TRACKED;
    }

    private void HandleTerrainDetected()
    {
        AsegurarNivel();

        if (!terrenoDetectadoUnaVez)
        {
            terrenoDetectadoUnaVez = true;

            if (enemySpawner != null)
            {
                enemySpawner.NotifyTerrainDetected();
            }
            else
            {
                Debug.LogWarning("WorldAnchor: no se encontró EnemySpawner en el nivel instanciado.");
            }
        }
    }

    private void AsegurarNivel()
    {
        if (nivelInstanciado == null && nivelPrefab != null)
        {
            nivelInstanciado = Instantiate(nivelPrefab);
            nivelInstanciado.name = nivelPrefab.name;
        }

        if (nivelInstanciado == null)
            return;

        RefNivelPrefabs refs = nivelInstanciado.GetComponent<RefNivelPrefabs>();
        if (refs == null)
        {
            refs = nivelInstanciado.GetComponentInChildren<RefNivelPrefabs>();
        }

        if (refs != null)
        {
            if (refs.enemySpawner != null)
                enemySpawner = refs.enemySpawner;

            if (refs.terrenoPrincipal != null)
                terrenoPrincipal = refs.terrenoPrincipal;
        }

        if (enemySpawner == null)
        {
            enemySpawner = nivelInstanciado.GetComponentInChildren<EnemySpawner>();
        }
    }

    private void PosicionarNivel()
    {
        if (nivelInstanciado == null || imageTarget == null)
            return;

        Transform targetTransform = imageTarget.transform;
        nivelInstanciado.transform.position = targetTransform.TransformPoint(offsetPosicionLocal);

        Quaternion rotacionBase = bloquearPitchYRoll
            ? Quaternion.Euler(0f, targetTransform.eulerAngles.y, 0f)
            : targetTransform.rotation;

        nivelInstanciado.transform.rotation = rotacionBase * Quaternion.Euler(offsetRotacionLocal);
    }
}