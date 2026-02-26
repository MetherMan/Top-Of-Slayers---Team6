using UnityEngine;

public class DamageUI : Singleton<DamageUI>
{
    [SerializeField] private Canvas TWCanvas;
    [SerializeField] private GameObject damageText;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ShowDamage(Transform enemy, int amount)
    {
        if (damageText == null || TWCanvas == null) return;

        GameObject textObj = Instantiate(damageText, TWCanvas.transform);
        textObj.transform.position = enemy.position + Vector3.right * 2f + Vector3.up * 1f; //적 위에 텍스트 위치 조정

        var textComponent = textObj.GetComponent<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = amount.ToString();
        }

        Destroy(textObj, 1f);
    }
}
