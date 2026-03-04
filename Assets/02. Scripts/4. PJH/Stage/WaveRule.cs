using UnityEngine;

/*
    공통적으로 적용되는 룰
    stage, boss, chellenge룰의 경우 해당 룰에만 적용되는 구문 작성

    룰.cs는 변수의 데이터 값이 무엇인지 몰라도 구현되도록 작성

    ^ data 매개변수를 사용한 if문만 작성할 것, 직접 실행되는 상세구문은 WaveDirectorSystem에서 작성
    
    data : if문에 사용될 변수를 담아두는 매개변수
    context : 조건이 충족될 경우 실행될 메서드

    ^ if문 작성에 필요한 data 변수를 추가해야 할 경우,
        1. WaveDirectorSystem.cs에 RuleDataContainer에 변수를 추가
        2. ConnectData() 메서드에 var stageFlow 변수를 사용해 할당

    ^ context.RoundClear()는 클리어가 되었다는 전제하에 작성되어있으므로, if문 작성 필요

    ^ 새로운 룰을 추가해야할 경우
        1. WaveDirectorSystem.cs에서 [ #region 룰 ]에 해당 룰을 작성 후
        2. 공통적으로 적용되는 룰일 경우 WaveRule.cs에 작성
            2-1.개별 룰일 경우 해당 룰.cs에 작성
        3. OnUpdate 메서드에 추가
*/

public abstract class WaveRule : ScriptableObject
{
    #region 룰 실행 사이클
    //룰을 적용시키기 위해 필요한 것이 있을 경우 구문작성
    public abstract void OnStart(RuleDataContainer data, WaveDirectorSystem context);

    //Update단에 배치해 룰을 적용하는 메서드
    public abstract void OnUpdate(RuleDataContainer data, WaveDirectorSystem context);

    //클리어 시 수집해야할 데이터 전송
    public abstract void OnExit(RuleDataContainer data, WaveDirectorSystem context);
    #endregion

    #region 게임오버 룰
    //타임오버 : if문 수정필요
    public void TimeOver(RuleDataContainer data, WaveDirectorSystem context)
    {
        if (data.playTime >= data.stageData.stageTime)
        {
            context.TimeOver();
        }
    }

    //체력 0 : if문 작성필요
    public void HpZero(RuleDataContainer data, WaveDirectorSystem context)
    {
        //플레이어 체력 '0' 이하 일 경우 게임 오버
        //context.HpZero();
    }
    #endregion

    #region 게임 진행에 필요한 룰
    public void WaveClear(RuleDataContainer data, WaveDirectorSystem context)
    {
        context.WaveClear();
    }
    #endregion

    #region 게임 클리어
    public abstract void RoundClear(RuleDataContainer data, WaveDirectorSystem context);
    #endregion
}