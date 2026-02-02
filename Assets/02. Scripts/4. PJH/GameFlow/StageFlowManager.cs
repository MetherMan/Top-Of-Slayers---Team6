using UnityEngine;

/*
    웨이브 시작, 종료
    클리어 판정
*/

public class StageFlowManager : Singleton<StageFlowManager>
{
    #region field
    public int remainingTime;
    public int playTime;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        //선택된 스테이지의 StageConfigSO에서 stageTime 변수 값을 가져와서 할당
        remainingTime = StageManager.Instance.selectDB.stageTime;
    }

    void Update()
    {
        playTime += (int)Time.deltaTime;
    }

    #region method
    #endregion
}