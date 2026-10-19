using UnityEngine;
using Vuforia;

public class TowerBuilder : MonoBehaviour
{
    [Header("Construcción Base")]
    public GameObject torrePrefab;
    public Transform terrenoPrincipal;

    [Header("Detección de Colisión")]
    public float radioDeteccion = 0.2f;
    public LayerMask capaTorres;

    [Header("Sistema de Cooldown")]
    public float cooldownTime = 5f;
    private float nextBuildTime = 0f;
    public float CooldownDuration => Mathf.Max(0f, cooldownTime);
    public float CooldownRemaining => Mathf.Max(0f, nextBuildTime - Time.time);
    public bool IsOnCooldown => CooldownRemaining > 0;

    private ObserverBehaviour mObserverBehaviour;

    void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();
        if (mObserverBehaviour != null)
        {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            IntentarColocarTorre();
        }
    }

    void IntentarColocarTorre()
    {
        if (Time.time < nextBuildTime)
        {
            float tiempoRestante = nextBuildTime - Time.time;
            Debug.Log($"Tarjeta en recarga. Espera {tiempoRestante:F1} segundos.");
            return;
        }
        Collider[] objetosEncontrados = Physics.OverlapSphere(transform.position, radioDeteccion, capaTorres);

        if (objetosEncontrados.Length > 0)
        {
            Debug.LogWarning("¡Ubicación ocupada!");
            return;
        }
        ColocarTorre();
    }

    void ColocarTorre()
    {
        GameObject nuevaTorre = Instantiate(torrePrefab, transform.position, transform.rotation);
        nuevaTorre.transform.SetParent(terrenoPrincipal, true);

        nextBuildTime = Time.time + cooldownTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}