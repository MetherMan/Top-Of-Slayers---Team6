using UnityEngine;
using UnityEngine.UI;
public class BackgroundFitter : MonoBehaviour
{
    void Start()
    {
        Fit();
    }

    public void Fit()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.one;

        }
    }
}