using UnityEngine;

public class QuitApplicationUtility
{
    public static void QuitApp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject activity =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity");

        activity.Call<bool>("moveTaskToBack", true);

#else
        Application.Quit();
#endif
    }
}