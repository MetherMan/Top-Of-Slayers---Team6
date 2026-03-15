using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int size;
        public int maxSize;
    }

    [SerializeField] private List<Pool> pools;

    //프리팹과 오브젝트 큐를 매핑하는 딕셔너리
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary
        = new Dictionary<GameObject, Queue<GameObject>>();

    //풀에서 현재 개수 추적하는 딕셔너리
    private Dictionary<GameObject, int> poolCurrent = new Dictionary<GameObject, int>();
    private readonly Dictionary<GameObject, GameObject> pooledObjectSources = new Dictionary<GameObject, GameObject>();

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        if (poolDictionary == null) poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        if (poolCurrent == null) poolCurrent = new Dictionary<GameObject, int>();
        if (pools == null || pools.Count == 0)
        {
            return;
        }

        foreach (var pool in pools)
        {
            if (pool == null || pool.prefab == null)
            {
                continue;
            }

            if (poolDictionary.ContainsKey(pool.prefab))
            {
                continue;
            }

            //오브젝트 큐 생성
            Queue<GameObject> objectQ = new Queue<GameObject>();

            //풀 사이즈만큼 오브젝트 생성
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                RegisterPooledObject(pool.prefab, obj);
                obj.SetActive(false);
                objectQ.Enqueue(obj);
            }
            //딕셔너리에 큐 추가
            poolDictionary.Add(pool.prefab, objectQ);
            poolCurrent.Add(pool.prefab, Mathf.Max(0, pool.size));
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        base.OnDestroy();
    }

    public GameObject SpawnPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        if (poolDictionary == null) poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        if (poolCurrent == null) poolCurrent = new Dictionary<GameObject, int>();

        if (!poolDictionary.TryGetValue(prefab, out Queue<GameObject> objectQueue))
        {
            objectQueue = new Queue<GameObject>();
            poolDictionary[prefab] = objectQueue;

            if (!poolCurrent.ContainsKey(prefab))
            {
                poolCurrent[prefab] = 0;
            }
        }

        GameObject obj = null;

        if (objectQueue.Count > 0)
        {
            //큐에서 오브젝트 꺼내기
            obj = objectQueue.Dequeue();
        }
        else
        {
            Pool poolSetting = pools?.Find(p => p != null && p.prefab == prefab);
            bool hasMaxLimit = poolSetting != null && poolSetting.maxSize > 0;
            int currentCount = poolCurrent.TryGetValue(prefab, out int count) ? count : 0;

            //맥스사이즈 이상이면 널
            if (hasMaxLimit && currentCount >= poolSetting.maxSize)
            {
                return null;
            }

            //큐가 비어있으면 새로 생성
            obj = Instantiate(prefab, transform);
            RegisterPooledObject(prefab, obj);
            poolCurrent[prefab] = currentCount + 1;
        }

        if (obj.transform.parent != transform)
        {
            obj.transform.SetParent(transform);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    public void ReturnPool(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null) return;

        if (!poolDictionary.TryGetValue(prefab, out Queue<GameObject> objectQueue))
        {
            objectQueue = new Queue<GameObject>();
            poolDictionary[prefab] = objectQueue;
        }

        if (obj.transform.parent != transform)
        {
            obj.transform.SetParent(transform);
        }

        obj.SetActive(false);
        objectQueue.Enqueue(obj);
    }

    private void RegisterPooledObject(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null)
        {
            return;
        }

        pooledObjectSources[obj] = prefab;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupActivePooledObjects();
    }

    private void CleanupActivePooledObjects()
    {
        if (pooledObjectSources.Count == 0)
        {
            return;
        }

        List<GameObject> destroyedObjects = null;

        foreach (var pair in pooledObjectSources)
        {
            GameObject pooledObject = pair.Key;
            GameObject prefab = pair.Value;

            if (pooledObject == null)
            {
                destroyedObjects ??= new List<GameObject>();
                destroyedObjects.Add(pooledObject);
                continue;
            }

            if (!pooledObject.activeSelf)
            {
                continue;
            }

            if (!poolDictionary.TryGetValue(prefab, out Queue<GameObject> objectQueue))
            {
                objectQueue = new Queue<GameObject>();
                poolDictionary[prefab] = objectQueue;
            }

            if (pooledObject.transform.parent != transform)
            {
                pooledObject.transform.SetParent(transform);
            }

            pooledObject.SetActive(false);
            objectQueue.Enqueue(pooledObject);
        }

        if (destroyedObjects == null)
        {
            return;
        }

        for (int i = 0; i < destroyedObjects.Count; i++)
        {
            pooledObjectSources.Remove(destroyedObjects[i]);
        }
    }
}
