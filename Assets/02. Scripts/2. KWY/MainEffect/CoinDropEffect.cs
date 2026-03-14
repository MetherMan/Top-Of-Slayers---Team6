using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class CoinDropEffect : MonoBehaviour
{
    [SerializeField] private RectTransform pileOfCoins;
    [SerializeField] private RectTransform targetPosition;
    [SerializeField] private TextMeshProUGUI coinText;

    private Vector2[] initialPos;
    private Quaternion[] initialRotation;

    private RectTransform[] coinRects;

    private int coinsAmount;

    private void Start()
    {
        coinsAmount = pileOfCoins.childCount;

        initialPos = new Vector2[coinsAmount];
        initialRotation = new Quaternion[coinsAmount];
        coinRects = new RectTransform[coinsAmount];

        for (int i = 0; i < coinsAmount; i++)
        {
            RectTransform coin = pileOfCoins.GetChild(i).GetComponent<RectTransform>();

            coinRects[i] = coin;

            initialPos[i] = coin.anchoredPosition;
            initialRotation[i] = coin.rotation;

            coin.localScale = Vector3.zero;
            coin.gameObject.SetActive(false);
        }

        pileOfCoins.gameObject.SetActive(false);
    }

    public void CoinParty(Vector3 startPosition)
    {

        Debug.Log(targetPosition);
        Debug.Log(pileOfCoins);
        pileOfCoins.gameObject.SetActive(true);

        pileOfCoins.position = startPosition;

        float delay = 0f;

        for (int i = 0; i < coinsAmount; i++)
        {
            RectTransform coin = coinRects[i];

            coin.gameObject.SetActive(true);

            coin.DOScale(1f, 0.3f).SetDelay(delay).SetEase(Ease.OutBack);

            coin.DOMove(targetPosition.position, 0.8f).SetDelay(delay + 0.5f).SetEase(Ease.InBack);

            coin.DORotate(Vector3.zero, 0.5f).SetDelay(delay + 0.5f).SetEase(Ease.Flash);

            coin.DOScale(0f, 0.3f).SetDelay(delay + 1.5f).SetEase(Ease.OutBack);

            delay += 0.1f;
        }

        coinText.transform.parent.DOScale(1.1f, 0.1f).SetLoops(10, LoopType.Yoyo).SetEase(Ease.InOutSine)
            .SetDelay(1.2f);

        StartCoroutine(InitCoin());
    }

    IEnumerator InitCoin()
    {
        yield return new WaitForSecondsRealtime(2f);

        for (int i = 0; i < coinsAmount; i++)
        {
            RectTransform coin = coinRects[i];

            coin.anchoredPosition = initialPos[i];
            coin.rotation = initialRotation[i];

            coin.gameObject.SetActive(false);
        }

        pileOfCoins.gameObject.SetActive(false);
    }
}