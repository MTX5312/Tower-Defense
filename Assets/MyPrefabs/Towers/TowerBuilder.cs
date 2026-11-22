using UnityEngine;
using Vuforia;

public class TowerBuilder : MonoBehaviour
{
    [Header("Construcción Base")]
    public GameObject torrePrefab;
    public Transform terrenoPrincipal;
    public float alturaOffsetLocal = 0f;

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

    void OnDestroy()
    {
        if (mObserverBehaviour != null)
        {
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
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
        if (terrenoPrincipal == null)
        {
            Debug.LogWarning("TowerBuilder: terrenoPrincipal aún no está asignado. Escaneá primero IT_Terreno.");
            return;
        }

        if (Time.time < nextBuildTime)
        {
            float tiempoRestante = nextBuildTime - Time.time;
            Debug.Log($"Tarjeta en recarga. Espera {tiempoRestante:F1} segundos.");
            return;
        }

        Vector3 posicionLocalEnTerreno = terrenoPrincipal.InverseTransformPoint(transform.position);
        posicionLocalEnTerreno.y += alturaOffsetLocal;
        Vector3 posicionMundoCalculada = terrenoPrincipal.TransformPoint(posicionLocalEnTerreno);

        Collider[] objetosEncontrados = Physics.OverlapSphere(posicionMundoCalculada, radioDeteccion, capaTorres);

        if (objetosEncontrados.Length > 0)
        {
            Debug.LogWarning("¡Ubicación ocupada!");
            return;
        }

        ColocarTorre(posicionLocalEnTerreno);
    }

    void ColocarTorre(Vector3 posicionLocalEnTerreno)
    {
        if (torrePrefab == null || terrenoPrincipal == null)
            return;

        GameObject nuevaTorre = Instantiate(torrePrefab, terrenoPrincipal);
        nuevaTorre.transform.localPosition = posicionLocalEnTerreno;

        Quaternion rotacionLocalRelativa = Quaternion.Inverse(terrenoPrincipal.rotation) * transform.rotation;
        nuevaTorre.transform.localRotation = Quaternion.Euler(0f, rotacionLocalRelativa.eulerAngles.y, 0f);

        nextBuildTime = Time.time + cooldownTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 centro = terrenoPrincipal != null
            ? terrenoPrincipal.TransformPoint(terrenoPrincipal.InverseTransformPoint(transform.position) + Vector3.up * alturaOffsetLocal)
            : transform.position;
        Gizmos.DrawWireSphere(centro, radioDeteccion);
    }
}