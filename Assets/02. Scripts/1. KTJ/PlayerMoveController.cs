using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool useFixedUpdate = true;

    [Header("입력 소스")]
    [SerializeField] private VirtualJoystickController joystick;
    [SerializeField] private bool useKeyboardInput = false;
    [SerializeField, Range(0f, 1f)] private float keyboardDeadZone = 0.1f;
    [SerializeField] private bool allowRotationWhenLocked = true;

    [Header("체인")]
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private bool lockMovementDuringChain = true;

    [Header("카메라 기준")]
    [SerializeField] private bool useCameraRelative = true;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool autoFindCamera = true;

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
    private bool chainLockApplied;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        moveCommand = new MoveCommand();
        stopCommand = new StopCommand();
        currentCommand = stopCommand;

        ResolveCameraTransform();
        ResolveChainCombat();
    }

    private void OnEnable()
    {
        if (joystick != null)
        {
            joystick.OnInputChanged += HandleInputChanged;
            joystick.OnInputReleased += HandleInputReleased;
        }

        if (lockMovementDuringChain)
        {
            ResolveChainCombat();
            if (chainCombat != null)
            {
                chainCombat.OnSlowStateChanged += HandleChainSlowStateChanged;
                ApplyChainLock(chainCombat.IsSlowActive);
            }
        }
    }

    private void OnDisable()
    {
        if (joystick != null)
        {
            joystick.OnInputChanged -= HandleInputChanged;
            joystick.OnInputReleased -= HandleInputReleased;
        }

        if (chainCombat != null)
        {
            chainCombat.OnSlowStateChanged -= HandleChainSlowStateChanged;
        }
        ApplyChainLock(false);
    }

    private void Update()
    {
        if (!useFixedUpdate)
        {
            ExecuteNextCommand(Time.deltaTime);
            return;
        }

        if (movementLocked && allowRotationWhenLocked && !Mathf.Approximately(Time.timeScale, 0f))
        {
            UpdateLockedRotation();
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

    private void UpdateLockedRotation()
    {
        var input = GetRealtimeInput();
        if (input == Vector2.zero) return;

        var moveInput = new Vector3(input.x, 0f, input.y);
        var direction = GetMoveDirection(moveInput);
        if (direction == Vector3.zero) return;

        ApplyRotation(direction);
    }

    private void HandleChainSlowStateChanged(bool isActive)
    {
        ApplyChainLock(isActive);
    }

    private void ApplyChainLock(bool isActive)
    {
        if (isActive)
        {
            if (!lockMovementDuringChain) return;
            if (chainLockApplied) return;
            chainLockApplied = true;
            AddMovementLock();
            return;
        }

        if (!chainLockApplied) return;
        chainLockApplied = false;
        RemoveMovementLock();
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

    public void ForceSyncRotation()
    {
        var input = GetRealtimeInput();
        if (input == Vector2.zero) return;

        var moveInput = new Vector3(input.x, 0f, input.y);
        var direction = GetMoveDirection(moveInput);
        if (direction == Vector3.zero) return;

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

        cachedRigidbody.velocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
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

    private void ResolveChainCombat()
    {
        if (chainCombat != null) return;
        chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();
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
