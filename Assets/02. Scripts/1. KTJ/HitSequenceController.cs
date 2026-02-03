using UnityEngine;
public class HitSequenceController : MonoBehaviour
{
    [Header("히트 스톱")]
    [SerializeField] private bool useHitStop = true;
    [SerializeField, Min(0f)] private float hitStopDuration = 0.05f;
    private Coroutine hitStopRoutine;
    private static int hitStopCount;
    private static float cachedTimeScale = 1f;
    private int localHitStopCount;
    public void TriggerHitStop()
    {
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
}
