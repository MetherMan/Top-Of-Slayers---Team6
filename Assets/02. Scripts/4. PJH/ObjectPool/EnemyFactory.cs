using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager objectPoolManager;

    public GameObject Create(EnemyConfigSO enemySO, Vector3 position, Quaternion rotation)
    {
        GameObject enemy = objectPoolManager.SpawnPool(enemySO.monsterPrefab, position, rotation);

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if(enemyBase != null)
        {
            enemyBase.Init(enemySO.monsterPrefab);
        }
        return enemy;
    }
}
