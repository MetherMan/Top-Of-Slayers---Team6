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

        ApplyRotation(direction);
    }

    public void ForceLookDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0f) return;
        ApplyRotation(direction);
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

    private void ApplyRotation(Vector3 moveDirection)
    {
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

        var forward = cam.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0f) forward.Normalize();

        var right = cam.right;
        right.y = 0f;
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
