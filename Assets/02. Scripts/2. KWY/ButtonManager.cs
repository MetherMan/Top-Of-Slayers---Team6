using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private ButtonOn[] tabButtons;

    private int currentIndex = -1;

    private void Start()
    {
        SelectTab(0);
    }
    public void SelectTab(int index)
    {
        if (currentIndex == index) return;

        currentIndex = index;

        for (int i = 0; i < tabButtons.Length; i++) 
        {
            tabButtons[i].SetSelected(i == index);
        }
    }
}
