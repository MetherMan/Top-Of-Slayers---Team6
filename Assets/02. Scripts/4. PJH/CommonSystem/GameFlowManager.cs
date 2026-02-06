using UnityEngine;

public class GameFlowManager : Singleton<GameFlowManager>
{
    /*
        상태전환
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
        if (StageManager.Instance.selectDB.clearResult == (ClearResult)1)
        {
            Debug.Log("GameFlowManager RoundClear()");
        }
    }
    #endregion
}