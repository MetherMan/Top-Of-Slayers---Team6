using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float maxXDistance = 10f;
    [SerializeField] private float maxZDistance;

    private float startX;
    private float startZ;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        startX = player.position.x;
        startZ = player.position.z;
    }

    private void LateUpdate()
    {
        Vector3 cameraPosition = player.position;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, startX - maxXDistance, startX+maxXDistance);
        cameraPosition.z = Mathf.Clamp(cameraPosition.z, startZ - maxZDistance, startZ+maxZDistance);
        cameraPosition.y = 25f;

        transform.position = cameraPosition;
    }
}
