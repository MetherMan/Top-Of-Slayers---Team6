using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class CoinDropEffect : MonoBehaviour
{
    [SerializeField] private GameObject pileOfCoins;
    [SerializeField] private RectTransform targetPosition;
    [SerializeField] private TextMeshProUGUI coinText;

    private Vector2[] initialPos;
    private Quaternion[] initialRotation;

    private int coinsAmount;

    private void Start()
    {
        coinsAmount = pileOfCoins.transform.childCount;

        initialPos = new Vector2[coinsAmount];
        initialRotation = new Quaternion[coinsAmount];

        for (int i = 0; i < coinsAmount; i++)
        {
            RectTransform coin = pileOfCoins.transform.GetChild(i).GetComponent<RectTransform>();

            initialPos[i] = coin.anchoredPosition;
            initialRotation[i] = coin.rotation;

            coin.localScale = Vector3.zero;
        }
    }

    public void CoinParty()
    {
        pileOfCoins.SetActive(true);

        float delay = 0f;

        for (int i = 0; i < coinsAmount; i++)
        {
            RectTransform coin = pileOfCoins.transform.GetChild(i).GetComponent<RectTransform>();

            coin.gameObject.SetActive(true);

            // 코인 등장
            coin.DOScale(1f, 0.3f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);

            // 목표 위치 계산
            Vector2 targetCanvasPos = GetCanvasPosition(coin, targetPosition);

            // 이동
            coin.DOAnchorPos(targetCanvasPos, 0.8f)
                .SetDelay(delay + 0.5f)
                .SetEase(Ease.InBack);

            // 회전
            coin.DORotate(Vector3.zero, 0.5f)
                .SetDelay(delay + 0.5f)
                .SetEase(Ease.Flash);

            // 사라짐
            coin.DOScale(0f, 0.3f)
                .SetDelay(delay + 1.5f)
                .SetEase(Ease.OutBack);

            delay += 0.1f;
        }

        // 골드 텍스트 튀는 효과
        coinText.transform.parent.DOScale(1.1f, 0.1f)
            .SetLoops(10, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(1.2f);

        StartCoroutine(InitCoin());
    }

    Vector2 GetCanvasPosition(RectTransform coin, RectTransform target)
    {
        RectTransform canvasRect = coin.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Vector2 targetCanvasPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, target.position),
            null,
            out targetCanvasPos
        );

        return targetCanvasPos;
    }

    IEnumerator InitCoin()
    {
        yield return new WaitForSecondsRealtime(2f);


        for (int i = 0; i < coinsAmount; i++)
        {
            RectTransform coin = pileOfCoins.transform.GetChild(i).GetComponent<RectTransform>();

            coin.anchoredPosition = initialPos[i];
            coin.rotation = initialRotation[i];

            coin.gameObject.SetActive(false);
        }

        pileOfCoins.SetActive(false);
    }

}