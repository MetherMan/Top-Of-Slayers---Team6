using System.Collections.Generic;
using UnityEngine;
/*
    0306 AddressableManager 기반으로 코드 변경

    해당 스테이지 StageSO만 가지고 있는다 나머지 다 필요 없음
*/
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
                instance = AddressableManager.Instance.GetDatabase("Database_Main");

                if (instance == null)
                {
                    Debug.LogWarning("데이터베이스 파일을 찾을 수 없음");
                    Debug.LogWarning("데이터베이스 파일을 찾을 수 없음");
                }
            }
            return instance;
        }
    }

    public StageConfigSO selectStage;

    //스테이지 라운드(웨이브) 데이터
    public List<StageConfigSO.RoundData> roundDatas;
    public StageConfigSO.RoundData roundData;

    #endregion

    #region method
    //public void Initialization()
    //{
    //    if (stageDic != null) return; //중복실행 방지

    //    stageDic = new Dictionary<string, StageConfigSO>();
    //    if (stageData == null) return;

    //    foreach (var data in stageData)
    //    {
    //        if (data == null) continue;

    //        if (!TryResolveStageKey(data, out string stageKey))
    //        {
    //            Debug.LogWarning($"StageDatabase 초기화 중 stageKey를 확인할 수 없습니다: {data.name}");
    //            continue;
    //        }

    //        if (!stageDic.ContainsKey(stageKey))
    //        {
    //            stageDic.Add(stageKey, data);
    //        }
    //    }
    //}
    private void Init()
    {

    }


    public StageConfigSO GetStageData(string addressableName)
    {
        if (string.IsNullOrWhiteSpace(addressableName))
        {
            Debug.LogWarning("StageNum이 비어 있어 데이터를 조회할 수 없습니다.");
            return null;
        }

        //if (stageDic == null) Init();
        string stageKey = addressableName.Trim();
        /*if (stageDic.TryGetValue(stageKey, out StageConfigSO data))
        {
            roundDatas = data.roundDatas;
            return data;
        }*/

        Debug.LogWarning($"StageNum {addressableName}에 해당하는 데이터를 찾을 수 없습니다.");
        return null;
    }

    public StageConfigSO GetStageData(int num)
    {
        return GetStageData(num.ToString());
    }

    //private bool TryResolveStageKey(StageConfigSO data, out string stageKey)
    //{
    //    stageKey = null;
    //    if (!string.IsNullOrWhiteSpace(data.stageKey))
    //    {
    //        stageKey = data.stageKey.Trim();
    //        return true;
    //    }

    //    string name = data.name;
    //    if (string.IsNullOrWhiteSpace(name))
    //    {
    //        return false;
    //    }

    //    int start = -1;
    //    int end = -1;
    //    for (int i = 0; i < name.Length; i++)
    //    {
    //        if (char.IsDigit(name[i]))
    //        {
    //            if (start < 0) start = i;
    //            end = i;
    //            continue;
    //        }

    //        if (start >= 0) break;
    //    }

    //    if (start < 0 || end < start)
    //    {
    //        return false;
    //    }

    //    stageKey = name.Substring(start, end - start + 1);
    //    return !string.IsNullOrWhiteSpace(stageKey);
    //}
    #endregion
}
