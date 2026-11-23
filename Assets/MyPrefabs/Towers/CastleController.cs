using UnityEngine;

public class CastleController : MonoBehaviour
{
    [Header("Vida del Castillo")]
    public float maxHealth = 500f;

    private float currentHealth;
    public bool IsDestroyed { get; private set; }

    [Header("Estados")]
    public GameObject intacto;
    public GameObject destruido;

    void Awake()
    {
        currentHealth = maxHealth;
        IsDestroyed = false;

        ConfigurarEstadoInicial();
    }

    private void ConfigurarEstadoInicial()
    {
        if (intacto != null)
        {
            intacto.SetActive(true);
        }

        if (destruido != null)
        {
            destruido.SetActive(false);
        }
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
        if (IsDestroyed)
            return;

        IsDestroyed = true;

        if (intacto != null)
        {
            intacto.SetActive(false);
        }

        if (destruido != null)
        {
            destruido.SetActive(true);
        }

        Debug.Log("¡El castillo ha sido destruido! Fin de la partida.");
    }
}