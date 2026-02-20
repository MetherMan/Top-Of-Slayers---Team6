using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager objectPoolManager;

    public GameObject Create(EnemyConfigSO enemySO, Vector3 position, Quaternion rotation)
    {
        if (enemySO == null || enemySO.monsterPrefab == null)
        {
            return null;
        }

        if (objectPoolManager == null)
        {
            objectPoolManager = ObjectPoolManager.Instance;
        }

        GameObject enemy = null;
        if (objectPoolManager != null)
        {
            enemy = objectPoolManager.SpawnPool(enemySO.monsterPrefab, position, rotation);
        }

        if (enemy == null)
        {
            enemy = Instantiate(enemySO.monsterPrefab, position, rotation);
        }

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if(enemyBase != null)
        {
            enemyBase.Init(enemySO.monsterPrefab);
        }
        return enemy;
    }
}
