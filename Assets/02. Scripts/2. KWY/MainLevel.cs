using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainLevel : MonoBehaviour
{
    [SerializeField] Slider levelSlider;
    [SerializeField] TextMeshProUGUI levelCount;

    int currentLevel = 1;
    int currentExp = 0;

    [SerializeField] int maxExp = 100;
    [SerializeField] int stageClearExp = 20;

    private void Start()
    {
        UpdateUI();
    }

    public void AddEXP(int amount)
    {
        currentExp += amount;

        if(currentExp >= maxExp)
        {
            LevelUp();
        }
        UpdateUI();
    }
    private void LevelUp()
    {
        currentLevel++;
        currentExp = 0;

        maxExp += 30;
    }

    private void UpdateUI()
    {
        levelSlider.maxValue = maxExp;
        levelSlider.value = currentExp;

        levelCount.text = $"{currentLevel}";
    }

    /*
    private void OnEnable()
    {
        //스테이지매니저에서 클리언 내용 구독 연결
    }
    private void OnDisable()
    {
        //스테이지매니저에서 클리언 내용 구독 연결
    }
    private void OnstageCler(bool Cler)
    {
        AddEXP(stageClearExp);
    }
    */

    // 테스트용
    [ContextMenu("Test Stage Clear")]
    void TestStageClear()
    {
        AddEXP(stageClearExp);
    }
}
