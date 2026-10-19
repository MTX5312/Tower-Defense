using UnityEngine;
using Vuforia;

public class WorldAnchor : MonoBehaviour
{
    [Header("Contenido completo del escenario")]
    [SerializeField] private Transform contenidoTablero;

    private ObserverBehaviour imageTarget;
    private bool escenarioAnclado = false;

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
            imageTarget.OnTargetStatusChanged +=
                OnTargetStatusChanged;
        }
    }

    private void OnDisable()
    {
        if (imageTarget != null)
        {
            imageTarget.OnTargetStatusChanged -=
                OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(
        ObserverBehaviour behaviour,
        TargetStatus targetStatus)
    {
        if (escenarioAnclado)
            return;

        bool imagenReconocida =
            targetStatus.Status == Status.TRACKED ||
            targetStatus.Status == Status.EXTENDED_TRACKED;

        if (!imagenReconocida)
            return;

        escenarioAnclado = true;

        contenidoTablero.SetParent(null, true);

        gameObject.SetActive(false);
    }
}