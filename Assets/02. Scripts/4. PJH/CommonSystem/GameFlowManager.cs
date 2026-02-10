using UnityEngine;

public class GameFlowManager : Singleton<GameFlowManager>
{
    /*
        게임 총 관리
    */

    #region field
    public int waveIndex;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        
    }

    #region method
    public void RoundClear()
    {
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null)
        {
            return;
        }

        if (stageManager.selectDB.clearResult == (ClearResult)1)
        {
            Debug.Log("스테이지 클리어.");
        }
    }
    #endregion
}
