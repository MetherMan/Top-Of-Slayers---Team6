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
        if(other.CompareTag("Player"))
        {
            PlayerHP playerHP = other.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(attackDamage);
            }
            ReturnPool();
        }
    }

    private void ReturnPool()
    {
        ObjectPoolManager.Instance.ReturnPool(bulletPrefab, this.gameObject);
    }
}
