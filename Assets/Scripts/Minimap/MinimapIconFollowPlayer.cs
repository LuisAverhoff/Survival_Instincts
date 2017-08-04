using UnityEngine;

public class MinimapIconFollowPlayer : MonoBehaviour
{
    public GameObject player;

    void LateUpdate()
    {
        Vector3 minimapIconRotation = new Vector3(90, player.transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z);
        transform.rotation = Quaternion.Euler(minimapIconRotation);
        transform.position = player.transform.position;
    }
}
