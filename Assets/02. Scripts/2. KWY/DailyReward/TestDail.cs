using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class TestDail : MonoBehaviour
{
    string url = "https://www.google.com";

    private void Start()
    {
        StartCoroutine(WebTime());
    }

    IEnumerator WebTime()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                string date = request.GetResponseHeader("date");

                DateTime kst = DateTime.Parse(date);    


                Debug.Log(kst.ToString("yyyy-MM-dd HH:mm:ss" ));
            }
        }
    }
}
