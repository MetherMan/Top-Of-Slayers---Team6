using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager objectPoolManager;
    [SerializeField] private StageConfigSO stageSO;

    public GameObject Create(Vector3 position, Quaternion rotation)
    {
        GameObject enemy = objectPoolManager.SpawnPool(stageSO.monsterPrefab, position, rotation);
        return enemy;
    }
}
