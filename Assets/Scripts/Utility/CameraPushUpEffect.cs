using UnityEngine;
using InControl;

public class CameraPushUpEffect : MonoBehaviour
{
    public float force; // controls recoil amplitude.
    public float upSpeed; // controls smoothing speed;
    public float downSpeed; // How fast the weapons returns to original position.

    private Vector3 initialAngle;
    private float targetX; // unfiltered recoil angle.
    private Vector3 smoothedAngle = Vector3.zero;

    void Start()
    {
        initialAngle = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update ()
    {
        InputDevice device = InputManager.ActiveDevice;

        if(device.RightBumper)
        {
            targetX += force;
        }

        //smooth movement a little
        smoothedAngle.x = Mathf.Lerp(smoothedAngle.x, targetX, upSpeed * Time.deltaTime);
        // move the camera
        transform.localEulerAngles = initialAngle - smoothedAngle;
        // Return to resting position.
        targetX = Mathf.Lerp(targetX, 0.0f, downSpeed * Time.deltaTime);
    }
}
