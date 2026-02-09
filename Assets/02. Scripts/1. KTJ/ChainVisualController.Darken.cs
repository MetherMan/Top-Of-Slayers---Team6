using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class ChainVisualController
{
    private void PlayDarken(bool isActive)
    {
        var useEnvironment = ShouldUseEnvironmentDarken(isActive);
        if (useEnvironment)
        {
            PlayEnvironmentDarken(isActive);
            if (!useSpriteDarkenWithEnvironment)
            {
                PlaySpriteDarken(false);
                return;
            }
        }
        else
        {
            PlayEnvironmentDarken(false);
        }

        PlaySpriteDarken(isActive);
    }

    private void PlaySpriteDarken(bool isActive)
    {
        if (darkenGroup == null && darkenGraphic == null && darkenSprite == null) return;
        var darkenHost = ResolveDarkenHost();
        if (isActive && darkenHost != null && !darkenHost.activeSelf)
        {
            darkenHost.SetActive(true);
        }

        if (darkenGroup != null) darkenGroup.DOKill();
        if (darkenGraphic != null) darkenGraphic.DOKill();
        if (darkenSprite != null) darkenSprite.DOKill();
        if (darkenRoot != null) darkenRoot.DOKill();

        if (!isActive)
        {
            darkenTween = CreateDarkenFade(0f);
            if (darkenTween != null)
            {
                darkenTween.OnComplete(() =>
                {
                    if (darkenGroup != null) darkenGroup.blocksRaycasts = false;
                    var host = ResolveDarkenHost();
                    if (host != null) host.SetActive(false);
                });
            }
            else if (darkenHost != null)
            {
                darkenHost.SetActive(false);
            }
            return;
        }

        if (darkenGroup != null) darkenGroup.blocksRaycasts = true;
        SetDarkenAlpha(0f);

        var sequence = DOTween.Sequence().SetUpdate(useUnscaledTime);
        if (useDarkenBurst && darkenRoot != null)
        {
            darkenRoot.localScale = darkenBaseScale * darkenStartScale;
            sequence.Append(darkenRoot.DOScale(darkenBaseScale * darkenOvershootScale, darkenBurstTime).SetEase(Ease.OutBack));
            sequence.Append(darkenRoot.DOScale(darkenBaseScale, darkenSettleTime).SetEase(Ease.OutSine));
        }
        var fade = CreateDarkenFade(darkenAlpha);
        if (fade != null) sequence.Join(fade);
        darkenTween = sequence;
    }

    private Tween CreateDarkenFade(float alpha)
    {
        if (darkenGroup != null)
        {
            return darkenGroup.DOFade(alpha, darkenFadeTime).SetEase(darkenFadeEase).SetUpdate(useUnscaledTime);
        }
        if (darkenGraphic != null)
        {
            return darkenGraphic.DOFade(alpha, darkenFadeTime).SetEase(darkenFadeEase).SetUpdate(useUnscaledTime);
        }
        if (darkenSprite != null)
        {
            return darkenSprite.DOFade(alpha, darkenFadeTime).SetEase(darkenFadeEase).SetUpdate(useUnscaledTime);
        }
        return null;
    }

    private void SetDarkenAlpha(float alpha)
    {
        if (darkenGroup != null)
        {
            darkenGroup.alpha = alpha;
            darkenGroup.blocksRaycasts = alpha > 0.01f;
            return;
        }
        if (darkenGraphic != null)
        {
            var color = darkenGraphic.color;
            color.a = alpha;
            darkenGraphic.color = color;
            return;
        }
        if (darkenSprite != null)
        {
            var color = darkenSprite.color;
            color.a = alpha;
            darkenSprite.color = color;
        }
    }

    private void KillTweens()
    {
        if (darkenTween != null) darkenTween.Kill();
        darkenTween = null;
        KillEnvironmentTween();
        if (darkenRoot != null) darkenRoot.DOKill();
        if (darkenGroup != null) darkenGroup.DOKill();
        if (darkenGraphic != null) darkenGraphic.DOKill();
        if (darkenSprite != null) darkenSprite.DOKill();
        if (chainTextRoot != null) chainTextRoot.DOKill();
        if (chainTextGroup != null) chainTextGroup.DOKill();
    }

    private void ResetDarkenImmediate()
    {
        ResetEnvironmentDarkenImmediate();
        if (darkenRoot != null)
        {
            darkenRoot.localScale = darkenBaseScale;
        }
        SetDarkenAlpha(0f);
        if (darkenGroup != null)
        {
            darkenGroup.blocksRaycasts = false;
        }

        var darkenHost = ResolveDarkenHost();
        if (darkenHost != null)
        {
            darkenHost.SetActive(false);
        }
    }

    private GameObject ResolveDarkenHost()
    {
        if (darkenRoot != null) return darkenRoot.gameObject;
        if (darkenGroup != null) return darkenGroup.gameObject;
        if (darkenGraphic != null) return darkenGraphic.gameObject;
        if (darkenSprite != null) return darkenSprite.gameObject;
        return null;
    }
}
