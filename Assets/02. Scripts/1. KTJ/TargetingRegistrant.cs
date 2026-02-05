using UnityEngine;

public class TargetingRegistrant : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private bool autoFindTargetingSystem = true;

    private bool isRegistered;

    private void Awake()
    {
        if (targetTransform == null) targetTransform = transform;
        ResolveTargetingSystem();
    }

    private void OnEnable()
    {
        ResolveTargetingSystem();
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void Register()
    {
        if (isRegistered) return;
        if (targetingSystem == null) return;
        if (targetTransform == null) return;

        targetingSystem.RegisterTarget(targetTransform);
        isRegistered = true;
    }

    private void Unregister()
    {
        if (!isRegistered) return;
        if (targetingSystem == null) return;
        if (targetTransform == null) return;

        targetingSystem.UnregisterTarget(targetTransform);
        isRegistered = false;
    }

    private void ResolveTargetingSystem()
    {
        if (!autoFindTargetingSystem) return;
        if (targetingSystem != null) return;

        targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = FindObjectOfType<TargetingSystem>();
    }
}
