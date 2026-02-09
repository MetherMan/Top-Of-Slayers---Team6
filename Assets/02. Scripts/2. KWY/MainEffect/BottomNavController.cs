using DG.Tweening;
using UnityEngine;

public class BottomNavController : MonoBehaviour
{
    public RectTransform[] buttons;

    public float normalWidth = 500f;
    public float selectedWidth = 900f;

    public float normalHeight = 300f;
    public float selectedHeight = 400f;

    public float pozX = -1000f;
    public float pozY = -300f;
    public float spacing = 0f;

    public float tweenDuration = 0.25f;

    public int current = 0;

    void Start()
    {
        UpdateLayout(true);
    }

    public void Select(int index)
    {
        current = index;
        UpdateLayout(false);
    }

    void UpdateLayout(bool instant)
    {
        float startX = pozX;

        for (int i = 0; i < buttons.Length; i++)
        {
            bool selected = (i == current);

            float w = selected ? selectedWidth : normalWidth;
            float h = selected ? selectedHeight : normalHeight;

            Vector2 targetSize = new Vector2(w, h);
            Vector2 targetPos = new Vector2(startX + w * 0.5f, pozY);

            buttons[i].DOKill();

            if (instant)
            {
                buttons[i].sizeDelta = targetSize;
                buttons[i].anchoredPosition = targetPos;
            }
            else
            {
                buttons[i].DOSizeDelta(targetSize, tweenDuration).SetEase(Ease.OutCubic);
                buttons[i].DOAnchorPos(targetPos, tweenDuration).SetEase(Ease.OutCubic);
            }

            startX += w + spacing;
        }
    }
}
