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
                }
            }
            return instance;
        }
    }

    private StageConfigSO selectStage;

    //스테이지 라운드(웨이브) 데이터
    public List<StageConfigSO.RoundData> roundDatas;
    public StageConfigSO.RoundData roundData; //어딘가 사용되고 있는 코드?

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
    public StageConfigSO GetStageData(string addressableName)
    {
        StageConfigSO data = AddressableManager.Instance.GetStageData(addressableName);

        roundDatas = data.roundDatas;
        selectStage = data;

        StageManager.Instance.GetData(selectStage, this);

        return selectStage;
    }
    #endregion
}