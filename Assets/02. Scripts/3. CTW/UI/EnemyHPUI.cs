using UnityEngine;
using UnityEngine.UI;

public class EnemyHPUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider effectSlider;
    [SerializeField] private float damageSpeed = 2f;
    [SerializeField] private float waitTime = 0.5f;
    private float timer;

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

        effectSlider.maxValue = target.maxHp;
        effectSlider.value = target.currentHp;

        target.OnHPChanged += UpdateHPUI;
    }

    private void Update()
    {
        if(effectSlider.value > hpSlider.value)
        {
            timer += Time.deltaTime;

            if(timer > waitTime)
            {
                effectSlider.value = Mathf.Lerp(effectSlider.value, hpSlider.value, Time.deltaTime * damageSpeed);
            }
        }
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

        timer = 0f;

        if(currentHP > effectSlider.value)
        {
            effectSlider.value = currentHP;
        }
    }
}
