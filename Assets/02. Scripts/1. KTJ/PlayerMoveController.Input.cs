using UnityEngine;

public partial class PlayerMoveController
{
    private void HandleInputChanged(Vector2 input)
    {
        joystickInput = input;
    }

    private void HandleInputReleased()
    {
        joystickInput = Vector2.zero;
    }

    public Vector2 GetAimInput()
    {
        return GetRealtimeInput();
    }

    public bool HasAimInput(float deadZone)
    {
        if (deadZone < 0f) deadZone = 0f;
        var input = GetRealtimeInput();
        return input.sqrMagnitude >= deadZone * deadZone;
    }

    private Vector2 GetMoveInput()
    {
        if (!useKeyboardInput)
        {
            return Vector2.zero;
        }

        var keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (keyboard.sqrMagnitude > keyboardDeadZone * keyboardDeadZone)
        {
            return Vector2.ClampMagnitude(keyboard, 1f);
        }

        return Vector2.zero;
    }

    private Vector2 GetRealtimeInput()
    {
        var keyboardInput = GetMoveInput();
        if (keyboardInput.sqrMagnitude > 0f)
        {
            return keyboardInput;
        }

        if (joystick != null)
        {
            return joystick.InputVector;
        }

        return joystickInput;
    }
}
