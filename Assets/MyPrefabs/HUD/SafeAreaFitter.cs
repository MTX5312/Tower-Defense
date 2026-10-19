using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea;
    private int lastWidth;
    private int lastHeight;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        AplicarAreaSegura();
    }

    void Update()
    {
        if (lastSafeArea != Screen.safeArea ||
            lastWidth != Screen.width ||
            lastHeight != Screen.height)
        {
            AplicarAreaSegura();
        }
    }

    void AplicarAreaSegura()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect area = Screen.safeArea;

        lastSafeArea = area;
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        Vector2 anchorMin = new Vector2(
            area.xMin / Screen.width,
            area.yMin / Screen.height
        );

        Vector2 anchorMax = new Vector2(
            area.xMax / Screen.width,
            area.yMax / Screen.height
        );

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
    }
}