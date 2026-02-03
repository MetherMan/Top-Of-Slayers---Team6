using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using DG.Tweening;

public class UIStatePresenter : MonoBehaviour
{
    public void OpenPanel(GameObject target)
    {
        target.SetActive(true);
    }

    public void CloseCurrent(GameObject target)
    {
        target.SetActive(false);
    }


    /*
     

    1. 제작소
    제작 분해


    2. 인벤토리 - 내캐릭 | 인벤 제작 분해 
    클릭 시 아래 바뀜

    제작 - 
    가운대 플러스 누르면 아이템

    분해 - 
    분해는 반 나눠서
    아이템 분해는 2개정도만

    제작은 따로
    무기 헬멧 갑옵 바지 신발

    무기 누르면 무기제작 리스트 쭉 무기 누르면 해당 무기 판넬뜨면서 해당 무기 아이콘 옆에 뜨고 
    필요한 재료뜨기 밑에 제작 버튼
    무기 아이콘 클릭하면 무기 이미지랑 스텟까지만 간단하게 보여주기

    분해는 인벤토르에서 직접
    인벤토리에서 아이템 클릭하면 무기 이미지랑 스텟까지만 간단하게 보여주기
    밑에 분해 버튼 나옴 클릭하면
    아이템 분해 판넬뜨고 분해재료 뜨고 아래에 주의사항(분해한 아이템은 사라지며 복구할 수 없다)
    아래에 분해하기 버튼



    상점은 아에 분리 - 상점을 열면  왼쪽은 상점 npc 모습 | 아이템 나열

  
    가차때
    npc믹사모 넣어서 연출
     
    플레이 위에 어느 스테이지까지 했는지



    캐릭터 스텟

    특성  	캐릭터       공격력
        	             방어력
	            		이동속도
    버프			        에너지


     */
}
