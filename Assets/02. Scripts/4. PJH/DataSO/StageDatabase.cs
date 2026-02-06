using System.Collections.Generic;
using System.Linq;
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
                //현재 사용
                instance = Resources.Load<StageDatabase>("StageInfo/Database_Main");

                /*
                    StageDatabase handle = Addressables.LoadAssetAsyne<StageDatabase>("");
                    instance = handle.WaitForCompletion();
                */ //Addressables 용

                if (instance == null)
                {
                    Debug.LogError("StageDatabase 에셋을 찾을 수 없습니다." +
                        "99. Resources 폴더를 확인하세요");
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

    private Dictionary<int, StageConfigSO> stageDic;
    #endregion

    private void Awake()
    {
        
    }

    void start()
    {

    }

    #region method
    public void Initialization()
    {
        if (stageDic != null) return; //중복실행 방지

        stageDic = new Dictionary<int, StageConfigSO>();

        foreach (var data in stageData)
        {
            if (data == null) continue;
            if (!stageDic.ContainsKey(data.stageNum))
            {
                stageDic.Add(data.stageNum, data);
            }
        }
    }

    public StageConfigSO GetStageData(int num)
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