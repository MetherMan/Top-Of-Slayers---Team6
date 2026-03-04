using UnityEngine;
using UnityEngine.UI;

/*
    Button으로 중복터치 방지
*/
[RequireComponent(typeof(Button))]
public class SceneSlot : MonoBehaviour
{
    #region field
    Button button;

    [Header("스테이지 정보")]
    [SerializeField] public string stageKey;

    #endregion

    void Awake()
    {
        //인스펙터 할당 없이 코드로 연동
        //https://youtu.be/l0QwB7xafl4?si=dTBTe71V7pU-arU1
        button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);

        //익명함수로 구현
        //this.button.onClick.AddListener(() =>
        //{
        //    OnClick();
        //});
    }

    #region method
    public void OnClick()
    {
        button.interactable = false;

        Debug.Log("Slot -> SceneLoadManager 데이터 전달 중");
        //데이터 전달
        SceneLoadManager.Instance.ActiveScene(this);

        Invoke("ActiveButton", 1f);
    }

    void ActiveButton()
    {
        button.interactable = true;
    }
    #endregion
}