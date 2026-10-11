using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Estadísticas del Proyectil")]
    public float speed = 2f;
    public float damage = 25f;
    public float lifeTime = 2.5f;

    [Tooltip("Distancia mínima para impacto")]
    public float hitDistanceThreshold = 0.05f;

    [Header("Efectos")]
    public GameObject impactEffectPrefab;

    private Transform objective;
    private bool impactado = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (impactado) return;

        if (objective == null)
        {
            GoForward();
            return;
        }

        DirectMovementLogic();
    }

    public void SetObjective(Transform target)
    {
        objective = target;
    }

    void DirectMovementLogic()
    {
        Vector3 targetPosition = objective.position + new Vector3(0, 0.15f, 0);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        Vector3 direction = targetPosition - transform.position;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget <= hitDistanceThreshold)
        {
            ProcesarImpacto(objective.gameObject);
        }
    }

    void GoForward()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ProcesarImpacto(other.gameObject);
        }
    }

    void ProcesarImpacto(GameObject enemigoGO)
    {
        if (impactado) return;
        impactado = true;

        EnemyController enemy = enemigoGO.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}