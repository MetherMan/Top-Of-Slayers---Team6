using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
    [SerializeField] DailyRewardSystem dailySystem;
    [SerializeField] Image[] daySlot;

    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite changeSprite;


    private void OnEnable()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        int day = dailySystem.currentDay;

        for(int i =0; i < daySlot.Length; i++)
        {
            if(i < day)
            {
                daySlot[i].sprite = changeSprite;
            }
            else
            {
                daySlot[i].sprite = defaultSprite;
            }
        }
    }
}
