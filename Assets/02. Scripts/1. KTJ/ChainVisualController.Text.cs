using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class ChainVisualController
{
    private static Sprite chainTimerBarSprite;

    private void ShowChain(int chain)
    {
        if (chainUI != null && chainUI.IsReady)
        {
            chainUI.UpdateChainUI(chain);
            return;
        }

        SetChainPanelVisible(true);
        if (chainText != null)
        {
            chainText.text = string.Format(chainTextFormat, chain);
        }

        EnsureChainTimerBar();
        PlayChainText();
    }

    private void HideChain()
    {
        if (chainUI != null && chainUI.IsReady)
        {
            chainUI.HideChainUI(lastChain);
            return;
        }

        if (chainPanel == null) return;
        if (chainTextGroup == null)
        {
            SetChainPanelVisible(false);
            return;
        }

        chainTextGroup.DOKill();
        chainTextGroup.alpha = 1f;
        chainTextGroup
            .DOFade(0f, chainTextFadeOut)
            .SetEase(Ease.OutQuad)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() => SetChainPanelVisible(false));
    }

    private bool IsChainVisible()
    {
        if (chainUI != null && chainUI.IsReady) return chainUI.IsVisible;
        if (chainPanel == null) return false;
        return chainPanel.activeSelf;
    }

    private void HideChainImmediate()
    {
        if (chainUI != null && chainUI.IsReady)
        {
            chainUI.HideChainUI(lastChain);
        }

        if (chainTextGroup != null)
        {
            chainTextGroup.DOKill();
            chainTextGroup.alpha = 0f;
        }

        if (chainPanel != null)
        {
            SetChainPanelVisible(false);
        }
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

    private void PlayChainMilestoneBeat(int chain)
    {
        if (!useChainMilestoneBeat) return;
        if (!isChainActive) return;

        var chainWeight = Mathf.Clamp01((Mathf.Max(1, chain) - 1f) / 6f);
        var punchScale = milestoneTextPunchScale * Mathf.Lerp(1f, 1.25f, chainWeight);

        if (useMilestoneHitStop && hitSequence != null)
        {
            hitSequence.TriggerHitStop();
        }

        PunchChainText(punchScale, milestoneTextPunchDuration);
        FlashChainTextColor(milestoneTextFlashColor, milestoneTextFlashReturn);
        FlashChainTimerBarColor(milestoneTimerBarFlashColor, milestoneTimerBarFlashReturn);
    }

    private void PlayKillFinishBeat()
    {
        if (!useChainKillFinishBeat) return;
        if (!isChainActive) return;

        if (useKillFinishHitStop && hitSequence != null)
        {
            hitSequence.TriggerHitStop();
        }

        PunchChainText(killFinishTextPunchScale, killFinishTextPunchDuration);
        FlashChainTextColor(killFinishTextFlashColor, killFinishTextFlashReturn);
    }

    private void PunchChainText(float punchScale, float punchDuration)
    {
        if (chainTextRoot == null) return;
        if (punchScale <= 0f || punchDuration <= 0f) return;

        chainTextRoot.DOKill();
        var punch = Vector3.one * punchScale;
        chainTextRoot.DOPunchScale(punch, punchDuration, 9, 0.65f)
            .SetEase(Ease.OutBack)
            .SetUpdate(useUnscaledTime);
    }

    private void FlashChainTextColor(Color flashColor, float returnDuration)
    {
        if (chainText == null) return;
        if (returnDuration <= 0f) return;

        if (chainTextColorTween != null)
        {
            chainTextColorTween.Kill();
            chainTextColorTween = null;
        }

        chainText.color = flashColor;
        chainTextColorTween = chainText
            .DOColor(chainTextBaseColor, returnDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() => chainTextColorTween = null);
    }

    private void FlashChainTimerBarColor(Color flashColor, float returnDuration)
    {
        if (chainTimerBarFillImage == null) return;
        if (returnDuration <= 0f) return;

        if (chainTimerBarColorTween != null)
        {
            chainTimerBarColorTween.Kill();
            chainTimerBarColorTween = null;
        }

        chainTimerBarFillImage.color = flashColor;
        chainTimerBarColorTween = chainTimerBarFillImage
            .DOColor(chainTimerBarColor, returnDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() => chainTimerBarColorTween = null);
    }

    private void ResetChainBeatImmediate()
    {
        if (chainTextColorTween != null)
        {
            chainTextColorTween.Kill();
            chainTextColorTween = null;
        }

        if (chainTimerBarColorTween != null)
        {
            chainTimerBarColorTween.Kill();
            chainTimerBarColorTween = null;
        }

        if (chainText != null)
        {
            chainText.color = chainTextBaseColor;
        }

        if (chainTimerBarFillImage != null)
        {
            chainTimerBarFillImage.color = chainTimerBarColor;
        }
    }

    private void UpdateChainTimerBar()
    {
        EnsureChainTimerBar();
        if (chainTimerBarFillImage == null) return;

        var timerRoot = GetChainTimerBarRoot();
        if (timerRoot == null) return;

        var isTimerActive = isChainActive && chainCombat != null && chainCombat.IsSlowActive;
        if (!isTimerActive)
        {
            SetChainTimerBarVisible(false);
            return;
        }

        SetChainTimerBarVisible(true);

        var fillAmount = chainCombat.SlowRemainingNormalized;
        chainTimerBarFillImage.fillAmount = fillAmount;

        var fillColor = chainTimerBarColor;
        fillColor.a = Mathf.Lerp(chainTimerBarEmptyAlpha, chainTimerBarColor.a, fillAmount);
        chainTimerBarFillImage.color = fillColor;

        if (chainTimerBarBackgroundImage != null)
        {
            chainTimerBarBackgroundImage.color = chainTimerBarBackgroundColor;
        }
    }

    private void ResetChainTimerBarImmediate()
    {
        if (chainTimerBarFillImage != null)
        {
            chainTimerBarFillImage.fillAmount = 0f;
        }

        var timerRoot = GetChainTimerBarRoot();
        if (timerRoot != null) SetChainTimerBarVisible(false);
    }

    private void EnsureChainTimerBar()
    {
        if (chainTimerBarFillImage == null && !TryBindChainTimerBar())
        {
            if (!autoCreateChainTimerBar) return;

            var parentRect = GetChainTimerBarParent();
            if (parentRect == null) return;

            CreateChainTimerBar(parentRect);
            SetChainTimerBarVisible(false);
        }

        SetupChainTimerBar();
    }

    private RectTransform GetChainTimerBarParent()
    {
        if (chainPanel != null)
        {
            var panelParent = chainPanel.transform.parent as RectTransform;
            if (panelParent != null) return panelParent;

            var panelRect = chainPanel.GetComponent<RectTransform>();
            if (panelRect != null) return panelRect;
        }

        if (chainTextRoot != null)
        {
            var textParent = chainTextRoot.parent as RectTransform;
            if (textParent != null) return textParent;
        }

        return transform as RectTransform;
    }

    private GameObject GetChainTimerBarRoot()
    {
        if (chainTimerBarBackgroundImage != null)
        {
            return chainTimerBarBackgroundImage.gameObject;
        }

        if (chainTimerBarFillImage == null) return null;
        if (chainTimerBarFillImage.transform.parent != null)
        {
            return chainTimerBarFillImage.transform.parent.gameObject;
        }

        return chainTimerBarFillImage.gameObject;
    }

    private void SetChainPanelVisible(bool visible)
    {
        if (chainPanel == null) return;
        if (chainPanel.activeSelf == visible) return;
        chainPanel.SetActive(visible);
    }

    private bool TryBindChainTimerBar()
    {
        if (chainTimerBarFillImage != null)
        {
            return true;
        }

        Transform timerRoot = null;
        if (chainTimerBarBackgroundImage != null)
        {
            timerRoot = chainTimerBarBackgroundImage.transform;
        }
        else
        {
            var parentRect = GetChainTimerBarParent();
            if (parentRect == null) return false;
            timerRoot = parentRect.Find("Chain Timer Bar");
        }

        if (timerRoot == null) return false;

        chainTimerBarBackgroundImage = timerRoot.GetComponent<Image>();
        var fillTransform = timerRoot.Find("Fill");
        if (fillTransform == null) return false;

        chainTimerBarFillImage = fillTransform.GetComponent<Image>();
        return chainTimerBarFillImage != null;
    }

    private void CreateChainTimerBar(RectTransform parentRect)
    {
        var barRoot = new GameObject("Chain Timer Bar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var barRect = barRoot.GetComponent<RectTransform>();
        barRect.SetParent(parentRect, false);
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.offsetMin = new Vector2(chainTimerBarSidePadding, -(chainTimerBarTopMargin + chainTimerBarHeight));
        barRect.offsetMax = new Vector2(-chainTimerBarSidePadding, -chainTimerBarTopMargin);
        barRect.SetAsLastSibling();

        var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.SetParent(barRect, false);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        chainTimerBarBackgroundImage = barRoot.GetComponent<Image>();
        chainTimerBarFillImage = fillObject.GetComponent<Image>();
    }

    private void SetChainTimerBarVisible(bool visible)
    {
        var timerRoot = GetChainTimerBarRoot();
        if (timerRoot == null) return;
        if (timerRoot.activeSelf == visible) return;
        timerRoot.SetActive(visible);
    }

    private void SetupChainTimerBar()
    {
        var sprite = GetChainTimerBarSprite();

        if (chainTimerBarBackgroundImage != null)
        {
            if (chainTimerBarBackgroundImage.sprite == null)
            {
                chainTimerBarBackgroundImage.sprite = sprite;
            }
            chainTimerBarBackgroundImage.raycastTarget = false;
            chainTimerBarBackgroundImage.type = Image.Type.Simple;
            chainTimerBarBackgroundImage.color = chainTimerBarBackgroundColor;
        }

        if (chainTimerBarFillImage == null) return;

        if (chainTimerBarFillImage.sprite == null)
        {
            chainTimerBarFillImage.sprite = sprite;
        }

        chainTimerBarFillImage.raycastTarget = false;
        chainTimerBarFillImage.type = Image.Type.Filled;
        chainTimerBarFillImage.fillMethod = Image.FillMethod.Horizontal;
        chainTimerBarFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        chainTimerBarFillImage.fillClockwise = true;
        chainTimerBarFillImage.fillAmount = 1f;
        chainTimerBarFillImage.color = chainTimerBarColor;
    }

    private static Sprite GetChainTimerBarSprite()
    {
        if (chainTimerBarSprite != null) return chainTimerBarSprite;

        var texture = Texture2D.whiteTexture;
        chainTimerBarSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        chainTimerBarSprite.name = "ChainTimerBarSprite";
        return chainTimerBarSprite;
    }
}
