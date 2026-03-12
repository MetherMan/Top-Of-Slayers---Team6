using UnityEngine;

public class EnemyIndicator : MonoBehaviour
{
    private Transform enemy;
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Camera mainCam;
    [SerializeField] private float margin = 80f;

    void Update()
    {
        if (enemy == null)
        {
            FindCloseEnemy();
            return;
        }

        if (!enemy.gameObject.activeInHierarchy)
        {
            enemy = null;
            return;
        }

        Vector3 view = mainCam.WorldToViewportPoint(enemy.position);

        bool isOffScreen = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;

        arrow.gameObject.SetActive(isOffScreen);

        if (!isOffScreen) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(enemy.position);

        Vector3 dir = (screenPos - new Vector3(Screen.width / 2f, Screen.height / 2f)).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arrow.rotation = Quaternion.Euler(0, 0, angle + 90f);

        float x = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
        float y = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);

        arrow.position = new Vector2(x, y);
    }

    private void FindCloseEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDistance = Mathf.Infinity;
        Transform close = null;

        foreach (var e in enemies)
        {
            float distance = Vector3.Distance(player.transform.position, e.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                close = e.transform;
            }
        }
        enemy = close;
    }
}
