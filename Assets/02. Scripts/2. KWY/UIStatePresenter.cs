using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIStatePresenter : MonoBehaviour
{
    [SerializeField] private GameObject[] allPanls;


    public void OpenPanel(GameObject target)
    {
        target.SetActive(true);
    }

    public void CloseCurrent(GameObject target)
    {
        target.SetActive(false);
    }

}
