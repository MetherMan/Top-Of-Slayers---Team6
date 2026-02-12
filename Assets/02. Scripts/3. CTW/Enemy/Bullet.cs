using UnityEngine;

public class Bullet : MonoBehaviour
{
    private GameObject bulletPrefab;
    private float bulletSpeed;
    private Vector3 bulletDir;
    private float lifeTime = 3f;
    private float timer;

    public void Init(GameObject bulletPrefab, float bulletSpeed, Vector3 bulletDir)
    {
        this.bulletPrefab = bulletPrefab;
        this.bulletSpeed = bulletSpeed;
        this.bulletDir = bulletDir;
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
            ReturnPool();
        }
    }

    private void ReturnPool()
    {
        ObjectPoolManager.Instance.ReturnPool(bulletPrefab, this.gameObject);
    }
}
