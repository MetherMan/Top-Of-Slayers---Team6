using UnityEngine;

[DefaultExecutionOrder(1000)]
public class KtjCameraFollowController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private SlashDashController dashController;

    [Header("기본 추적")]
    [SerializeField, Min(0f)] private float followLerpSpeed = 8f;
    [SerializeField] private bool keepInitialOffset = true;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 20f, -9f);

    [Header("공격 끌림 연출")]
    [SerializeField] private bool useAttackPull = true;
    [SerializeField, Min(0f)] private float attackLookAhead = 1.4f;
    [SerializeField, Min(0f)] private float attackPullLerpSpeed = 3f;

    [Header("맵 경계(선택)")]
    [SerializeField] private bool useBoundsClamp = false;
    [SerializeField, Min(0f)] private float maxXDistance = 0f;
    [SerializeField, Min(0f)] private float maxZDistance = 0f;

    private bool hasCapturedOffset;

    private void Awake()
    {
        ResolveReferences();
        CaptureOffsetIfNeeded();
    }

    private void LateUpdate()
    {
        ResolveReferences();
        if (followTarget == null) return;

        var targetPosition = followTarget.position + followOffset;
        var lerpSpeed = followLerpSpeed;

        if (useAttackPull && dashController != null && dashController.IsDashing)
        {
            var dashDirection = dashController.DashDirection;
            dashDirection.y = 0f;
            if (dashDirection.sqrMagnitude <= 0.0001f)
            {
                dashDirection = followTarget.forward;
                dashDirection.y = 0f;
            }

            if (dashDirection.sqrMagnitude > 0.0001f)
            {
                targetPosition += dashDirection.normalized * attackLookAhead;
            }

            lerpSpeed = attackPullLerpSpeed;
        }

        if (useBoundsClamp)
        {
            if (maxXDistance > 0f)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, -maxXDistance, maxXDistance);
            }
            if (maxZDistance > 0f)
            {
                targetPosition.z = Mathf.Clamp(targetPosition.z, -maxZDistance, maxZDistance);
            }
        }

        var t = 1f - Mathf.Exp(-Mathf.Max(0f, lerpSpeed) * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, t);
    }

    private void ResolveReferences()
    {
        if (followTarget == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                followTarget = player.transform;
            }
        }

        if (dashController == null && followTarget != null)
        {
            dashController = followTarget.GetComponent<SlashDashController>();
            if (dashController == null)
            {
                dashController = followTarget.GetComponentInChildren<SlashDashController>(true);
            }
        }
    }

    private void CaptureOffsetIfNeeded()
    {
        if (!keepInitialOffset) return;
        if (hasCapturedOffset) return;
        if (followTarget == null) return;

        followOffset = transform.position - followTarget.position;
        hasCapturedOffset = true;
    }
}
