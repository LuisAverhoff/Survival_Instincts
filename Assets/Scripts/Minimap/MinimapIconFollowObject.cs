using UnityEngine;

public class MinimapIconFollowObject : MonoBehaviour
{
    public GameObject objectToFollow;
    private float iconOriginalPositionY;

    void Start()
    {
        iconOriginalPositionY = transform.position.y;
    }

    void LateUpdate()
    {
        Vector3 minimapIconRotation = new Vector3(90, objectToFollow.transform.rotation.eulerAngles.y, objectToFollow.transform.rotation.eulerAngles.z);
        transform.rotation = Quaternion.Euler(minimapIconRotation);
        Vector3 minimapIconPosition = new Vector3(objectToFollow.transform.position.x, iconOriginalPositionY + objectToFollow.transform.position.y, objectToFollow.transform.position.z);
        transform.position = minimapIconPosition;
    }
}
