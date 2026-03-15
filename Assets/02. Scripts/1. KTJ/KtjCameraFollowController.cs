using UnityEngine;

[DefaultExecutionOrder(1000)]
public class KtjCameraFollowController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private ChainCombatController chainCombat;

    [Header("기본 추적")]
    [SerializeField, Min(0f)] private float followLerpSpeed = 8f;
    [SerializeField] private bool keepInitialOffset = true;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 20f, -9f);

    [Header("공격 끌림 연출")]
    [SerializeField] private bool useAttackPull = true;
    [SerializeField, Min(0f)] private float attackLookAhead = 1.4f;
    [SerializeField, Min(0f)] private float attackPullLerpSpeed = 3f;
    [SerializeField] private bool useUnscaledFollowDuringChain = true;
    [SerializeField, Min(0f)] private float chainAttackPullLerpSpeed = 6f;

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
        var isChainActive = chainCombat != null && chainCombat.IsSlowActive;

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
            if (isChainActive)
            {
                lerpSpeed = Mathf.Max(lerpSpeed, chainAttackPullLerpSpeed);
            }
        }

        if (useBoundsClamp)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, -maxXDistance, maxXDistance);
            targetPosition.z = Mathf.Clamp(targetPosition.z, -maxZDistance, maxZDistance);
        }

        var delta = useUnscaledFollowDuringChain && isChainActive
            ? Time.unscaledDeltaTime
            : Time.deltaTime;
        var t = 1f - Mathf.Exp(-Mathf.Max(0f, lerpSpeed) * delta);
        transform.position = Vector3.Lerp(transform.position, targetPosition, t);
    }

    private void ResolveReferences()
    {
        if (followTarget == null)
        {
            followTarget = FindPlayerTarget();
        }

        if (dashController == null && followTarget != null)
        {
            dashController = followTarget.GetComponent<SlashDashController>();
            if (dashController == null)
            {
                dashController = followTarget.GetComponentInChildren<SlashDashController>(true);
            }
        }

        if (dashController == null)
        {
            dashController = FindFirstObjectByType<SlashDashController>();
        }

        if (chainCombat == null && followTarget != null)
        {
            chainCombat = followTarget.GetComponent<ChainCombatController>();
            if (chainCombat == null)
            {
                chainCombat = followTarget.GetComponentInChildren<ChainCombatController>(true);
            }
        }

        if (chainCombat == null)
        {
            chainCombat = FindFirstObjectByType<ChainCombatController>();
        }

        CaptureOffsetIfNeeded();
    }

    private void CaptureOffsetIfNeeded()
    {
        if (!keepInitialOffset) return;
        if (hasCapturedOffset) return;
        if (followTarget == null) return;

        followOffset = transform.position - followTarget.position;
        hasCapturedOffset = true;
    }

    private static Transform FindPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            GameObject player = players[i];
            if (player == null) continue;

            bool hasCombatController =
                player.GetComponent<SlashDashController>() != null ||
                player.GetComponent<ChainCombatController>() != null ||
                player.GetComponentInChildren<SlashDashController>(true) != null ||
                player.GetComponentInChildren<ChainCombatController>(true) != null;

            if (hasCombatController)
            {
                return player.transform;
            }
        }

        if (players.Length > 0 && players[0] != null)
        {
            return players[0].transform;
        }

        return null;
    }
}
