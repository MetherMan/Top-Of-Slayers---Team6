using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool useFixedUpdate = true;

    [Header("입력 소스")]
    [SerializeField] private VirtualJoystickController joystick;
    [SerializeField] private bool useKeyboardInput = true;
    [SerializeField, Range(0f, 1f)] private float keyboardDeadZone = 0.1f;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveBlendParam = "MoveBlend";

    private IInputCommand currentCommand;
    private IInputCommand moveCommand;
    private IInputCommand stopCommand;
    private Vector2 joystickInput;
    private Vector2 currentInput;
    private Rigidbody cachedRigidbody;
    private bool movementLocked;
    private int movementLockCount;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        moveCommand = new MoveCommand();
        stopCommand = new StopCommand();
        currentCommand = stopCommand;
    }

    private void OnEnable()
    {
        if (joystick == null) return;
        joystick.OnInputChanged += HandleInputChanged;
        joystick.OnInputReleased += HandleInputReleased;
    }

    private void OnDisable()
    {
        if (joystick == null) return;
        joystick.OnInputChanged -= HandleInputChanged;
        joystick.OnInputReleased -= HandleInputReleased;
    }

    private void Update()
    {
        if (!useFixedUpdate)
        {
            ExecuteNextCommand(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            ExecuteNextCommand(Time.fixedDeltaTime);
        }
    }

    private void HandleInputChanged(Vector2 input)
    {
        joystickInput = input;
    }

    private void HandleInputReleased()
    {
        joystickInput = Vector2.zero;
    }

    private void SetCommand(IInputCommand command)
    {
        currentCommand = command;
    }

    private void ExecuteNextCommand(float deltaTime)
    {
        if (movementLocked)
        {
            currentInput = Vector2.zero;
            currentCommand = stopCommand;
            currentCommand?.Execute(this, deltaTime);
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

        var move = new Vector3(input.x, 0f, input.y) * moveSpeed * deltaTime;

        if (cachedRigidbody != null)
        {
            cachedRigidbody.MovePosition(cachedRigidbody.position + move);
        }
        else
        {
            transform.position += move;
        }

        ApplyRotation(move);
    }

    public void ApplyStop()
    {
        if (cachedRigidbody == null) return;

        cachedRigidbody.velocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
    }

    private void ApplyRotation(Vector3 move)
    {
        if (move.sqrMagnitude <= 0f) return;

        var targetDirection = move.normalized;
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

    private void UpdateMoveAnimation(Vector2 input)
    {
        if (animator == null) return;

        var magnitude = Mathf.Clamp01(input.magnitude);
        animator.SetFloat(moveBlendParam, magnitude);
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

    public interface IInputCommand
    {
        void Execute(PlayerMoveController controller, float deltaTime);
    }

    public void SetMovementLocked(bool locked)
    {
        if (locked)
        {
            AddMovementLock();
        }
        else
        {
            RemoveMovementLock();
        }
    }

    public void AddMovementLock()
    {
        movementLockCount++;
        movementLocked = movementLockCount > 0;
    }

    public void RemoveMovementLock()
    {
        movementLockCount = Mathf.Max(0, movementLockCount - 1);
        movementLocked = movementLockCount > 0;
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
