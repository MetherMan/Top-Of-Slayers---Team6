using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool useFixedUpdate = true;
    [SerializeField, Range(0f, 1f)] private float slowRunThreshold = 0.4f;

    [Header("입력 소스")]
    [SerializeField] private VirtualJoystickController joystick;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveBlendParam = "MoveBlend";

    private IInputCommand currentCommand;
    private Vector2 currentInput;
    private Rigidbody cachedRigidbody;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
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

 /*

  HandleInput (파스칼 케이스) 함수명 클래스 명

    playerHP (카멜 케이스) 변수명 
  
  */

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
            SetCommand(new StopCommand());
            return;
        }

        SetCommand(new MoveCommand(input));
    }

    private void HandleInputReleased()
    {
        currentInput = Vector2.zero;
        UpdateMoveAnimation(Vector2.zero);
        SetCommand(new StopCommand());
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

        if (magnitude <= slowRunThreshold)
        {
            animator.SetFloat(moveBlendParam, magnitude);
            return;
        }

        animator.SetFloat(moveBlendParam, magnitude);
    }

    public interface IInputCommand
    {
        void Execute(PlayerMoveController controller, float deltaTime);
    }

    private class MoveCommand : IInputCommand
    {
        private readonly Vector2 input;

        public MoveCommand(Vector2 input)
        {
            this.input = input;
        }

        public void Execute(PlayerMoveController controller, float deltaTime)
        {
            controller.ApplyMove(input, deltaTime);
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
