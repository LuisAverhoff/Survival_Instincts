using UnityEngine;
using InControl;

public class ToggleLight : MonoBehaviour
{
    private Light toggableLight;
    private bool hasLightChangedState;

    // Use this for initialization
    void Start ()
    {
        toggableLight = GetComponent<Light>();
        hasLightChangedState = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        InputDevice device = InputManager.ActiveDevice;

        if (device.RightStickButton)
        {
            if (!hasLightChangedState)
            {
                toggableLight.enabled = !toggableLight.enabled;
                hasLightChangedState = true;
            }
        }
        else
        {
            hasLightChangedState = false;
        }
    }
}
