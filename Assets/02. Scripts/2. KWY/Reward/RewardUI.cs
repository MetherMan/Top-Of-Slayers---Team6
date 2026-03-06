using System.Collections.Generic;
using UnityEngine;

public class RewardUI : MonoBehaviour
{
    [SerializeField] GameObject rewardPanel;

    [SerializeField] Transform iconParent;
    [SerializeField] GameObject iconPrefab;

    public void ShowReward(List<RewardData> rewards)
    {
        ClearResult();

        rewardPanel.SetActive(true);

        foreach(var reward in rewards)
        {
            var icon = Instantiate(iconPrefab, iconParent).GetComponent<RewardIcon>();

            icon.Set(reward);
        }
    }

    private void ClearResult()
    {
        rewardPanel.SetActive(false);

        foreach(Transform child in iconParent)
        {
            Destroy(child.gameObject);
        }
    }

}

