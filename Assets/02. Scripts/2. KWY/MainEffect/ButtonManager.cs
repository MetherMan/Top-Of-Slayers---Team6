using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] ButtonOn[] buttons;

    int currentIndex = -1;

    void Start()
    {
        SelectTab(2); // 중앙 기본 선택
    }

    public void SelectTab(int index)
    {
        if (currentIndex == index) return;

        currentIndex = index;

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetSelected(i == index);
        }
    }
}
