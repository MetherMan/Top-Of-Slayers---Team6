using UnityEngine;

public abstract class WaveRule : MonoBehaviour
{
    /*
        공통 기능 : 클리어 타임내에 라운드를 마무리 할 것 등 
        세부적인 웨이브 조건은 다루지 않는다.
    */
    #region field
    //조건 체크 변수
    protected int cleardWaveNum;

    #endregion

    #region method
    //실패 조건
    public void TimeOut()
    {
        if (StageFlowManager.Instance.remainingTime <= StageFlowManager.Instance.playTime)
        {
            //게임오버

            //
            StageManager.Instance.selectDB.clearResult = (ClearResult)4;
        }
    }

    public void HpZero(int hp)
    {
        //플레이어 체력 '0' 이하 일 경우 게임 오버
        if (hp <= 0)
        {
            StageManager.Instance.selectDB.clearResult = (ClearResult)4;
        }
    }

    //라운드 클리어 조건
    public void WaveAllCleared()
    {
        //웨이브 진행상황을 어떻게 확인할 것인가?
        //StageConfigSO -> StageDatabase -> StageManager -> ObjectPool -> EnemySpawner
        //EnemyDown() x++ 변수 증감 -> StageDatabase.Instance.stageLound 변수와 if문 비교
        if (StageDatabase.Instance.stageLound == cleardWaveNum)
        {
            //스테이지 클리어
        }
    }
    #endregion
}