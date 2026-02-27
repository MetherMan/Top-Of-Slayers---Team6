using System;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardSystem : MonoBehaviour
{
    [field: SerializeField] private string today;
    [field: SerializeField] public int currentDay { get; private set; }
    [field: SerializeField] public string lastDate { get; private set; }


    private void Start()
    {
        today = DateTime.Now.ToString("yyyyMMdd"); // 오늘 날짜
        currentDay = PlayerPrefs.GetInt("CurrentDay", 0);
        lastDate = PlayerPrefs.GetString("LastDate", "NONE");

        if (currentDay >= 7 && lastDate != today)
        {
            RestReward();
        }
    }

    public bool CanReward()
    {
        today = DateTime.Now.ToString("yyyyMMdd");
        return lastDate != today;
    }

    public void GetReward()
    {
        if (!CanReward()) return;

        currentDay++;

        if (currentDay == 7)
        {
            CurrencyManager.Instance.Add(10000);

        }
        else
        {
            CurrencyManager.Instance.Add(1000);

        }

        lastDate = today;
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.SetString("LastDate", lastDate);


        PlayerPrefs.Save();
    }

    private void RestReward()
    {
        currentDay = 0;
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.Save();
    }

    public void ResetButton()
    {
        PlayerPrefs.DeleteAll();

    }
}