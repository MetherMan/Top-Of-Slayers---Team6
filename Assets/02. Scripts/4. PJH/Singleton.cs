using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
    static bool isQuitting;

    [SerializeField] bool DontDestroy;

    public static bool HasInstance => instance != null;

    public static T Instance
    {
        get
        {
            if (isQuitting)
            {
                return null;
            }

            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if (instance == null && Application.isPlaying)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;

            if (DontDestroy)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        //else Destroy(gameObject);
    }

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this as T)
        {
            instance = null;
        }
    }
}
