using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
    [SerializeField] DailyRewardSystem dailySystem;
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
            dailySystem.GetReward();
        }
        RefreshUI();
    }

    private void RefreshUI()
    {
        int day = dailySystem.currentDay;
        bool canReward = dailySystem.CanReward();

        for(int i =0; i < daySlot.Length; i++)
        {

            //daySlot[i].sprite = defaultSprite;
            checkImage[i].SetActive(i < day);

            if(i == day && canReward)
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
