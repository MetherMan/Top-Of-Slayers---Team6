using UnityEngine;
using UnityEngine.UI;

public class EnemyHPUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;

    private DummyTarget target;
    private Camera mainCamera;
    private GameObject prefab;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Init(DummyTarget target, GameObject prefab)
    {
        if (this.target != null)
        {
            this.target.OnHPChanged -= UpdateHPUI;
        }

        this.target = target;
        this.prefab = prefab;

        hpSlider.maxValue = target.maxHp;
        hpSlider.value = target.currentHp;

        target.OnHPChanged += UpdateHPUI;
    }

    private void LateUpdate()
    {
        if(target == null || target.IsDead)
        {
            ObjectPoolManager.Instance.ReturnPool(prefab, gameObject);
            return;
        }

        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.transform.position + Vector3.right * 1.5f);

        transform.position = screenPos;
    }

    private void UpdateHPUI(int currentHP, int maxHP)
    {
        hpSlider.value = currentHP;
        hpSlider.maxValue = maxHP;
    }
}
