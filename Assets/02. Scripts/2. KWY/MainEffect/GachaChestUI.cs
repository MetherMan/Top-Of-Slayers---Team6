using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GachaChestUI : MonoBehaviour
{
    [SerializeField] Image chestImage;
    [SerializeField] Sprite defaultImage;
    [SerializeField] Sprite closedChest;
    [SerializeField] Sprite openedChest;
    [SerializeField] float resultDelay = 0.6f;

    Action cachedOnOpened;


    public void PlayChest(Action onOpened)
    {
        cachedOnOpened = onOpened;

        chestImage.sprite = closedChest;

        chestImage.transform
            .DOShakePosition(1f, new Vector2(25f, 0f))
            .OnComplete(OnShakeComplete);
    }

    private void OnShakeComplete()
    {
        chestImage.sprite = openedChest;

        chestImage.transform.DOScale(1.2f, 0.2f)
            .SetLoops(2, LoopType.Yoyo);

        DOVirtual.DelayedCall(resultDelay, OnResultDelayComplete);
    }

    private void OnResultDelayComplete()
    {
        cachedOnOpened?.Invoke();
    }

    public void ResetChest()
    {
        chestImage.sprite = defaultImage;
    }

    public void OnClickReset()
    {
        ResetChest();
    }
}
