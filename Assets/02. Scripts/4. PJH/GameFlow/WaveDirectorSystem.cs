using UnityEngine;

public class WaveDirectorSystem : MonoBehaviour
{
    /*
        !전략패턴 - 인터페이스

        웨이브 룰 : 라운드 시간을 공유 / 
        스폰 트리거 실행 :
    */

    #region field
    [Header("스테이지 데이터")]
    [SerializeField] private StageConfigSO stageData; //해당 데이터 값 받아오기, 인스펙터 수동 할당 x

    [Header("활성화 된 스테이지 룰")]
    [SerializeField] private WaveRule ruleType;
    #endregion

    void Awake()
    {

    }

    void Update()
    {
        ruleType.TimeOut();
    }

    #region method

    #endregion
}