using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Slider hpSlider;

    private Camera mainCamera;

    private void Awake()
    {
        if(playerHP == null) playerHP = FindObjectOfType<PlayerHP>();
        if(playerTransform == null) playerTransform = playerHP.transform;

        mainCamera = Camera.main;
    }

    private void Start()
    {
        hpSlider.maxValue = playerHP.maxHP;
        hpSlider.value = playerHP.currentHP;
    }

    private void OnEnable()
    {
        if (playerHP != null) playerHP.OnHPChanged += UpdateHPUI;
    }

    private void OnDisable()
    {
        if (playerHP != null) playerHP.OnHPChanged -= UpdateHPUI;
    }

    private void LateUpdate()
    {
        //플레이어 머리 위 월드 좌표
        Vector3 worldPos = playerTransform.position + Vector3.right * 1.5f + Vector3.up * 1f;

        //월드좌표를 스크린 좌표로 변환
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        transform.position = screenPos;
    }

    private void UpdateHPUI(int currentHp, int maxHp)
    {
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;
    }
}
