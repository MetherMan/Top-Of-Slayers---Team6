using System.Collections.Generic;
using UnityEngine;

public class RewardUI : MonoBehaviour
{
    [SerializeField] GameObject rewardPanel;

    [SerializeField] Transform iconParent;
    [SerializeField] GameObject iconPrefab;

    public void ShowReward(List<RewardData> rewards)
    {
        ResetPresentation();

        if (rewardPanel == null || iconParent == null || iconPrefab == null)
        {
            return;
        }

        rewardPanel.SetActive(true);

        foreach(var reward in rewards)
        {
            var icon = Instantiate(iconPrefab, iconParent).GetComponent<RewardIcon>();

            icon.Set(reward);
        }
    }

    public void ResetPresentation()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        if (iconParent == null)
        {
            return;
        }

        foreach(Transform child in iconParent)
        {
            Destroy(child.gameObject);
        }
    }

}

