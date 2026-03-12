using UnityEngine;

public partial class PlayerMoveController
{
    private const float RotationInputReleaseDeadZone = 0.05f;
    private bool hasReadyFacingDirection;
    private bool hasDashFacingDirection;
    private bool hasSlashFacingDirection;
    private Vector3 readyFacingDirection = Vector3.forward;
    private Vector3 dashFacingDirection = Vector3.forward;
    private Vector3 slashFacingDirection = Vector3.forward;

    private enum RotationOverrideSource
    {
        None = 0,
        Ready = 1,
        Dash = 2,
        Slash = 3
    }

    public void SetReadyFacingDirection(Vector3 direction)
    {
        SetRotationOverride(RotationOverrideSource.Ready, direction);
    }

    public void ClearReadyFacingDirection()
    {
        ClearRotationOverride(RotationOverrideSource.Ready);
    }

    public void SetDashFacingDirection(Vector3 direction)
    {
        SetRotationOverride(RotationOverrideSource.Dash, direction);
    }

    public void ClearDashFacingDirection()
    {
        ClearRotationOverride(RotationOverrideSource.Dash);
    }

    public void SetSlashFacingDirection(Vector3 direction)
    {
        SetRotationOverride(RotationOverrideSource.Slash, direction);
    }

    public void ClearSlashFacingDirection()
    {
        ClearRotationOverride(RotationOverrideSource.Slash);
    }

    public void BlockInputRotationUntilRelease()
    {
        blockInputRotationUntilRelease = true;
    }

    public Vector3 GetAimDirection()
    {
        var input = GetRealtimeInput();
        if (input == Vector2.zero)
        {
            return transform.forward;
        }

        var moveInput = new Vector3(input.x, 0f, input.y);
        var direction = GetMoveDirection(moveInput);
        if (direction.sqrMagnitude <= 0f)
        {
            return transform.forward;
        }

        return direction.normalized;
    }

    private void ApplyAuthoritativeRotation()
    {
        if (TryGetRotationOverride(out var overrideDirection, out var overrideSource))
        {
            ApplyRotation(overrideDirection, true);
            return;
        }

        if (TryGetInputFacingDirection(false, out var inputDirection))
        {
            ApplyRotation(inputDirection);
        }
    }

    private bool TryGetInputFacingDirection(bool ignoreLock, out Vector3 direction)
    {
        direction = Vector3.zero;
        if (ShouldBlockInputDrivenRotation()) return false;
        if (IsMovementBlocked() && !allowRotationWhenLocked && !ignoreLock) return false;
        if (IsRotationLocked && !ignoreLock) return false;

        var input = GetRealtimeInput();
        if (input == Vector2.zero) return false;

        var moveInput = new Vector3(input.x, 0f, input.y);
        direction = GetMoveDirection(moveInput);
        if (direction == Vector3.zero) return false;

        return true;
    }

    private void ApplyRotation(Vector3 moveDirection, bool ignoreLock = false)
    {
        if (!ignoreLock && IsRotationLocked) return;
        if (moveDirection.sqrMagnitude <= 0f) return;

        var targetDirection = moveDirection.normalized;
        var targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        if (cachedRigidbody != null)
        {
            if (useFixedUpdate && Time.inFixedTimeStep && !cachedRigidbody.isKinematic)
            {
                cachedRigidbody.MoveRotation(targetRotation);
            }
            else
            {
                cachedRigidbody.rotation = targetRotation;
            }
        }

        // 물리 바디 갱신 타이밍과 무관하게 같은 프레임에 루트 Transform도 바로 맞춘다.
        transform.rotation = targetRotation;
    }

    private bool ShouldBlockInputDrivenRotation()
    {
        if (!blockInputRotationUntilRelease) return false;

        var input = GetRealtimeInput();
        var releaseDeadZone = Mathf.Max(RotationInputReleaseDeadZone, keyboardDeadZone);
        if (input.sqrMagnitude <= releaseDeadZone * releaseDeadZone)
        {
            blockInputRotationUntilRelease = false;
            return false;
        }

        return true;
    }

    private void SetRotationOverride(RotationOverrideSource source, Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0f) return;

        var normalizedDirection = direction.normalized;
        switch (source)
        {
            case RotationOverrideSource.Ready:
                hasReadyFacingDirection = true;
                readyFacingDirection = normalizedDirection;
                break;
            case RotationOverrideSource.Dash:
                hasDashFacingDirection = true;
                dashFacingDirection = normalizedDirection;
                break;
            case RotationOverrideSource.Slash:
                hasSlashFacingDirection = true;
                slashFacingDirection = normalizedDirection;
                break;
        }
    }

    private void ClearRotationOverride(RotationOverrideSource source)
    {
        if (source == RotationOverrideSource.None) return;

        switch (source)
        {
            case RotationOverrideSource.Ready:
                hasReadyFacingDirection = false;
                break;
            case RotationOverrideSource.Dash:
                hasDashFacingDirection = false;
                break;
            case RotationOverrideSource.Slash:
                hasSlashFacingDirection = false;
                break;
        }
    }

    private bool TryGetRotationOverride(out Vector3 direction, out RotationOverrideSource source)
    {
        direction = Vector3.zero;
        source = RotationOverrideSource.None;

        if (hasSlashFacingDirection)
        {
            direction = slashFacingDirection;
            source = RotationOverrideSource.Slash;
            return direction.sqrMagnitude > 0f;
        }

        if (hasDashFacingDirection)
        {
            direction = dashFacingDirection;
            source = RotationOverrideSource.Dash;
            return direction.sqrMagnitude > 0f;
        }

        if (hasReadyFacingDirection)
        {
            direction = readyFacingDirection;
            source = RotationOverrideSource.Ready;
            return direction.sqrMagnitude > 0f;
        }

        return false;
    }
    private Vector3 GetMoveDirection(Vector3 moveInput)
    {
        if (!useCameraRelative)
        {
            return moveInput;
        }

        var cam = ResolveCameraTransform();
        if (cam == null)
        {
            return moveInput;
        }

        var magnitude = moveInput.magnitude;
        if (magnitude <= 0f) return Vector3.zero;

        // 카메라가 수직에 가깝게 내려다보면 forward의 수평 성분이 0에 가까워진다.
        // 이 경우 cam.up을 대체 축으로 써서 상하 이동 축이 사라지지 않게 보정한다.
        var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(cam.up, Vector3.up);
        }
        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        }
        if (forward.sqrMagnitude > 0f) forward.Normalize();

        var right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude > 0f) right.Normalize();

        var direction = right * moveInput.x + forward * moveInput.z;
        if (direction.sqrMagnitude <= 0f) return Vector3.zero;

        return direction.normalized * magnitude;
    }

    private Transform ResolveCameraTransform()
    {
        if (!autoFindCamera)
        {
            return cameraTransform;
        }

        if (cameraTransform != null && cameraTransform.gameObject.activeInHierarchy)
        {
            return cameraTransform;
        }

        if (Time.unscaledTime < nextCameraResolveTime)
        {
            return cameraTransform;
        }

        nextCameraResolveTime = Time.unscaledTime + cameraResolveInterval;

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            return cameraTransform;
        }

        var anyCamera = FindObjectOfType<Camera>();
        if (anyCamera != null)
        {
            cameraTransform = anyCamera.transform;
        }

        return cameraTransform;
    }
}
