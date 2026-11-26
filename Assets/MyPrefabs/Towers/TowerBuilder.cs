using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class TowerBuilder : MonoBehaviour
{
    [Header("Construcción Base")]
    public GameObject torrePrefab;

    [Tooltip("Opcional: si se deja vacío, la torre se crea como hija de este mismo ImageTarget")]
    public Transform padreTorre;

    public float alturaOffsetLocal = 0f;

    [Header("Detección de Colisión")]
    public float radioDeteccion = 0.05f;
    public LayerMask capaTorres;

    [Header("UI de Bloqueo Permanente")]
    [Tooltip("Imagen UI que cubre toda la tarjeta/botón y queda oscura al construir")]
    public UnityEngine.UI.Image overlayOscuroCompleto;

    [Range(0f, 1f)]
    public float opacidadOscura = 0.78f;

    private ObserverBehaviour mObserverBehaviour;
    private bool torreConstruida = false;

    public bool TorreConstruida => torreConstruida;

    void Awake()
    {
        SetOverlayOscuro(false);
    }

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
        if (torreConstruida)
            return;

        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            IntentarColocarTorre();
        }
    }

    public void IntentarColocarTorre()
    {
        if (torreConstruida || torrePrefab == null)
            return;

        Vector3 posicionSpawn = transform.position + transform.up * alturaOffsetLocal;

        if (capaTorres.value != 0)
        {
            Collider[] objetosEncontrados = Physics.OverlapSphere(posicionSpawn, radioDeteccion, capaTorres);
            if (objetosEncontrados.Length > 0)
            {
                Debug.LogWarning("TowerBuilder: ubicación ocupada por otra torre.");
                return;
            }
        }

        ColocarTorre(posicionSpawn);
    }

    private void ColocarTorre(Vector3 posicionSpawn)
    {
        Transform destinoPadre = padreTorre != null ? padreTorre : transform;

        GameObject nuevaTorre = Instantiate(
            torrePrefab,
            posicionSpawn,
            Quaternion.Euler(0f, transform.eulerAngles.y, 0f),
            destinoPadre
        );

        if (nuevaTorre != null)
        {
            torreConstruida = true;
            SetOverlayOscuro(true);
        }
    }

    private void SetOverlayOscuro(bool activa)
    {
        if (overlayOscuroCompleto == null)
            return;

        overlayOscuroCompleto.gameObject.SetActive(activa);

        if (activa)
        {
            Color c = overlayOscuroCompleto.color;
            c.r = 0f;
            c.g = 0f;
            c.b = 0f;
            c.a = opacidadOscura;
            overlayOscuroCompleto.color = c;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 centro = transform.position + transform.up * alturaOffsetLocal;
        Gizmos.DrawWireSphere(centro, radioDeteccion);
    }
}