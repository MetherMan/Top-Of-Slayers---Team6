using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager objectPoolManager;

    public GameObject Create(EnemyConfigSO enemySO, Vector3 position, Quaternion rotation)
    {
        if (objectPoolManager == null)
        {
            Debug.LogError("ObjectPoolManager가 EnemyFactory에 연결 안 됨");
        }

        GameObject enemy = objectPoolManager.SpawnPool(enemySO.monsterPrefab, position, rotation);
        return enemy;
    }
}
