using System;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Move,
        Attack,
        Chain,
        Dead
    }

    [Header("참조")]
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private ChainCombatController chainCombat;

    [Header("설정")]
    [SerializeField, Range(0f, 1f)] private float moveDeadZone = 0.1f;
    [SerializeField] private bool lockMovementOnDead = true;

    private PlayerState currentState = PlayerState.Idle;
    private bool isDead;

    public PlayerState CurrentState => currentState;
    public bool IsDead => isDead;
    public event Action<PlayerState, PlayerState> OnStateChanged;

    private void Awake()
    {
        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();
        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();
        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
    }

    private void Update()
    {
        EvaluateState(false);
        UpdateRotationLock();
    }

    public void SetDead(bool dead)
    {
        if (isDead == dead) return;
        isDead = dead;
        if (lockMovementOnDead && moveController != null)
        {
            moveController.SetMovementLocked(dead);
        }
        UpdateRotationLock();
        EvaluateState(true);
    }

    private void EvaluateState(bool force)
    {
        var next = ResolveState();
        if (!force && next == currentState) return;

        var previous = currentState;
        currentState = next;
        OnStateChanged?.Invoke(previous, currentState);
    }

    private PlayerState ResolveState()
    {
        if (isDead) return PlayerState.Dead;

        if (dashController != null && dashController.IsDashing)
        {
            return PlayerState.Attack;
        }

        if (chainCombat != null && chainCombat.IsSlowActive)
        {
            return PlayerState.Chain;
        }

        if (moveController != null && !moveController.IsMovementLocked)
        {
            if (moveController.HasAimInput(moveDeadZone))
            {
                return PlayerState.Move;
            }
        }

        return PlayerState.Idle;
    }

    private void UpdateRotationLock()
    {
        if (moveController == null) return;

        if (isDead)
        {
            moveController.SetRotationLocked(true);
            return;
        }

        if (chainCombat != null && chainCombat.IsSlowActive)
        {
            var allowRotation = dashController != null && dashController.IsDashing;
            moveController.SetRotationLocked(!allowRotation);
            return;
        }

        moveController.SetRotationLocked(false);
    }
}
