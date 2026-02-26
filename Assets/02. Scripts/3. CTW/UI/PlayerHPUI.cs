using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Slider hpSlider;

    private void Awake()
    {
        if(playerHP == null) playerHP = FindObjectOfType<PlayerHP>();
        if(playerTransform == null) playerTransform = playerHP.transform;

        hpSlider.maxValue = playerHP.maxHP;
    }

    private void OnEnable()
    {
        if (playerHP != null) playerHP.OnHPChanged += UpdateHPUI;
    }

    private void OnDisable()
    {
        if (playerHP != null) playerHP.OnHPChanged -= UpdateHPUI;
    }

    private void UpdateHPUI(int currentHp, int maxHp)
    {
        transform.position = playerTransform.position + Vector3.right * 2f;
        hpSlider.value = playerHP.currentHP;
    }
}
