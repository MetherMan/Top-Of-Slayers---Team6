using UnityEngine;

public partial class PlayerMoveController
{
    private void ExecuteNextCommand(float deltaTime)
    {
        if (movementLocked)
        {
            currentInput = Vector2.zero;
            currentCommand = stopCommand;
            currentCommand?.Execute(this, deltaTime);
            if (allowRotationWhenLocked)
            {
                var input = GetRealtimeInput();
                if (input != Vector2.zero)
                {
                    var moveInput = new Vector3(input.x, 0f, input.y);
                    var direction = GetMoveDirection(moveInput);
                    if (direction != Vector3.zero)
                    {
                        ApplyRotation(direction);
                    }
                }
            }
            UpdateMoveAnimation(Vector2.zero);
            return;
        }

        var keyboardInput = GetMoveInput();
        currentInput = keyboardInput.sqrMagnitude > 0f ? keyboardInput : joystickInput;
        currentCommand = currentInput == Vector2.zero ? stopCommand : moveCommand;
        currentCommand?.Execute(this, deltaTime);
        UpdateMoveAnimation(currentInput);
    }

    public void ApplyMove(Vector2 input, float deltaTime)
    {
        if (input == Vector2.zero) return;

        var moveInput = new Vector3(input.x, 0f, input.y);
        var moveDirection = GetMoveDirection(moveInput);
        if (moveDirection == Vector3.zero) return;

        var move = moveDirection * moveSpeed * deltaTime;

        if (cachedRigidbody != null)
        {
            cachedRigidbody.MovePosition(cachedRigidbody.position + move);
        }
        else
        {
            transform.position += move;
        }

        ApplyRotation(moveDirection);
    }

    public void ApplyStop()
    {
        if (cachedRigidbody == null) return;
        if (cachedRigidbody.isKinematic) return;

        cachedRigidbody.velocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
    }

    private void UpdateMoveAnimation(Vector2 input)
    {
        if (animator == null) return;

        var magnitude = Mathf.Clamp01(input.magnitude);
        animator.SetFloat(moveBlendParam, magnitude);
    }

    public interface IInputCommand
    {
        void Execute(PlayerMoveController controller, float deltaTime);
    }

    private class MoveCommand : IInputCommand
    {
        public void Execute(PlayerMoveController controller, float deltaTime)
        {
            controller.ApplyMove(controller.currentInput, deltaTime);
        }
    }

    private class StopCommand : IInputCommand
    {
        public void Execute(PlayerMoveController controller, float deltaTime)
        {
            controller.ApplyStop();
        }
    }
}
