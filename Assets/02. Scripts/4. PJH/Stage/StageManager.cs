using UnityEngine;

/*
    !!! 0306 : 코드 엎음
  
    StageDatabase에서 StageSO를 가져온다
*/
public class StageManager : Singleton<StageManager>
{
    #region field
    public StageDatabase stageDB;
    public StageConfigSO selectDB;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        stageDB = StageDatabase.Instance;
        StageData("21");
        if (selectDB != null) return;

        /*
        if (stageDB == null || stageDB.stageData == null) return;
        for (int i = 0; i < stageDB.stageData.Count; i++)
        {
            var fallback = stageDB.stageData[i];
            if (fallback == null) continue;
            selectDB = fallback;
            Debug.LogWarning($"StageData(21) 로드 실패. 대체 스테이지({fallback.stageKey})를 사용합니다.");
            return;
        }
        */ //기본 데이터가 없을 때는 첫 유효 스테이지를 대체 로드한다.
    }

    #region method
    //스테이지 UI 클릭 시 실행될 매서드
    public void StageData(string key)
    {
        AddressableManager addressableManager = FindFirstObjectByType<AddressableManager>();
        if (addressableManager != null)
        {
            StageConfigSO addressableData = addressableManager.GetStageData(key);
            if (addressableData != null)
            {
                //해당 스테이지 데이터를 불러오기
                selectDB = addressableData;
                return;
            }
        }

        //if (stageDB == null)
        //{
        //    stageDB = StageDatabase.Instance;
        //}

        //if (stageDB != null)
        //{
        //    StageConfigSO localData = stageDB.GetStageData(key);
        //    if (localData != null)
        //    {
        //        //Addressable 데이터가 없을 경우 로컬 DB로 대체
        //        selectDB = localData;
        //        return;
        //    }
        //}

        Debug.LogWarning($"StageData({key}) 로드에 실패했습니다.");
    }
    #endregion
}
