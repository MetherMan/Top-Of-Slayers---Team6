using UnityEngine;

public partial class AutoSlashController
{
    private const float CoincidentTargetSqrThreshold = 0.0001f;
    private enum SameTargetReattackInputRegion
    {
        None,
        SameTarget,
        Opposite
    }

    private Transform lastAttackTarget;
    private Transform lastAttackAnchorTarget;
    private Vector3 lastAttackAnchorPosition;
    private bool hasLastAttackAnchor;
    private bool sameTargetReleased = true;
    private Vector3 lastAttackAimDirection = Vector3.forward;
    private bool hasLastAttackAim;
    private Vector2 lastAttackInput;
    private bool hasLastAttackInput;
    private float sameTargetReleaseTimer;
    private float sameTargetAutoReleaseTimer;
    private float sameTargetReattackInputBufferTimer;
    private bool sameTargetReattackReady;
    private bool sameTargetReattackNeedsRelease;
    private float sameTargetReattackAimHoldTimer;
    private SameTargetReattackInputRegion sameTargetReattackInputRegion;

    private void CleanupInvalidAttackTargetState()
    {
        if (lastAttackTarget == null) return;
        if (IsAttackableTarget(lastAttackTarget)) return;

        ResetSameTargetRelease();
    }

    private void ClearSameTargetReattackState()
    {
        sameTargetReattackInputBufferTimer = 0f;
        sameTargetReattackReady = false;
        sameTargetReattackNeedsRelease = false;
        sameTargetReattackAimHoldTimer = 0f;
        sameTargetReattackInputRegion = SameTargetReattackInputRegion.None;
    }

    private Transform GetValidLastChainTarget()
    {
        if (chainCombat == null) return null;

        var lastTarget = chainCombat.LastTarget;
        return IsAttackableTarget(lastTarget) ? lastTarget : null;
    }

    private Transform GetSameTargetCandidate()
    {
        var attackTarget = IsAttackableTarget(lastAttackTarget) ? lastAttackTarget : null;
        var chainTarget = GetValidLastChainTarget();
        if (attackTarget == null) return chainTarget;
        if (chainTarget == null) return attackTarget;
        if (!AreSameAttackTargets(attackTarget, chainTarget)) return null;
        return chainTarget;
    }

    private bool TryGetPostChainFallbackOrigin(out Vector3 origin)
    {
        origin = transform.position;

        var anchor = chainCombat != null ? chainCombat.LastTarget : null;
        if (anchor != null)
        {
            origin = anchor.position;
            origin.y = transform.position.y;
            return true;
        }

        if (!hasLastAttackAnchor) return false;

        origin = lastAttackAnchorTarget != null
            ? lastAttackAnchorTarget.position
            : lastAttackAnchorPosition;
        origin.y = transform.position.y;
        return true;
    }

    private bool IsAttackableTarget(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        var damageable = ResolveDamageableTarget(target);
        return damageable == null || !damageable.IsDead;
    }

