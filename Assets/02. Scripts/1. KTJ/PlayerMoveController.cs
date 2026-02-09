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

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        moveCommand = new MoveCommand();
        stopCommand = new StopCommand();
        currentCommand = stopCommand;

        ResolveJoystick();
        ResolveCameraTransform();
        ResolveChainCombat();
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

    private void ResolveAnimator()
    {
        if (animator != null) return;
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
    }

    public void BindSceneRefs(VirtualJoystickController sceneJoystick, Transform sceneCamera, ChainCombatController sceneChainCombat)
    {
        if (sceneJoystick != null)
        {
            joystick = sceneJoystick;
        }

        if (sceneCamera != null)
        {
            cameraTransform = sceneCamera;
        }

        if (sceneChainCombat != null)
        {
            chainCombat = sceneChainCombat;
        }
    }
}
