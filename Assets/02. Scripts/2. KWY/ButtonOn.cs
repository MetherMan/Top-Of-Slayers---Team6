using UnityEngine;

public class ButtonOn : MonoBehaviour
{
    [SerializeField] private float selectedScale = 1.5f;
    [SerializeField] private float normalScale = 1.0f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            rect.localScale = new Vector3(1,1,1) * selectedScale;
        }
        else
        {
            rect.localScale = new Vector3(1,1,1) * normalScale;
        }
    }
}
