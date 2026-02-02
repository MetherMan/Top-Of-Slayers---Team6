using UnityEngine;
using UnityEngine.EventSystems;

public class AttackInputBinder : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private AttackInputController input;
    [SerializeField] private bool endOnExit = false;
    [SerializeField] private Camera aimCamera;
    [SerializeField, Min(0f)] private float minDragPixels = 8f;
    [SerializeField, Min(1f)] private float maxDragPixels = 120f;
    [SerializeField, Min(0f)] private float maxAimDistance = 8f;

    private Vector2 pressScreenPos;

    public void Bind(AttackInputController target)
    {
        input = target;
    }

    public void Clear()
    {
        input = null;
    }

    public void BeginInput()
    {
        input?.BeginInput();
    }

    public void EndInput()
    {
        input?.EndInput();
    }

    public void EndInputOnExit()
    {
        if (!endOnExit) return;
        input?.EndInput();
    }

    public void BeginInputWithData(BaseEventData data)
    {
        if (input == null) return;
        if (!TryGetPointerPosition(data, out var screenPos)) return;

        pressScreenPos = screenPos;
        input.BeginInput();
        UpdateAim(screenPos);
    }

    public void DragInput(BaseEventData data)
    {
        if (input == null) return;
        if (!TryGetPointerPosition(data, out var screenPos)) return;

        UpdateAim(screenPos);
    }

    public void EndInputWithData(BaseEventData data)
    {
        if (input == null) return;
        if (TryGetPointerPosition(data, out var screenPos))
        {
            UpdateAim(screenPos);
        }

        input.EndInput();
    }

    private void UpdateAim(Vector2 currentScreenPos)
    {
        if (input == null) return;

        var drag = currentScreenPos - pressScreenPos;
        if (drag.sqrMagnitude < minDragPixels * minDragPixels)
        {
            input.SetAimDistance(0f);

            var fallback = input.AimDirection.sqrMagnitude > 0f
                ? input.AimDirection
                : input.transform.forward;
            input.SetAimDirection(fallback);
            return;
        }

        if (!TryGetWorldPoint(pressScreenPos, out var startWorld)) return;
        if (!TryGetWorldPoint(currentScreenPos, out var currentWorld)) return;

        var pull = currentWorld - startWorld;
        if (pull.sqrMagnitude <= 0f) return;

        input.SetAimDirection(-pull);
        var distance = pull.magnitude;
        if (maxAimDistance > 0f)
        {
            if (maxDragPixels > 0f)
            {
                var dragRatio = Mathf.Clamp01(drag.magnitude / maxDragPixels);
                distance = dragRatio * maxAimDistance;
            }
            else
            {
                distance = Mathf.Min(distance, maxAimDistance);
            }
        }

        input.SetAimDistance(distance);
    }

    private bool TryGetWorldPoint(Vector2 screenPos, out Vector3 world)
    {
        var cam = aimCamera != null ? aimCamera : Camera.main;
        if (cam == null)
        {
            world = Vector3.zero;
            return false;
        }

        var plane = new Plane(Vector3.up, new Vector3(0f, input.transform.position.y, 0f));
        var ray = cam.ScreenPointToRay(screenPos);
        if (plane.Raycast(ray, out float enter))
        {
            world = ray.GetPoint(enter);
            return true;
        }

        world = Vector3.zero;
        return false;
    }

    private bool TryGetPointerPosition(BaseEventData data, out Vector2 screenPos)
    {
        if (data is PointerEventData pointer)
        {
            screenPos = pointer.position;
            return true;
        }

        screenPos = Vector2.zero;
        return false;
    }
}
