using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class VirtualJoystickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("조이스틱")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField, Range(0.1f, 1.0f)] private float handleRange = 0.9f;
    [SerializeField] private float deadZone = 0.1f;

    [Header("동적 위치")]
    [SerializeField] private JoystickPositionMode positionMode = JoystickPositionMode.Fixed;
    [SerializeField] private bool returnToDefault = true;
    [SerializeField] private float returnDuration = 0.1f;
    [SerializeField] private bool hideWhenIdle = false;

    public event Action<Vector2> OnInputChanged;
    public event Action OnInputReleased;

    public Vector2 InputVector { get; private set; }

    private Canvas rootCanvas;
    private Camera uiCamera;
    private Vector2 defaultBackgroundPosition;
    private IJoystickPositionStrategy positionStrategy;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }

        if (background != null)
        {
            defaultBackgroundPosition = background.anchoredPosition;
        }

        positionStrategy = CreatePositionStrategy();
        if (hideWhenIdle && background != null)
        {
            background.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (hideWhenIdle && background != null)
        {
            background.gameObject.SetActive(true);
        }

        positionStrategy?.OnPointerDown(eventData);
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            uiCamera,
            out var localPoint);

        var radius = background.sizeDelta * 0.5f;
        var normalized = new Vector2(localPoint.x / radius.x, localPoint.y / radius.y);

        if (normalized.magnitude < deadZone)
        {
            normalized = Vector2.zero;
        }
        else
        {
            normalized = normalized.normalized * Mathf.Min(normalized.magnitude, 1f);
        }

        InputVector = normalized;
        handle.anchoredPosition = new Vector2(normalized.x * radius.x, normalized.y * radius.y) * handleRange;

        OnInputChanged?.Invoke(InputVector);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }

        OnInputChanged?.Invoke(InputVector);
        OnInputReleased?.Invoke();

        positionStrategy?.OnPointerUp(eventData);
    }

    private IJoystickPositionStrategy CreatePositionStrategy()
    {
        if (positionMode == JoystickPositionMode.Floating)
        {
            return new FloatingJoystickStrategy(this);
        }

        return new FixedJoystickStrategy(this);
    }

    private void SetBackgroundPosition(Vector2 anchoredPosition, bool useTween)
    {
        if (background == null) return;

        if (useTween && returnDuration > 0f)
        {
            background.DOAnchorPos(anchoredPosition, returnDuration).SetEase(Ease.OutQuad);
            return;
        }

        background.anchoredPosition = anchoredPosition;
    }

    private enum JoystickPositionMode
    {
        Fixed,
        Floating
    }

    private interface IJoystickPositionStrategy
    {
        void OnPointerDown(PointerEventData eventData);
        void OnPointerUp(PointerEventData eventData);
    }

    private class FixedJoystickStrategy : IJoystickPositionStrategy
    {
        private readonly VirtualJoystickController owner;

        public FixedJoystickStrategy(VirtualJoystickController owner)
        {
            this.owner = owner;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (owner.hideWhenIdle && owner.background != null)
            {
                owner.background.gameObject.SetActive(false);
            }
        }
    }

    private class FloatingJoystickStrategy : IJoystickPositionStrategy
    {
        private readonly VirtualJoystickController owner;

        public FloatingJoystickStrategy(VirtualJoystickController owner)
        {
            this.owner = owner;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (owner.background == null) return;

            var parent = owner.background.parent as RectTransform;
            if (parent == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                eventData.position,
                owner.uiCamera,
                out var localPoint);

            owner.SetBackgroundPosition(localPoint, false);
            owner.background.anchoredPosition = localPoint;
            owner.handle.anchoredPosition = Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!owner.returnToDefault) return;

            owner.SetBackgroundPosition(owner.defaultBackgroundPosition, true);

            if (owner.hideWhenIdle && owner.background != null)
            {
                owner.background.gameObject.SetActive(false);
            }
        }
    }
}
