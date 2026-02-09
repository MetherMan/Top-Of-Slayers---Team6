using UnityEngine;

public class ButtonOn : MonoBehaviour
{
    public float selectedWidth = 140f;
    public float normalWidth = 100f;

    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetSelected(bool selected)
    {
        float w = selected ? selectedWidth : normalWidth;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
    }
}
