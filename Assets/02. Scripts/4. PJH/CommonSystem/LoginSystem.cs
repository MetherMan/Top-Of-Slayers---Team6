using TMPro;
using UnityEngine;

public class LoginSystem : MonoBehaviour
{
    #region field
    [Header("Login")]
    [SerializeField] GameObject loginUI;
    [SerializeField] TextMeshProUGUI inputID;
    [SerializeField] TextMeshProUGUI inputPW;

    [Header("Sign Up")]
    [SerializeField] GameObject signUpUI;
    #endregion

    void Awake()
    {
        TakeObject();
    }

    void Update()
    {
        
    }

    #region method
    void TakeObject()
    {

    }
    #endregion
}