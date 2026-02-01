using UnityEngine;

/*
    !해당 스테이지에 해당하는 데이터 연결

    *싱글톤
    StageFlowManager 연동
    StageDatabase 메서드 실행
*/
public class StageManager : MonoBehaviour
{
    #region field
    public static StageManager Instance { get; private set; }
    public StageDatabase stageDB;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        
    }

    #region method
    public void StageData(int id)
    {
        StageConfigSO data = stageDB.GetStageData(id);
        if (data != null)
        {
            //해당 스테이지 데이터를 불러오기
        }
    }
    #endregion
}