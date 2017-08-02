using UnityEngine;
using InControl;

public class SimpleMouseLook : MonoBehaviour
{
    private float rotationX;
    private float rotationY;
    public float xSensitivity;
    public float ySensitivity;

    public float minimumX;
    public float maximumX;

    public bool lockCursor;
    private bool isCursorLocked;

    void Start()
    {
        rotationX = 0;
        rotationY = 0;
        isCursorLocked = true;
    }

    // Update is called once per frame
    void Update ()
    {
        InputDevice device = InputManager.ActiveDevice;

        rotationX -= device.RightStickY * xSensitivity;
        rotationY += device.RightStickX * xSensitivity;

        rotationX = Mathf.Clamp(rotationX, minimumX, maximumX);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);

        updateCursorLock();
	}

    public void updateCursorLock()
    {
        if (lockCursor)
            internalLockUpdate();
    }

    private void internalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            isCursorLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isCursorLocked = true;
        }

        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
