using DG.Tweening;
using TMPro;
using UnityEngine;

public partial class ChainVisualController
{
    private void ShowChain(int chain)
    {
        if (chainUI != null)
        {
            chainUI.UpdateChainUI(chain);
            return;
        }
        if (chainPanel != null && !chainPanel.activeSelf)
        {
            chainPanel.SetActive(true);
        }
        if (chainText != null)
        {
            chainText.text = string.Format(chainTextFormat, chain);
        }
        PlayChainText();
    }

    private void HideChain()
    {
        if (chainUI != null)
        {
            chainUI.HideChainUI(lastChain);
            return;
        }
        if (chainPanel == null)
        {
            return;
        }
        if (chainTextGroup == null)
        {
            chainPanel.SetActive(false);
            return;
        }
        chainTextGroup.DOKill();
        chainTextGroup.alpha = 1f;
        chainTextGroup
            .DOFade(0f, chainTextFadeOut)
            .SetEase(Ease.OutQuad)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() => chainPanel.SetActive(false));
    }

    private bool IsChainVisible()
    {
        if (chainUI != null) return true;
        if (chainPanel == null) return false;
        return chainPanel.activeSelf;
    }

    private void PlayChainText()
    {
        if (chainTextRoot == null) return;

        chainTextRoot.DOKill();
        var needFadeIn = false;
        if (chainTextGroup != null)
        {
            chainTextGroup.DOKill();
            needFadeIn = chainTextGroup.alpha <= 0.01f;
            chainTextGroup.alpha = needFadeIn ? 0f : 1f;
        }

        var sequence = DOTween.Sequence().SetUpdate(useUnscaledTime);
        if (chainTextGroup != null && needFadeIn)
        {
            sequence.Join(chainTextGroup.DOFade(1f, chainTextFadeIn).SetEase(Ease.OutQuad));
        }
        if (chainTextPunchScale > 0f)
        {
            var punch = Vector3.one * chainTextPunchScale;
            sequence.Join(chainTextRoot.DOPunchScale(punch, chainTextPunchDuration, 8, 0.6f).SetEase(chainTextEase));
        }
    }
}
