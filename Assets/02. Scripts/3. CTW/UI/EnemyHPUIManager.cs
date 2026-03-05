using UnityEngine;

public class EnemyHPUIManager : Singleton<EnemyHPUIManager>
{
    [SerializeField] private GameObject hpBarPrefab;
    [SerializeField] private Transform canvas;
    protected override void Awake()
    {
        base.Awake();
    }
    
    public void CreateHPBar(DummyTarget target)
    {
        GameObject hpBarObj = ObjectPoolManager.Instance.SpawnPool(hpBarPrefab, Vector3.zero, Quaternion.identity);

        if (hpBarObj != null)
        {
            hpBarObj.transform.SetParent(canvas, false);
            EnemyHPUI hpUI = hpBarObj.GetComponent<EnemyHPUI>();
            if (hpUI != null)
            {
                hpUI.Init(target, hpBarPrefab);
            }
        }
    }
}
