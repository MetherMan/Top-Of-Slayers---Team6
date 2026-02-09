using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTest : MonoBehaviour
{
    public RectTransform iconAnchor; // 위에서 만든 Anchor
    public GameObject yellowBG;     // 노란 배경

    // 이 버튼이 선택되었을 때 호출
    public void SelectMenu()
    {
        // 1. 배경 켜기
        yellowBG.SetActive(true);

        // 2. 아이콘 연출: 위로 이동하면서 커짐
        // Pivot이 하단(0)이므로 Scale만 키워도 위로 커지는 효과가 납니다.
        iconAnchor.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack); // 살짝 튕기는 느낌
        iconAnchor.DOAnchorPosY(20f, 0.2f); // 약간 위로 띄워줌

        // 3. 레이어 우선순위: 옆 아이콘보다 앞으로 나오게 함
        transform.SetAsLastSibling();
    }

    // 다른 메뉴가 선택되어 해제될 때 호출
    public void DeselectMenu()
    {
        yellowBG.SetActive(false);
        iconAnchor.DOScale(1.0f, 0.2f);
        iconAnchor.DOAnchorPosY(0f, 0.2f);
    }
}
