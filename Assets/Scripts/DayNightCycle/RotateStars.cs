using UnityEngine;

public class RotateStars : MonoBehaviour
{
    public Transform stars;

    // Update is called once per frame
    void Update ()
    {
        stars.transform.rotation = transform.rotation;
    }
}
