using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CirculDrag : MonoBehaviour
{
    public RectTransform[] panels;

    public float slideDuration = 0.5f;

    [SerializeField] private int currentIndex = 0;

    private bool isSliding = false;

    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    private void Start()
    {
        currentIndex = 0;
        SetOnClickButton();
    }

    private void SetOnClickButton()
    {
        nextButton.onClick.AddListener(() => ShowPanel(currentIndex - 1));
        prevButton.onClick.AddListener(() => ShowPanel(currentIndex + 1));
    }

    public void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Length) return;

        if (index == currentIndex || isSliding) return;

        isSliding = true;

        RectTransform fromPanel = panels[currentIndex];
        RectTransform toPanel = panels[index];

        fromPanel.transform.SetAsFirstSibling();
        toPanel.transform.SetAsFirstSibling();

        Vector2 panelPosition = currentIndex < index
            ? new Vector2(Screen.width, 0)
            : new Vector2(-Screen.width, 0);

        toPanel.gameObject.SetActive(true);
        toPanel.anchoredPosition = panelPosition;

        fromPanel.DOAnchorPos(-panelPosition, slideDuration).SetEase(Ease.OutCubic);
        toPanel.DOAnchorPos(Vector2.zero, slideDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            fromPanel.gameObject.SetActive(false);
            fromPanel.anchoredPosition = Vector2.zero;
            currentIndex = index;
            isSliding = false;
        });
    }


}
