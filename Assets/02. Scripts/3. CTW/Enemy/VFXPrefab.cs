using UnityEngine;

public class VFXPrefab : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float lifeTime = 1f;

    private void OnEnable()
    {
        Invoke(nameof(ReturnPool), lifeTime);
    }

    private void ReturnPool()
    {
        ObjectPoolManager.Instance.ReturnPool(prefab, gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