    private DamageSystem.IDamageable ResolveDamageableTarget(Transform target)
    {
        if (target == null) return null;

        var direct = target.GetComponent<DamageSystem.IDamageable>();
        if (direct != null) return direct;

        var parent = target.GetComponentInParent<DamageSystem.IDamageable>();
        if (parent != null) return parent;

        var root = target.root;
        if (root == null) return null;

        var components = root.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] is DamageSystem.IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }

    private Transform ResolveAttackTargetIdentity(Transform target)
    {
        if (target == null) return null;

        var damageable = ResolveDamageableTarget(target);
        if (damageable is Component component)
        {
            return component.transform;
        }

        return target;
    }

    private bool AreSameAttackTargets(Transform first, Transform second)
    {
        if (first == null || second == null) return false;
        return ResolveAttackTargetIdentity(first) == ResolveAttackTargetIdentity(second);
    }

    private Transform GetIgnoreTarget(bool isChainActive, Vector3 aimDirection)
    {
        if (!isChainActive || !ignoreLastTargetDuringChain) return null;
        var lastTarget = GetSameTargetCandidate();
        if (lastTarget == null) return null;
        if (HasSameTargetReattackRequest()) return null;
        if (useSameTargetRelease && !sameTargetReleased && AreSameAttackTargets(lastAttackTarget, lastTarget))
        {
            return lastTarget;
        }
        if (IsSameTargetAllowed(lastTarget, aimDirection)) return null;
        return lastTarget;
    }

    private bool TryGetForcedSameTarget(Vector3 rawAimDirection, out Transform target, out Vector3 aimDirection)
    {
        target = null;
        aimDirection = rawAimDirection;

        if (!HasSameTargetReattackRequest()) return false;
        if (!allowForcedSameTargetReattack) return false;
        var lastTarget = GetSameTargetCandidate();
        if (lastTarget == null) return false;

        target = lastTarget;

        var toTarget = lastTarget.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0f)
        {
            aimDirection = toTarget.normalized;
            return true;
        }

        rawAimDirection.y = 0f;
        if (rawAimDirection.sqrMagnitude > 0f)
        {
            aimDirection = rawAimDirection.normalized;
            return true;
        }

        aimDirection = transform.forward;
        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude > 0f)
        {
            aimDirection = aimDirection.normalized;
            return true;
        }

        aimDirection = Vector3.forward;
        return true;
    }

    private bool HasSameTargetReattackRequest()
    {
        return sameTargetReattackReady;
    }

    private bool IsSameTargetAllowed(Transform target, Vector3 aimDirection)
    {
        if (target == null) return false;

        var toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0f) return true;

        var angle = Vector3.Angle(aimDirection, toTarget.normalized);
        return angle <= allowSameTargetAngle;
    }

    private void RegisterAttackTarget(Transform target, Vector3 attackDirection)
    {
        if (target == null) return;
        var consumedSameTargetRequest = HasSameTargetReattackRequest() && AreSameAttackTargets(lastAttackTarget, target);
        lastAttackTarget = target;
        lastAttackAnchorTarget = target;
        lastAttackAnchorPosition = target.position;
        hasLastAttackAnchor = true;
        sameTargetReleased = false;
        sameTargetReleaseTimer = 0f;
        sameTargetAutoReleaseTimer = 0f;
        sameTargetReattackInputBufferTimer = 0f;
        sameTargetReattackReady = false;
        sameTargetReattackNeedsRelease = consumedSameTargetRequest;
        sameTargetReattackAimHoldTimer = 0f;
        sameTargetReattackInputRegion = SameTargetReattackInputRegion.None;
        attackDirection.y = 0f;
        if (attackDirection.sqrMagnitude > 0f)
        {
            lastAttackAimDirection = attackDirection.normalized;
            hasLastAttackAim = true;
        }
        else
        {
            hasLastAttackAim = false;
        }
        CaptureLastAttackInput();
    }

    private void ResetSameTargetRelease()
    {
        lastAttackTarget = null;
        sameTargetReleased = true;
        hasLastAttackAim = false;
        hasLastAttackInput = false;
        sameTargetReleaseTimer = 0f;
        sameTargetAutoReleaseTimer = 0f;
        ClearSameTargetReattackState();
    }

    private void ClearLastAttackAnchor()
    {
        lastAttackAnchorTarget = null;
        lastAttackAnchorPosition = transform.position;
        hasLastAttackAnchor = false;
    }

    private void UpdateSameTargetRelease(Vector3 rawAimDirection)
    {
        if (!useSameTargetRelease) return;
        if (sameTargetReleaseAngle <= 0f)
        {
            sameTargetReleased = true;
            return;
        }
        if (lastAttackTarget == null) return;
        if (sameTargetReleased) return;

        var releaseHoldTime = chainTargetConfirmTime > 0f ? chainTargetConfirmTime : 0.05f;
        var delta = Time.unscaledDeltaTime;
        if (sameTargetAutoReleaseTime > 0f)
        {
            sameTargetAutoReleaseTimer += delta;
            if (sameTargetAutoReleaseTimer >= sameTargetAutoReleaseTime)
            {
                sameTargetReleased = true;
                sameTargetReleaseTimer = 0f;
                return;
            }
        }

        if (moveController != null && hasLastAttackInput)
        {
            var input = moveController.GetAimInput();
            if (input.sqrMagnitude > 0f)
            {
                var inputAngle = Vector2.Angle(input, lastAttackInput);
                if (inputAngle >= sameTargetReleaseAngle)
                {
                    sameTargetReleaseTimer += delta;
                    if (sameTargetReleaseTimer >= releaseHoldTime)
                    {
                        sameTargetReleased = true;
                    }
                    return;
                }

                sameTargetReleaseTimer = 0f;
                return;
            }
        }

        if (!hasLastAttackAim) return;

        rawAimDirection.y = 0f;
        if (rawAimDirection.sqrMagnitude <= 0f) return;
        var aimAngle = Vector3.Angle(rawAimDirection, lastAttackAimDirection);
        if (aimAngle >= sameTargetReleaseAngle)
        {
            sameTargetReleaseTimer += delta;
            if (sameTargetReleaseTimer >= releaseHoldTime)
            {
                sameTargetReleased = true;
            }
            return;
        }
        sameTargetReleaseTimer = 0f;
    }

    private void UpdateSameTargetReattackIntent(Vector3 rawAimDirection, float delta, bool isChainActive)
    {
        if (!isChainActive)
        {
            ClearSameTargetReattackState();
            return;
        }

        if (!allowForcedSameTargetReattack)
        {
            ClearSameTargetReattackState();
            return;
        }

        var sameTarget = GetSameTargetCandidate();
        if (sameTarget == null)
        {
            ClearSameTargetReattackState();
            return;
        }

        if (!useSameTargetReattackInputGate)
        {
            ClearSameTargetReattackState();
            return;
        }

        var hasAimInput = TryGetCurrentSameTargetReattackDirection(rawAimDirection, out var currentAimDirection);
        var inputRegion = hasAimInput
            ? EvaluateSameTargetReattackInputRegion(currentAimDirection)
            : SameTargetReattackInputRegion.None;

        if (sameTargetReattackNeedsRelease && inputRegion == SameTargetReattackInputRegion.None)
        {
            sameTargetReattackNeedsRelease = false;
        }

        if (inputRegion == SameTargetReattackInputRegion.None)
        {
            sameTargetReattackAimHoldTimer = 0f;
            sameTargetReattackInputRegion = SameTargetReattackInputRegion.None;

            if (sameTargetReattackInputBufferTimer > 0f)
            {
                if ((dashController != null && dashController.IsDashing) || isReadyWaiting)
                {
                    sameTargetReattackReady = true;
                    return;
                }

                sameTargetReattackInputBufferTimer = Mathf.Max(0f, sameTargetReattackInputBufferTimer - Mathf.Max(0f, delta));
            }

            sameTargetReattackReady = sameTargetReattackInputBufferTimer > 0f;
            return;
        }

        if (sameTargetReattackNeedsRelease)
        {
            sameTargetReattackInputRegion = inputRegion;
            sameTargetReattackAimHoldTimer = 0f;
            sameTargetReattackReady = sameTargetReattackInputBufferTimer > 0f;
            return;
        }

        if (inputRegion == SameTargetReattackInputRegion.Opposite)
        {
            QueueSameTargetReattackRequest();
            return;
        }

        if (dashController != null && dashController.IsDashing)
        {
            sameTargetReattackInputRegion = inputRegion;
            sameTargetReattackAimHoldTimer = 0f;
            sameTargetReattackReady = sameTargetReattackInputBufferTimer > 0f;
            return;
        }

        if (isReadyWaiting)
        {
            sameTargetReattackInputRegion = inputRegion;
            sameTargetReattackAimHoldTimer = 0f;
            sameTargetReattackReady = sameTargetReattackInputBufferTimer > 0f;
            return;
        }

        if (sameTargetReattackInputRegion != inputRegion)
        {
            sameTargetReattackAimHoldTimer = 0f;
            sameTargetReattackInputRegion = inputRegion;
        }

        sameTargetReattackAimHoldTimer += Mathf.Max(0f, delta);
        if (sameTargetReattackAimHoldTimer < Mathf.Max(0f, sameTargetReattackHoldTime))
        {
            sameTargetReattackReady = sameTargetReattackInputBufferTimer > 0f;
            return;
        }

        QueueSameTargetReattackRequest();
    }

    private void QueueSameTargetReattackRequest()
    {
        sameTargetReattackNeedsRelease = true;
        sameTargetReattackAimHoldTimer = 0f;

        var requestLifetime = Mathf.Max(0.001f, sameTargetReattackInputBufferTime);
        if ((dashController != null && dashController.IsDashing) || isReadyWaiting)
        {
            requestLifetime = Mathf.Max(requestLifetime, Mathf.Max(0f, sameTargetReattackDashLatchTime));
        }

        sameTargetReattackInputBufferTimer = Mathf.Max(sameTargetReattackInputBufferTimer, requestLifetime);
        sameTargetReattackReady = true;
    }

    private SameTargetReattackInputRegion EvaluateSameTargetReattackInputRegion(Vector3 currentAimDirection)
    {
        if (IsOppositeSameTargetInput(currentAimDirection))
        {
            return SameTargetReattackInputRegion.Opposite;
        }

        if (IsSameTargetDirectionInput(currentAimDirection))
        {
            return SameTargetReattackInputRegion.SameTarget;
        }

        return SameTargetReattackInputRegion.None;
    }

    private bool IsSameTargetDirectionInput(Vector3 currentAimDirection)
    {
        if (!TryGetSameTargetDirection(out var sameTargetDirection)) return false;

        var normalizedAimDirection = currentAimDirection.normalized;
        var sameTargetAngleLimit = Mathf.Max(0f, Mathf.Max(allowSameTargetAngle, sameTargetReattackInputAngle));
        var sameTargetAngle = Vector3.Angle(normalizedAimDirection, sameTargetDirection);
        return sameTargetAngle <= sameTargetAngleLimit;
    }

    private bool IsOppositeSameTargetInput(Vector3 currentAimDirection)
    {
        if (!hasLastAttackAim) return false;

        var normalizedAimDirection = currentAimDirection.normalized;
        var oppositeDirection = -lastAttackAimDirection;
        oppositeDirection.y = 0f;
        if (oppositeDirection.sqrMagnitude <= 0f) return false;

        var oppositeAngle = Vector3.Angle(normalizedAimDirection, oppositeDirection.normalized);
        return oppositeAngle <= Mathf.Max(0f, sameTargetReattackOppositeAngle);
    }

    private bool TryGetCurrentSameTargetReattackDirection(Vector3 rawAimDirection, out Vector3 aimDirection)
    {
        aimDirection = rawAimDirection;
        if (moveController != null)
        {
            if (!moveController.HasAimInput(GetChainInputDeadZone()))
            {
                return false;
            }

            aimDirection = moveController.GetAimDirection();
        }

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude <= 0f)
        {
            return false;
        }

        aimDirection = aimDirection.normalized;
        return true;
    }


    private bool TryGetSameTargetDirection(out Vector3 sameTargetDirection)
    {
        sameTargetDirection = Vector3.zero;

        var sameTarget = GetSameTargetCandidate();
        if (sameTarget == null) return false;

        sameTargetDirection = sameTarget.position - transform.position;
        sameTargetDirection.y = 0f;
        if (sameTargetDirection.sqrMagnitude <= 0f) return false;

        sameTargetDirection = sameTargetDirection.normalized;
        return true;
    }

    private void CaptureLastAttackInput()
    {
        if (moveController == null)
        {
            hasLastAttackInput = false;
            return;
        }

        var input = moveController.GetAimInput();
        if (input.sqrMagnitude <= 0f)
        {
            hasLastAttackInput = false;
            return;
        }

        lastAttackInput = input.normalized;
        hasLastAttackInput = true;
    }
}
