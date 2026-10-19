using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerCooldownHUD : MonoBehaviour
{
    [Header("ImageTarget asociado")]
    [SerializeField] private string imageTargetObjectName = "IT_SmallTower";

    [Header("Elementos de UI")]
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownText;

    private TowerBuilder towerBuilder;
    private bool warnedMissingTarget = false;

    void Start()
    {
        BuscarTowerBuilder();

        if (cooldownFill != null)
        {
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Radial360;
            cooldownFill.fillOrigin = (int)Image.Origin360.Top;
            cooldownFill.fillClockwise = true;
            cooldownFill.raycastTarget = false;
            cooldownFill.fillAmount = 0f;
        }

        ActualizarHUD();
    }

    void Update()
    {
        if (towerBuilder == null)
        {
            BuscarTowerBuilder();
        }

        ActualizarHUD();
    }

    void BuscarTowerBuilder()
    {
        GameObject imageTarget = GameObject.Find(imageTargetObjectName);

        if (imageTarget != null)
        {
            towerBuilder = imageTarget.GetComponent<TowerBuilder>();

            if (towerBuilder == null)
            {
                towerBuilder =
                    imageTarget.GetComponentInChildren<TowerBuilder>(true);
            }
        }

        if (towerBuilder == null && !warnedMissingTarget)
        {
            Debug.LogWarning("No se encontró TowerBuilder para el ImageTarget: " + imageTargetObjectName);

            warnedMissingTarget = true;
        }
        else if (towerBuilder != null)
        {
            warnedMissingTarget = false;
        }
    }

    void ActualizarHUD()
    {
        if (towerBuilder == null)
            return;

        float duracion = towerBuilder.CooldownDuration;
        float restante = towerBuilder.CooldownRemaining;

        bool enCooldown = restante > 0f && duracion > 0f;

        if (cooldownFill != null)
        {
            cooldownFill.enabled = enCooldown;

            if (enCooldown)
            {
                float progreso = 1f - (restante / duracion);

                cooldownFill.fillAmount = Mathf.Clamp01(progreso);
            }
            else
            {
                cooldownFill.fillAmount = 0f;
            }
        }

        if (cooldownText != null)
        {
            cooldownText.enabled = enCooldown;

            cooldownText.text = enCooldown
                ? restante.ToString("0.0") + " s"
                : "";
        }
    }
}