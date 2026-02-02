using UnityEngine;

public class AttackAimGuide : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private AttackInputController input;
    [SerializeField] private LineRenderer line;
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField, Min(0f)] private float overshootDistance = 0.5f;

    [Header("표시")]
    [SerializeField] private float lineLength = 8f;
    [SerializeField] private Vector3 originOffset = new Vector3(0f, 0.1f, 0f);

    private void Awake()
    {
        if (line != null)
        {
            line.positionCount = 2;
            line.enabled = false;
        }
    }

    private void Update()
    {
        if (input == null || line == null) return;

        if (!input.IsCharging)
        {
            if (line.enabled) line.enabled = false;
            return;
        }

        if (!line.enabled) line.enabled = true;

        var origin = transform.position + originOffset;
        var direction = input.AimDirection;
        var length = input.AimDistance > 0f ? input.AimDistance : input.DefaultDashDistance;

        if (targetingSystem != null && direction.sqrMagnitude > 0f)
        {
            var target = targetingSystem.GetTarget(transform.position, direction, length);
            if (target != null)
            {
                var toTarget = target.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0f)
                {
                    direction = toTarget.normalized;
                    length = toTarget.magnitude + Mathf.Max(0f, overshootDistance);
                }
            }
        }

        if (direction.sqrMagnitude <= 0f)
        {
            direction = transform.forward;
        }

        if (length <= 0f)
        {
            length = lineLength;
        }

        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction.normalized * length);
    }
}
