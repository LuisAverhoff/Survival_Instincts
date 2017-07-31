using UnityEngine;
using InControl;

public class WeaponSway : MonoBehaviour
{
    public float amount;
    public float maxAmountX;
    public float maxAmountY;
    public float smoothAmount;

    private Vector3 initialPosition;

    // Use this for initialization
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        InputDevice device = InputManager.ActiveDevice;

        float movementX = -device.RightStickX * amount;
        float movementY = -device.RightStickY * amount;

        movementX = Mathf.Clamp(movementX, -maxAmountX, maxAmountX);
        movementY = Mathf.Clamp(movementY, -maxAmountY, maxAmountY);

        Vector3 finalPosition = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
}
