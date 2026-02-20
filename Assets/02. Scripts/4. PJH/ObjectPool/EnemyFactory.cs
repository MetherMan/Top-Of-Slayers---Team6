using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager objectPoolManager;

    public GameObject Create(EnemyConfigSO enemySO, Vector3 position, Quaternion rotation)
    {
        GameObject enemy = objectPoolManager.SpawnPool(enemySO.monsterPrefab, position, rotation);
        return enemy;
    }
}
