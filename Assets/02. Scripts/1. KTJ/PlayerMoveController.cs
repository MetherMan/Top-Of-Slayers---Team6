using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool useFixedUpdate = true;

    [Header("입력 소스")]
    [SerializeField] private VirtualJoystickController joystick;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveBlendParam = "MoveBlend";

    private IInputCommand currentCommand;
    private IInputCommand moveCommand;
    private IInputCommand stopCommand;
    private Vector2 currentInput;
    private Rigidbody cachedRigidbody;

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
        currentInput = input;
        UpdateMoveAnimation(input);
        if (input == Vector2.zero)
        {
            SetCommand(stopCommand);
            return;
        }

        SetCommand(moveCommand);
    }

    private void HandleInputReleased()
    {
        currentInput = Vector2.zero;
        UpdateMoveAnimation(Vector2.zero);
        SetCommand(stopCommand);
    }

    private void SetCommand(IInputCommand command)
    {
        currentCommand = command;
    }

    private void ExecuteNextCommand(float deltaTime)
    {
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
