using UnityEngine;

public partial class PlayerMoveController
{
    private void UpdateLockedRotation()
    {
        if (IsRotationLocked) return;
        var input = GetRealtimeInput();
        if (input == Vector2.zero) return;

        var moveInput = new Vector3(input.x, 0f, input.y);
        var direction = GetMoveDirection(moveInput);
        if (direction == Vector3.zero) return;

        ApplyRotation(direction);
    }

    public void ForceSyncRotation()
    {
        var input = GetRealtimeInput();
        if (input == Vector2.zero) return;

        var moveInput = new Vector3(input.x, 0f, input.y);
        var direction = GetMoveDirection(moveInput);
        if (direction == Vector3.zero) return;

        ApplyRotation(direction, true);
    }

    public void ForceLookDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0f) return;
        ApplyRotation(direction, true);
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

    private void ApplyRotation(Vector3 moveDirection, bool ignoreLock = false)
    {
        if (!ignoreLock && IsRotationLocked) return;
        if (moveDirection.sqrMagnitude <= 0f) return;

        var targetDirection = moveDirection.normalized;
        var targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        if (cachedRigidbody != null)
        {
            cachedRigidbody.MoveRotation(targetRotation);
        }
        else
        {
            transform.rotation = targetRotation;
        }
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
