using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float maxXDistance;
    [SerializeField] private float maxZDistance;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        Vector3 cameraPosition = player.position;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, -maxXDistance, maxXDistance);
        cameraPosition.z = Mathf.Clamp(cameraPosition.z, -maxZDistance, maxZDistance);
        cameraPosition.y = 25f;

        transform.position = cameraPosition;
    }
}
