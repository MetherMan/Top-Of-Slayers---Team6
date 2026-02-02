using System;
using UnityEngine;

public class AttackInputController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private AttackTimingSystem timingSystem;
    [SerializeField] private AttackSpecSO spec;
    [SerializeField] private PlayerMoveController moveController;

    [Header("입력 게이지")]
    [SerializeField, Range(0.1f, 3f)] private float chargeSpeed = 1f;
    [SerializeField] private bool useTimingGauge = false;
    [SerializeField, Min(0f)] private float postAttackLockDuration = 0.5f;

    public event Action<float> OnChargeChanged;
    public event Action<TimingGrade> OnAttackTriggered;
    public event Action<Vector3> OnAimChanged;

    private IInputCommand startCommand;
    private IInputCommand updateCommand;
    private IInputCommand releaseCommand;

    private bool isCharging;
    private float charge01;
    private float cooldownTimer;
    private Vector3 aimDirection;
    private float aimDistance;
    private float postAttackLockTimer;
    private bool isInputLockActive;

    private void Awake()
    {
        if (timingSystem == null)
        {
            timingSystem = GetComponent<AttackTimingSystem>();
        }

        if (moveController == null)
        {
            moveController = GetComponent<PlayerMoveController>();
        }

        if (moveController == null)
        {
            moveController = GetComponentInParent<PlayerMoveController>();
        }

        if (timingSystem != null && spec != null)
        {
            timingSystem.SetSpec(spec);
        }

        startCommand = new StartChargeCommand();
        updateCommand = new UpdateChargeCommand();
        releaseCommand = new ReleaseChargeCommand();
        aimDirection = transform.forward;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (postAttackLockTimer > 0f)
        {
            postAttackLockTimer -= Time.deltaTime;
            if (postAttackLockTimer <= 0f)
            {
                ReleaseInputLock();
            }
        }

        if (!isCharging) return;

        updateCommand.Execute(this, Time.deltaTime);
    }

    private void OnDisable()
    {
        postAttackLockTimer = 0f;
        ReleaseInputLock();
    }

    public void BeginInput()
    {
        if (cooldownTimer > 0f) return;
        startCommand.Execute(this, 0f);
    }

    public void EndInput()
    {
        if (!isCharging) return;
        releaseCommand.Execute(this, 0f);

        if (postAttackLockDuration > 0f)
        {
            AcquireInputLock();
            postAttackLockTimer = postAttackLockDuration;
        }
        else
        {
            ReleaseInputLock();
        }
    }

    public float GetCharge01()
    {
        return charge01;
    }

    public bool IsCharging => isCharging;

    public Vector3 AimDirection => aimDirection;
    public float AimDistance => aimDistance;
    public float DefaultDashDistance
    {
        get
        {
            if (spec == null) return 0f;
            return Mathf.Max(0f, spec.dashSpeed * spec.dashDuration);
        }
    }

    public void SetAimDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0f) return;
        aimDirection = direction.normalized;
        OnAimChanged?.Invoke(aimDirection);
    }

    public void SetAimDistance(float distance)
    {
        aimDistance = Mathf.Max(0f, distance);
    }

    private void SetCharge01(float value)
    {
        charge01 = Mathf.Clamp01(value);
        OnChargeChanged?.Invoke(charge01);
    }

    private TimingGrade EvaluateTiming()
    {
        if (!useTimingGauge)
        {
            return TimingGrade.Good;
        }

        if (timingSystem == null) return TimingGrade.Miss;
        return timingSystem.EvaluateTiming(charge01);
    }

    private void StartCharge()
    {
        isCharging = true;
        SetCharge01(0f);
        aimDistance = 0f;
        SetAimDirection(transform.forward);
    }

    private void UpdateCharge(float deltaTime)
    {
        if (!isCharging) return;
        SetCharge01(charge01 + deltaTime * chargeSpeed);
    }

    private void ReleaseCharge()
    {
        isCharging = false;

        var grade = EvaluateTiming();
        OnAttackTriggered?.Invoke(grade);

        SetCharge01(0f);
        aimDistance = 0f;

        if (spec != null && spec.cooldown > 0f)
        {
            cooldownTimer = spec.cooldown;
        }
    }

    private void AcquireInputLock()
    {
        if (moveController == null) return;
        if (isInputLockActive) return;
        isInputLockActive = true;
        moveController.SetMovementLocked(true);
    }

    private void ReleaseInputLock()
    {
        if (moveController == null) return;
        if (!isInputLockActive) return;
        isInputLockActive = false;
        moveController.SetMovementLocked(false);
    }

    public interface IInputCommand
    {
        void Execute(AttackInputController controller, float deltaTime);
    }

    private class StartChargeCommand : IInputCommand
    {
        public void Execute(AttackInputController controller, float deltaTime)
        {
            controller.StartCharge();
        }
    }

    private class UpdateChargeCommand : IInputCommand
    {
        public void Execute(AttackInputController controller, float deltaTime)
        {
            controller.UpdateCharge(deltaTime);
        }
    }

    private class ReleaseChargeCommand : IInputCommand
    {
        public void Execute(AttackInputController controller, float deltaTime)
        {
            controller.ReleaseCharge();
        }
    }
}
