using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class POPUP : MonoBehaviour
{
    [SerializeField] GameObject popip;

    private void Start()
    {
        popip.SetActive(false);
        StartCoroutine(showPanel());
    }

    IEnumerator showPanel()
    {
        yield return new WaitForSeconds(3f);
        popip.SetActive(true);
    }
}
