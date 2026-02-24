using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class ChainVisualController
{
    private static Sprite chainTimerBarFallbackSprite;

    private void ShowChain(int chain)
    {
        if (chainUI != null && chainUI.IsReady)
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
            chainPanel.SetActive(false);
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

    private void UpdateChainTimerBar()
    {
        EnsureChainTimerBar();
        if (chainTimerBarFillImage == null) return;

        var timerRoot = ResolveChainTimerBarRoot();
        if (timerRoot == null) return;

        var isTimerActive = isChainActive && chainCombat != null && chainCombat.IsSlowActive;
        if (!isTimerActive)
        {
            if (timerRoot.activeSelf)
            {
                timerRoot.SetActive(false);
            }
            return;
        }

        if (!timerRoot.activeSelf)
        {
            timerRoot.SetActive(true);
        }

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

        var timerRoot = ResolveChainTimerBarRoot();
        if (timerRoot != null)
        {
            timerRoot.SetActive(false);
        }
    }

    private void EnsureChainTimerBar()
    {
        if (chainTimerBarFillImage != null)
        {
            if (!isChainTimerBarConfigured)
            {
                ConfigureChainTimerBar();
                isChainTimerBarConfigured = true;
            }
            return;
        }

        var parentRect = ResolveChainTimerBarParent();
        if (parentRect == null) return;

        var existingRoot = parentRect.Find("Chain Timer Bar");
        if (existingRoot != null)
        {
            chainTimerBarBackgroundImage = existingRoot.GetComponent<Image>();
            var existingFill = existingRoot.Find("Fill");
            if (existingFill != null)
            {
                chainTimerBarFillImage = existingFill.GetComponent<Image>();
            }

            if (chainTimerBarFillImage == null)
            {
                var images = existingRoot.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] == null || images[i] == chainTimerBarBackgroundImage) continue;
                    chainTimerBarFillImage = images[i];
                    break;
                }
            }

            ConfigureChainTimerBar();
            isChainTimerBarConfigured = true;
            return;
        }

        if (!autoCreateChainTimerBar) return;

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

        ConfigureChainTimerBar();
        isChainTimerBarConfigured = true;
        barRoot.SetActive(false);
    }

    private RectTransform ResolveChainTimerBarParent()
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

    private GameObject ResolveChainTimerBarRoot()
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

    private void ConfigureChainTimerBar()
    {
        var sprite = GetChainTimerBarFallbackSprite();

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

    private static Sprite GetChainTimerBarFallbackSprite()
    {
        if (chainTimerBarFallbackSprite != null) return chainTimerBarFallbackSprite;

        var texture = Texture2D.whiteTexture;
        chainTimerBarFallbackSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        chainTimerBarFallbackSprite.name = "ChainTimerBarFallbackSprite";
        return chainTimerBarFallbackSprite;
    }
}
