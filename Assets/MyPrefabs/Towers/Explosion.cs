using UnityEngine;

public class Explosion : MonoBehaviour
{
    private ProjectileController parentProjectile;

    void Awake()
    {
        parentProjectile = GetComponentInParent<ProjectileController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (parentProjectile != null)
        {
            parentProjectile.RegistrarEnemigoEnExplosion(other);
        }
    }
}