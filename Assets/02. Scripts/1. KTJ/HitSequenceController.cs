using DG.Tweening;
using UnityEngine;

public class HitSequenceController : MonoBehaviour
{
    [Header("히트 스톱")]
    [SerializeField] private bool useHitStop = true;
    [SerializeField, Min(0f)] private float hitStopDuration = 0.05f;

    [Header("히트 줌")]
    [SerializeField] private bool useHitZoom = true;
    [SerializeField] private Camera targetCamera;
    [SerializeField, Range(0f, 0.9f)] private float hitZoomStrength = 0.12f;
    [SerializeField, Min(0f)] private float hitZoomInDuration = 0.04f;
    [SerializeField, Min(0f)] private float hitZoomOutDuration = 0.12f;
    [SerializeField] private Ease hitZoomInEase = Ease.OutQuad;
    [SerializeField] private Ease hitZoomOutEase = Ease.OutCubic;
    [SerializeField] private bool useUnscaledZoom = true;

    private Coroutine hitStopRoutine;
    private Tween hitZoomTween;
    private static int hitStopCount;
    private static float cachedTimeScale = 1f;
    private int localHitStopCount;
    private bool hasCameraBaseValue;
    private float baseFieldOfView;
    private float baseOrthographicSize;

    private void Awake()
    {
        CaptureCameraBaseValue();
    }

    public void TriggerHitStop()
    {
        TriggerHitZoom();

        if (!useHitStop) return;
        if (hitStopDuration <= 0f) return;
        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
        }
        hitStopRoutine = StartCoroutine(HitStop(hitStopDuration));
    }

    private void OnDisable()
    {
        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
            hitStopRoutine = null;
        }

        KillHitZoom();
        RestoreCameraBaseValue();
        ClearHitStop();
    }

    private System.Collections.IEnumerator HitStop(float duration)
    {
        EnterHitStop();
        yield return new WaitForSecondsRealtime(duration);
        ExitHitStop();
        hitStopRoutine = null;
    }

    private void EnterHitStop()
    {
        if (hitStopCount == 0)
        {
            cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        hitStopCount++;
        localHitStopCount++;
    }

    private void ExitHitStop()
    {
        if (localHitStopCount <= 0) return;
        localHitStopCount--;
        hitStopCount = Mathf.Max(0, hitStopCount - 1);
        if (hitStopCount == 0)
        {
            Time.timeScale = cachedTimeScale;
        }
    }

    private void ClearHitStop()
    {
        if (localHitStopCount <= 0) return;
        hitStopCount = Mathf.Max(0, hitStopCount - localHitStopCount);
        localHitStopCount = 0;
        if (hitStopCount == 0)
        {
            Time.timeScale = cachedTimeScale;
        }
    }

    private void TriggerHitZoom()
    {
        if (!useHitZoom) return;
        if (hitZoomStrength <= 0f) return;

        var camera = ResolveCamera();
        if (camera == null) return;

        CaptureCameraBaseValue();
        if (!hasCameraBaseValue) return;

        KillHitZoom();

        if (camera.orthographic)
        {
            PlayOrthographicZoom(camera);
            return;
        }

        PlayPerspectiveZoom(camera);
    }

    private void PlayPerspectiveZoom(Camera camera)
    {
        var inDuration = Mathf.Max(0f, hitZoomInDuration);
        var outDuration = Mathf.Max(0f, hitZoomOutDuration);
        var targetFov = Mathf.Clamp(baseFieldOfView * (1f - hitZoomStrength), 10f, 179f);

        var sequence = DOTween.Sequence().SetUpdate(useUnscaledZoom);
        if (inDuration > 0f)
        {
            sequence.Append(DOTween.To(() => camera.fieldOfView, value => camera.fieldOfView = value, targetFov, inDuration).SetEase(hitZoomInEase));
        }
        else
        {
            camera.fieldOfView = targetFov;
        }

        if (outDuration > 0f)
        {
            sequence.Append(DOTween.To(() => camera.fieldOfView, value => camera.fieldOfView = value, baseFieldOfView, outDuration).SetEase(hitZoomOutEase));
        }
        else
        {
            camera.fieldOfView = baseFieldOfView;
        }

        hitZoomTween = sequence;
    }

    private void PlayOrthographicZoom(Camera camera)
    {
        var inDuration = Mathf.Max(0f, hitZoomInDuration);
        var outDuration = Mathf.Max(0f, hitZoomOutDuration);
        var targetSize = Mathf.Max(0.01f, baseOrthographicSize * (1f - hitZoomStrength));

        var sequence = DOTween.Sequence().SetUpdate(useUnscaledZoom);
        if (inDuration > 0f)
        {
            sequence.Append(DOTween.To(() => camera.orthographicSize, value => camera.orthographicSize = value, targetSize, inDuration).SetEase(hitZoomInEase));
        }
        else
        {
            camera.orthographicSize = targetSize;
        }

        if (outDuration > 0f)
        {
            sequence.Append(DOTween.To(() => camera.orthographicSize, value => camera.orthographicSize = value, baseOrthographicSize, outDuration).SetEase(hitZoomOutEase));
        }
        else
        {
            camera.orthographicSize = baseOrthographicSize;
        }

        hitZoomTween = sequence;
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null) return targetCamera;
        if (Camera.main != null)
        {
            targetCamera = Camera.main;
            return targetCamera;
        }

        targetCamera = FindObjectOfType<Camera>();
        return targetCamera;
    }

    private void CaptureCameraBaseValue()
    {
        var camera = ResolveCamera();
        if (camera == null) return;

        if (camera.orthographic)
        {
            baseOrthographicSize = camera.orthographicSize;
        }
        else
        {
            baseFieldOfView = camera.fieldOfView;
        }

        hasCameraBaseValue = true;
    }

    private void RestoreCameraBaseValue()
    {
        if (!hasCameraBaseValue) return;

        var camera = ResolveCamera();
        if (camera == null) return;

        if (camera.orthographic)
        {
            camera.orthographicSize = baseOrthographicSize;
            return;
        }

        camera.fieldOfView = baseFieldOfView;
    }

    private void KillHitZoom()
    {
        if (hitZoomTween != null)
        {
            hitZoomTween.Kill();
            hitZoomTween = null;
        }
    }
}
