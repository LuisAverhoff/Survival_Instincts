using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    public GameObject player;

    private Quaternion originalRotation;

    private Vector3 offset;

    void Awake()
    {
        originalRotation = transform.rotation;
    }

    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    void LateUpdate()
    {
        transform.rotation = originalRotation;
        transform.position = player.transform.position + offset;
    }
}
