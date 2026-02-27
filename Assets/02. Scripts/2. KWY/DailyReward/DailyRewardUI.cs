using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
    [SerializeField] DailyRewardSystem dailySystem;
    [SerializeField] Image[] daySlot;
    [SerializeField] ParticleSystem[] rewardParticle;

    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite changeSprite;


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

            daySlot[i].sprite = (i<day) ?changeSprite : defaultSprite;

            if(i == day && canReward)
            {
                if (!rewardParticle[i].isPlaying)
                {
                    rewardParticle[i].Play();
                }
            }
            else
            {
                rewardParticle[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
