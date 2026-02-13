using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

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

        foreach (var data in stageData)
        {
            if (data == null) continue;
            if (!stageDic.ContainsKey(data.stageKey))
            {
                stageDic.Add(data.stageKey, data);
            }
        }
    }

    public StageConfigSO GetStageData(string num)
    {
        if (stageDic == null) Initialization();
        if (stageDic.TryGetValue(num, out StageConfigSO data))
        {
            roundDatas = data.roundDatas;
            return data;
        }

        Debug.LogWarning($"StageNum {num}에 해당하는 데이터를 찾을 수 없습니다.");
        return null;
    }
    #endregion
}