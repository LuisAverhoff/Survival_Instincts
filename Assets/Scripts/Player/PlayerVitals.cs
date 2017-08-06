using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerVitals : MonoBehaviour
{
    [SerializeField]private GameObject healthRadialBar;
    [SerializeField]private GameObject thirstRadialBar;
    [SerializeField]private GameObject hungerRadialBar;
    [SerializeField]private GameObject staminaRadialBar;

    private MinimapRadialBar healthSlider;
    private MinimapRadialBar thirstSlider;
    private MinimapRadialBar hungerSlider;
    private MinimapRadialBar staminaSlider;

    [SerializeField]private float healthFallRate;
    [SerializeField]private float thirstFallRate;
    [SerializeField]private float hungerFallRate;
    private float staminaFallRate;
    [SerializeField]private float staminaFallMult;
    private float staminaRegainRate;
    [SerializeField]private float staminaRegainMult;

    private FirstPersonController playerController;

    // Use this for initialization
    void Start ()
    {
        playerController = GetComponent<FirstPersonController>();

        healthSlider = healthRadialBar.GetComponent<MinimapRadialBar>();
        thirstSlider = thirstRadialBar.GetComponent<MinimapRadialBar>();
        hungerSlider = hungerRadialBar.GetComponent<MinimapRadialBar>();
        staminaSlider = staminaRadialBar.GetComponent<MinimapRadialBar>();
        staminaFallRate = 1.0f;
        staminaRegainRate = 1.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (hungerSlider.getRadialValue() <= 0 && thirstSlider.getRadialValue() <= 0)
        {
            healthSlider.setRadialBar(Time.deltaTime / healthFallRate * 2);
        }
        else if(hungerSlider.getRadialValue() <= 0 || thirstSlider.getRadialValue() <= 0)
        {
            healthSlider.setRadialBar(Time.deltaTime / healthFallRate);
        }

        if(healthSlider.getRadialValue() <= 0)
        {
            killCharacter();
        }

        hungerSlider.setRadialBar(Time.deltaTime / hungerFallRate);
        thirstSlider.setRadialBar(Time.deltaTime / thirstFallRate);

        if(playerController.isPlayerRunning())
        {
            staminaSlider.setRadialBar(Time.deltaTime / staminaFallRate * staminaFallMult);
        }
        else
        {
            staminaSlider.setRadialBar(Time.deltaTime / staminaRegainRate * staminaRegainMult);
        }

        playerController.setRunningSpeed(staminaSlider.getRadialValue());
    }

    private void killCharacter()
    {
        Debug.Log("You are dead!!!");
    }
}
