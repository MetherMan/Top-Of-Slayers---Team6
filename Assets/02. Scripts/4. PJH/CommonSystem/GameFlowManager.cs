using UnityEngine;

public class GameFlowManager : Singleton<GameFlowManager>
{
    /*
        !전략패턴 - 컨텍스트 
    */

    #region field
    //스테이지 클리어 결과
    private int resultStarPoint; //클리어 조건 룰 충족 시 ++
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        
    }

    #region method

    #endregion
}