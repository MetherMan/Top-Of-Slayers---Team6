using UnityEngine;

public abstract class WaveRule : MonoBehaviour
{
    /*
        공통 기능 : 클리어 타임내에 웨이브를 마무리 할 것 등 
    */
    #region field
    

    #endregion

    protected virtual void Awake()
    {
        
    }

    #region method
    //실패 조건
    public void TimeOut()
    {
        if (StageFlowManager.Instance.remainingTime <= StageFlowManager.Instance.playTime)
        {

        }
    }

    public void HpZero()
    {

    }

    //웨이브 진행 조건
    public void EnemyDown()
    {

    }

    //라운드 클리어 조건
    public void WaveAllCleared()
    {
        //웨이브 진행상황을 어떻게 확인할 것인가?
        //StageConfigSO -> StageDatabase -> StageManager -> ObjectPool -> EnemySpawner
        //EnemyDown() x++ 변수 증감 -> StageDatabase.Instance.stageLound 변수와 if문 비교
        StageDatabase.Instance.stageLound
    }
    #endregion
}