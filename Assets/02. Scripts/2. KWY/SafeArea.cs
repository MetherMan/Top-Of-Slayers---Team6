using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform rt;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (lastSafeArea != Screen.safeArea)
        {
            ApplySafeArea(Screen.safeArea);
        }
    }

    void ApplySafeArea(Rect safeArea)
    {
        lastSafeArea = safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
    }
}