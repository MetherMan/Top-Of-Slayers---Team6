using UnityEngine;

/*
    !해당 스테이지에 해당하는 데이터 연결

    *싱글톤
    StageFlowManager 연동
    StageDatabase 메서드 실행
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
        StageData(21);
        if (selectDB != null) return;

        // 기본 데이터가 없을 때는 첫 유효 스테이지를 대체 로드한다.
        if (stageDB == null || stageDB.stageData == null) return;
        for (int i = 0; i < stageDB.stageData.Count; i++)
        {
            var fallback = stageDB.stageData[i];
            if (fallback == null) continue;
            selectDB = fallback;
            Debug.LogWarning($"StageData(21) 로드 실패. 대체 스테이지({fallback.stageKey})를 사용합니다.");
            return;
        }
    }

    void Update()
    {

    }

    #region method
    //스테이지 오브젝트 클릭 시 실행될 매서드
    //public void StageData(int id)
    //{
    //    StageConfigSO data = stageDB.GetStageData(id);
    //    if (data != null)
    //    {
    //        //해당 스테이지 데이터를 불러오기
    //        selectDB = data;
    //    }
    //}

    public void StageData(int id)
    {
        if (stageDB == null)
        {
            stageDB = StageDatabase.Instance;
        }
        if (stageDB == null)
        {
            Debug.LogWarning("StageDatabase를 찾을 수 없습니다.");
            return;
        }

        StageConfigSO data = stageDB.GetStageData(id);
        if (data != null)
        {
            //해당 스테이지 데이터를 불러오기
            selectDB = data;
            return;
        }

        Debug.LogWarning($"StageData({id}) 로드에 실패했습니다.");
    }
    #endregion
}
