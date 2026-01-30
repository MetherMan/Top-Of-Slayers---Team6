using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Database_", menuName = "Config/StageDatabase")]
public class StageDatabase : ScriptableObject
{
    static StageDatabase instance;
    public static StageDatabase Instance
    {
        get
        {
            if (instance == null)
            {

            }
        }
    }

    private Dictionary<float, StageConfigSO> stageDic;

    public void Initialization()
    {
        stageDic = new Dictionary<float, StageConfigSO>();

        foreach (var data in stageData)
        {
            if (!stageDic.ContainsKey(data.stageNum))
            {
                stageDic.Add(data.stageNum, data);
            }
        }
    }

    public StageConfigSO GetStageData(float num)
    {
        if (stageDic == null) Initialization();

        stageDic.TryGetValue(num, out StageConfigSO data);
        return data;
    }
}