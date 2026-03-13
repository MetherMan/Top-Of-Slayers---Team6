using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
    [SerializeField] DailyRewardSystem dailySystem;
    [SerializeField] CoinDropEffect coinDropEffect;
    [SerializeField] Image[] daySlot;
    [SerializeField] GameObject[] checkImage;
    [SerializeField] GameObject[] partickles;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void OnClickReward()
    {
        if (dailySystem.CanReward())
        {
            //현재 보상 받을 슬롯 위치
            Vector3 coinStartPos = daySlot[dailySystem.currentDay].transform.position;

            dailySystem.GetReward();

            //코인 이펙트
            coinDropEffect.CoinParty(coinStartPos);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        int day = dailySystem.currentDay;
        bool canReward = dailySystem.CanReward();

        for (int i = 0; i < daySlot.Length; i++)
        {
            checkImage[i].SetActive(i < day);

            if (i == day && canReward)
            {
                partickles[i].SetActive(true);
            }
            else
            {
                partickles[i].SetActive(false);
            }
        }
    }
}