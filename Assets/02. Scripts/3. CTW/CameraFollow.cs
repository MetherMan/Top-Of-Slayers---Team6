using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float maxXDistance = 10f;
    [SerializeField] private float maxZDistance;

    private float startX;
    private float startZ;
    [SerializeField] private LayerMask groundLayer;

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
        cameraPosition.y = player.position.y + 25f;

        transform.position = cameraPosition;
    }

    private void OnDrawGizmos()
    {
        Camera camera = GetComponent<Camera>();

        Gizmos.color = Color.red;

        Vector3[] corners = new Vector3[4];
        Vector3[] groundCorners =
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Ray ray = camera.ViewportPointToRay(groundCorners[i]);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                corners[i] = hit.point;
            }
            else
            {
                corners[i] = ray.GetPoint(30f);
            }
        }

        for(int i = 0; i < corners.Length; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
        }
    }
}
