using UnityEngine;

public class CastleController : MonoBehaviour
{
    [Header("Vida del Castillo")]
    public float maxHealth = 500f;

    private float currentHealth;
    public bool IsDestroyed { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
        IsDestroyed = false;
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsDestroyed || damageAmount <= 0f)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.Log(
            "El castillo recibió " + damageAmount +
            " de daño. Vida restante: " +
            currentHealth + "/" + maxHealth
        );

        if (currentHealth <= 0f)
        {
            DestroyCastle();
        }
    }

    void DestroyCastle()
    {
        IsDestroyed = true;

        Debug.Log("¡El castillo ha sido destruido! Fin de la partida.");
    }
}