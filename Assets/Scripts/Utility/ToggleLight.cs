using UnityEngine;
using InControl;

public class ToggleLight : MonoBehaviour
{
    private Light toggableLight;
    [SerializeField] private float timeToWaitToToggle = 1.0f;
    private float toggleTimer;
    private bool hasLightChangedState;

    // Use this for initialization
    void Start ()
    {
        toggableLight = GetComponent<Light>();
        toggleTimer = 0.0f;
        hasLightChangedState = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        InputDevice device = InputManager.ActiveDevice;

        if (device.Action2)
        {
            toggleTimer += Time.deltaTime;

            if (toggleTimer > timeToWaitToToggle && !hasLightChangedState)
            {
                toggableLight.enabled = !toggableLight.enabled;
                hasLightChangedState = true;
            }
        }
        else
        {
            toggleTimer = 0.0f;
            hasLightChangedState = false;
        }
    }
}
