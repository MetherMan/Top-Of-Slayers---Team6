using System.Collections;
using UnityEngine;

public class TargetingRegistrant : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private bool autoFindTargetingSystem = true;

    private bool isRegistered;
    private Coroutine retryRegisterRoutine;

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

        if (!isRegistered)
        {
            StartRetryRegister();
        }
    }

    private void OnDisable()
    {
        StopRetryRegister();
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

    private void StartRetryRegister()
    {
        if (retryRegisterRoutine != null) return;
        retryRegisterRoutine = StartCoroutine(RetryRegisterRoutine());
    }

    private void StopRetryRegister()
    {
        if (retryRegisterRoutine == null) return;
        StopCoroutine(retryRegisterRoutine);
        retryRegisterRoutine = null;
    }

    private IEnumerator RetryRegisterRoutine()
    {
        while (!isRegistered)
        {
            ResolveTargetTransform();
            ResolveTargetingSystem();
            Register();

            if (isRegistered) break;
            yield return null;
        }

        retryRegisterRoutine = null;
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

        // 타겟 등록 지점과 피격(IDamageable) 지점을 맞춰서 체인/피해 누락을 줄인다.
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
            // 리깅 본/메시 자식 기준점은 실제 위치와 어긋날 수 있어 루트를 기준점으로 고정한다.
            targetTransform = root;
            return;
        }

        targetTransform = transform;
    }
}
