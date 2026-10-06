using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Proyectil")]
    public float speed = 2f;
    public float damage = 25f;
    public float lifeTime = 1.5f;

    [Header("Efectos")]
    public GameObject impactEffectPrefab;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
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
}