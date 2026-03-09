using UnityEngine;

public class Bullet : MonoBehaviour
{
    private GameObject bulletPrefab;
    private float bulletSpeed;
    private Vector3 bulletDir;
    private int attackDamage;
    private float lifeTime = 3f;
    private float timer;

    public void Init(GameObject bulletPrefab, float bulletSpeed, Vector3 bulletDir, int attackDamage)
    {
        this.bulletPrefab = bulletPrefab;
        this.bulletSpeed = bulletSpeed;
        this.bulletDir = bulletDir;
        this.attackDamage = attackDamage;
        timer = 0f;
    }
    void Update()
    {
        transform.position += bulletDir * bulletSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if(timer >= lifeTime)
        {
            ReturnPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (!TryResolvePlayerRoot(other, out var playerRoot)) return;

        if (EnemyBase.TryApplyPlayerDamage(playerRoot, attackDamage))
        {
            ReturnPool();
        }
    }

    private static bool TryResolvePlayerRoot(Collider other, out GameObject playerRoot)
    {
        playerRoot = null;

        if (TryResolvePlayerRoot(other.gameObject, out playerRoot))
        {
            return true;
        }

        if (other.attachedRigidbody != null && TryResolvePlayerRoot(other.attachedRigidbody.gameObject, out playerRoot))
        {
            return true;
        }

        var combatResource = other.GetComponentInParent<PlayerCombatResource>();
        if (combatResource != null)
        {
            playerRoot = combatResource.gameObject;
            return true;
        }

        var playerHp = other.GetComponentInParent<PlayerHP>();
        if (playerHp != null)
        {
            playerRoot = playerHp.gameObject;
            return true;
        }

        return false;
    }

    private static bool TryResolvePlayerRoot(GameObject candidate, out GameObject playerRoot)
    {
        playerRoot = null;
        if (candidate == null) return false;

        var combatResource = candidate.GetComponent<PlayerCombatResource>();
        if (combatResource != null)
        {
            playerRoot = combatResource.gameObject;
            return true;
        }

        var playerHp = candidate.GetComponent<PlayerHP>();
        if (playerHp != null)
        {
            playerRoot = playerHp.gameObject;
            return true;
        }

        return false;
    }

    private void ReturnPool()
    {
        ObjectPoolManager.Instance.ReturnPool(bulletPrefab, this.gameObject);
    }
}
