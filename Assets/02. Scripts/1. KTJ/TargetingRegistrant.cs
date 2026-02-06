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
        ResolveTargetTransform();
        ResolveTargetingSystem();
    }

    private void OnEnable()
    {
        ResolveTargetTransform();
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

    private void ResolveTargetTransform()
    {
        if (targetTransform != null) return;

        // 타겟 등록 지점과 피격(IDamageable) 지점을 일치시켜 체인/데미지 누락을 방지한다.
        var components = GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] is DamageSystem.IDamageable)
            {
                targetTransform = components[i].transform;
                return;
            }
        }

        var root = transform.root;
        if (root != null)
        {
            var rootComponents = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < rootComponents.Length; i++)
            {
                if (rootComponents[i] is DamageSystem.IDamageable)
                {
                    targetTransform = rootComponents[i].transform;
                    return;
                }
            }
        }

        targetTransform = transform;
    }
}
