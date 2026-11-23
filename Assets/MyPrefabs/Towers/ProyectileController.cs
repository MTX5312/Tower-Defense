using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Estadísticas del Proyectil")]
    public float speed = 2f;
    public float damage = 25f;
    public float lifeTime = 2.5f;

    [Tooltip("Distancia mínima para impacto")]
    public float hitDistanceThreshold = 0.05f;

    [Header("Tipo de Proyectil")]
    public bool isPiercing = false;

    public bool isExplosive = false;
    public GameObject explosionTriggerObject;
    public float explosionActiveDuration = 0.1f;

    [Header("Efectos")]
    public GameObject impactEffectPrefab;

    private Transform objective;
    private bool impactado = false;
    private bool explosionEjecutada = false;

    private readonly HashSet<EntityId> enemigosGolpeadosPorPerforacion = new HashSet<EntityId>();
    private readonly HashSet<EntityId> enemigosDanadosEnExplosion = new HashSet<EntityId>();

    void Start()
    {
        if (explosionTriggerObject != null)
        {
            explosionTriggerObject.SetActive(false);
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!isPiercing && impactado)
            return;

        if (isPiercing)
        {
            if (objective != null && !impactado)
            {
                Vector3 targetPos = objective.position + new Vector3(0f, 0.15f, 0f);
                Vector3 dir = (targetPos - transform.position).normalized;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }
                objective = null;
            }

            GoForward();
            return;
        }

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
        Vector3 targetPosition = objective.position + new Vector3(0f, 0.15f, 0f);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

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
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (explosionTriggerObject != null && other.gameObject == explosionTriggerObject)
            return;

        if (other.CompareTag("Enemy"))
        {
            ProcesarImpacto(other.gameObject);
        }
    }

    public void RegistrarEnemigoEnExplosion(Collider other)
    {
        if (!isExplosive || !explosionEjecutada)
            return;

        if (!other.CompareTag("Enemy"))
            return;

        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy == null)
            return;

        EntityId id = enemy.gameObject.GetEntityId();
        if (enemigosDanadosEnExplosion.Contains(id))
            return;

        enemigosDanadosEnExplosion.Add(id);
        enemy.TakeDamage(damage);
    }

    void ProcesarImpacto(GameObject enemigoGO)
    {
        if (isPiercing)
        {
            EnemyController enemyPiercing = enemigoGO.GetComponentInParent<EnemyController>();
            if (enemyPiercing != null)
            {
                EntityId id = enemyPiercing.gameObject.GetEntityId();
                if (!enemigosGolpeadosPorPerforacion.Contains(id))
                {
                    enemigosGolpeadosPorPerforacion.Add(id);
                    enemyPiercing.TakeDamage(damage);

                    if (impactEffectPrefab != null)
                    {
                        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
                    }
                }
            }
            return;
        }

        if (impactado)
            return;

        impactado = true;

        if (isExplosive)
        {
            ActivarExplosion();
            return;
        }

        EnemyController enemy = enemigoGO.GetComponentInParent<EnemyController>();
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

    private void ActivarExplosion()
    {
        if (explosionEjecutada)
            return;

        explosionEjecutada = true;

        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        if (explosionTriggerObject != null)
        {
            StartCoroutine(SecuenciaExplosionTrigger());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator SecuenciaExplosionTrigger()
    {
        explosionTriggerObject.SetActive(true);

        yield return new WaitForFixedUpdate();
        float elapsed = 0f;
        float duracion = Mathf.Max(0.02f, explosionActiveDuration);
        while (elapsed < duracion)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        explosionTriggerObject.SetActive(false);

        Destroy(gameObject);
    }
}