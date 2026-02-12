using UnityEngine;

public class GameFlowManager : Singleton<GameFlowManager>
{
    /*
        게임 총 관리
        GameStageMachine에서 상태변화가 있을 경우 변화에 맞춰 해야할 일을 한다.
        일이 일단락 되면 GameStageMachine에 해야할 일을 끝냈음을 알린다.
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
