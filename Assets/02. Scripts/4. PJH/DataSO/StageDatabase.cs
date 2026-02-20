using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Database_", menuName = "Config/StageDatabase")]
public class StageDatabase : ScriptableObject
{
    #region field
    static StageDatabase instance;
    public static StageDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<StageDatabase>("StageInfo/Database_Main");

                //StageDatabase handle = Addressables.LoadAssetAsync<StageDatabase>("");
                //instance = handle.WaitForCompletion();

                if (instance == null)
                {

                }
            }
            return instance;
        }
    }
    //스테이지 라운드(웨이브) 데이터
    public List<StageConfigSO.RoundData> roundDatas;
    public StageConfigSO.RoundData roundData;

    //stageConfigSO의 창고로 사용할 예정
    public List<StageConfigSO> stageData = new List<StageConfigSO>();

    private Dictionary<string, StageConfigSO> stageDic;
    #endregion

    #region method
    public void Initialization()
    {
        if (stageDic != null) return; //중복실행 방지

        stageDic = new Dictionary<string, StageConfigSO>();
        if (stageData == null) return;

        foreach (var data in stageData)
        {
            if (data == null) continue;

            if (!TryResolveStageKey(data, out string stageKey))
            {
                Debug.LogWarning($"StageDatabase 초기화 중 stageKey를 확인할 수 없습니다: {data.name}");
                continue;
            }

            if (!stageDic.ContainsKey(stageKey))
            {
                stageDic.Add(stageKey, data);
            }
        }
    }

    public StageConfigSO GetStageData(string num)
    {
        if (string.IsNullOrWhiteSpace(num))
        {
            Debug.LogWarning("StageNum이 비어 있어 데이터를 조회할 수 없습니다.");
            return null;
        }

        if (stageDic == null) Initialization();
        string stageKey = num.Trim();
        if (stageDic.TryGetValue(stageKey, out StageConfigSO data))
        {
            roundDatas = data.roundDatas;
            return data;
        }

        Debug.LogWarning($"StageNum {num}에 해당하는 데이터를 찾을 수 없습니다.");
        return null;
    }

    public StageConfigSO GetStageData(int num)
    {
        return GetStageData(num.ToString());
    }

    private bool TryResolveStageKey(StageConfigSO data, out string stageKey)
    {
        stageKey = null;
        if (!string.IsNullOrWhiteSpace(data.stageKey))
        {
            stageKey = data.stageKey.Trim();
            return true;
        }

        string name = data.name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        int start = -1;
        int end = -1;
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsDigit(name[i]))
            {
                if (start < 0) start = i;
                end = i;
                continue;
            }

            if (start >= 0) break;
        }

        if (start < 0 || end < start)
        {
            return false;
        }

        stageKey = name.Substring(start, end - start + 1);
        return !string.IsNullOrWhiteSpace(stageKey);
    }
    #endregion
}
