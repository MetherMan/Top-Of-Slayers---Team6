using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Slider hpSlider;

    [SerializeField] private Slider effectSlider;
    [SerializeField] private float damageSpeed;
    [SerializeField] private float waitTime = 0.5f;

    private Camera mainCamera;
    private float timer;

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

        effectSlider.maxValue = playerHP.maxHP;
        effectSlider.value = playerHP.currentHP;
    }

    private void OnEnable()
    {
        if (playerHP != null) playerHP.OnHPChanged += UpdateHPUI;
    }

    private void OnDisable()
    {
        if (playerHP != null) playerHP.OnHPChanged -= UpdateHPUI;
    }

    private void Update()
    {
        if(effectSlider.value > hpSlider.value)
        {
            timer += Time.deltaTime;

            if (timer > waitTime)
            {
                effectSlider.value = Mathf.Lerp(effectSlider.value, hpSlider.value, Time.deltaTime * damageSpeed);
            }
        }
    }

    private void LateUpdate()
    {
        //플레이어 머리 위 월드 좌표
        Vector3 worldPos = playerTransform.position + Vector3.right * 1.5f + Vector3.up * 1f;

        //월드좌표를 스크린 좌표로 변환
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        transform.position = screenPos;
        effectSlider.transform.position = screenPos;
    }

    private void UpdateHPUI(int currentHp, int maxHp)
    {
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;

        timer = 0f;

        if(currentHp > effectSlider.value)
        {
            effectSlider.value = currentHp;
        }
    }
}
