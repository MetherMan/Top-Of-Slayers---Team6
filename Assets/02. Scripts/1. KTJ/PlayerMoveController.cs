using UnityEngine;

public partial class PlayerMoveController : MonoBehaviour
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
    [SerializeField, Min(0.05f)] private float cameraResolveInterval = 0.5f;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveBlendParam = "MoveBlend";
    [SerializeField] private bool keepAnimatorRealtimeDuringChain = true;

    private IInputCommand currentCommand;
    private IInputCommand moveCommand;
    private IInputCommand stopCommand;
    private Vector2 joystickInput;
    private Vector2 currentInput;
    private Rigidbody cachedRigidbody;
    private bool movementLocked;
    private int movementLockCount;
    private bool chainLockApplied;
    private bool rotationLocked;
    private int rotationLockCount;
    private float nextCameraResolveTime;
    private float equipmentMoveSpeedBonus;
    private float specMoveSpeed;
    private SlashDashController dashController;
    private AutoSlashController autoSlashController;
    private PlayerStateMachine playerStateMachine;
    private float animatorBaseSpeed = 1f;
    private AnimatorUpdateMode animatorBaseUpdateMode = AnimatorUpdateMode.Normal;
    private bool hasAnimatorDefaults;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        moveCommand = new MoveCommand();
        stopCommand = new StopCommand();
        currentCommand = stopCommand;

        ResolveJoystick();
        ResolveCameraTransform();
        ResolveChainCombat();
        ResolveLockRecoveryRefs();
        ResolveAnimator();
    }

    private void OnEnable()
    {
        ResolveJoystick();
        ResolveAnimator();
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
        ResetAnimatorTimingCompensation();
    }

    public void BindSceneRefs(
        VirtualJoystickController externalJoystick,
        Transform externalCameraTransform,
        ChainCombatController externalChainCombat)
    {
        if (externalJoystick != null && joystick != externalJoystick)
        {
            if (joystick != null)
            {
                joystick.OnInputChanged -= HandleInputChanged;
                joystick.OnInputReleased -= HandleInputReleased;
            }

            joystick = externalJoystick;
            if (isActiveAndEnabled)
            {
                joystick.OnInputChanged += HandleInputChanged;
                joystick.OnInputReleased += HandleInputReleased;
            }
        }

        if (externalCameraTransform != null)
        {
            cameraTransform = externalCameraTransform;
        }

        if (externalChainCombat != null && chainCombat != externalChainCombat)
        {
            if (chainCombat != null)
            {
                chainCombat.OnSlowStateChanged -= HandleChainSlowStateChanged;
            }

            chainCombat = externalChainCombat;
            if (lockMovementDuringChain && isActiveAndEnabled)
            {
                chainCombat.OnSlowStateChanged += HandleChainSlowStateChanged;
                ApplyChainLock(chainCombat.IsSlowActive);
            }
        }
    }

    private void Update()
    {
        RecoverInvalidMovementLockIfNeeded();
        UpdateAnimatorTimingCompensation();

        if (!useFixedUpdate)
        {
            ExecuteNextCommand(Time.deltaTime);
        }
        else if (IsMovementBlocked() && allowRotationWhenLocked && Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        ApplyAuthoritativeRotation();
    }

    private void FixedUpdate()
    {
        RecoverInvalidMovementLockIfNeeded();

        if (useFixedUpdate)
        {
            ExecuteNextCommand(Time.fixedDeltaTime);
        }
    }

    private void ResolveAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
        }

        if (animator == null) return;
        if (hasAnimatorDefaults) return;
        animatorBaseSpeed = animator.speed;
        animatorBaseUpdateMode = animator.updateMode;
        hasAnimatorDefaults = true;
    }

    private void ResolveLockRecoveryRefs()
    {
        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();
        if (dashController == null) dashController = FindObjectOfType<SlashDashController>();

        if (autoSlashController == null) autoSlashController = GetComponent<AutoSlashController>();
        if (autoSlashController == null) autoSlashController = GetComponentInParent<AutoSlashController>();
        if (autoSlashController == null) autoSlashController = FindObjectOfType<AutoSlashController>();

        if (playerStateMachine == null) playerStateMachine = GetComponent<PlayerStateMachine>();
        if (playerStateMachine == null) playerStateMachine = GetComponentInParent<PlayerStateMachine>();
        if (playerStateMachine == null) playerStateMachine = FindObjectOfType<PlayerStateMachine>();
    }

    private void RecoverInvalidMovementLockIfNeeded()
    {
        if (!movementLocked) return;

        ResolveChainCombat();
        ResolveLockRecoveryRefs();

        if (playerStateMachine != null && playerStateMachine.IsDead) return;
        if (chainCombat != null && chainCombat.IsSlowActive) return;
        if (dashController != null && dashController.IsDashing) return;
        if (autoSlashController != null && autoSlashController.IsReadyWaiting) return;

        movementLockCount = 0;
        movementLocked = false;
    }

    public void SetPlayerMoveSpeed(float speed)
    {
        specMoveSpeed = Mathf.Max(0f, speed);
    }

    public void SetEquipmentMoveSpeedBonus(float bonus)
    {
        equipmentMoveSpeedBonus = Mathf.Max(0f, bonus);
    }

    public float GetCurrentMoveSpeed()
    {
        var baseMoveSpeed = specMoveSpeed > 0f ? specMoveSpeed : Mathf.Max(0f, moveSpeed);
        return Mathf.Max(0f, baseMoveSpeed + equipmentMoveSpeedBonus);
    }

    private void UpdateAnimatorTimingCompensation()
    {
        if (!keepAnimatorRealtimeDuringChain) return;

        ResolveAnimator();
        if (animator == null) return;

        ResolveChainCombat();

        var useUnscaledAnimator = chainCombat != null && chainCombat.IsSlowActive;
        var nextUpdateMode = useUnscaledAnimator
            ? AnimatorUpdateMode.UnscaledTime
            : animatorBaseUpdateMode;

        if (animator.updateMode != nextUpdateMode)
        {
            animator.updateMode = nextUpdateMode;
        }

        if (!Mathf.Approximately(animator.speed, animatorBaseSpeed))
        {
            animator.speed = animatorBaseSpeed;
        }
    }

    private void ResetAnimatorTimingCompensation()
    {
        if (!keepAnimatorRealtimeDuringChain) return;
        if (animator == null) return;
        if (!hasAnimatorDefaults) return;
        animator.speed = animatorBaseSpeed;
        animator.updateMode = animatorBaseUpdateMode;
    }

    private bool IsMovementBlocked()
    {
        if (movementLocked)
        {
            return true;
        }

        if (!lockMovementDuringChain)
        {
            return false;
        }

        if (chainCombat == null)
        {
            ResolveChainCombat();
        }

        return chainCombat != null && chainCombat.IsSlowActive;
    }
}
