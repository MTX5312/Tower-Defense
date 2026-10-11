using UnityEngine;

public class TowerController : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    public float range = 1.5f;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Referencias")]
    public string enemyTag = "Enemy";
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Transform target;

    void Update()
    {
        UpdateTarget();

        if (target == null) return;
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 5f).eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null && target != null)
        {
            GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                projectile.SetObjective(target);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}