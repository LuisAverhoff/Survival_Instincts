using UnityEngine;

public class PlayerVitals : MonoBehaviour
{
    [SerializeField]private GameObject healthRadialBar;
    [SerializeField]private GameObject thirstRadialBar;
    [SerializeField]private GameObject hungerRadialBar;

    private MinimapRadialBar healthSlider;
    private MinimapRadialBar thirstSlider;
    private MinimapRadialBar hungerSlider;

    [SerializeField]private float healthFallRate;
    [SerializeField]private float thirstFallRate;
    [SerializeField]private float hungerFallRate;

    // Use this for initialization
    void Start ()
    {

        healthSlider = healthRadialBar.GetComponent<MinimapRadialBar>();
        thirstSlider = thirstRadialBar.GetComponent<MinimapRadialBar>();
        hungerSlider = hungerRadialBar.GetComponent<MinimapRadialBar>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (hungerSlider.getRadialValue() <= 0 && thirstSlider.getRadialValue() <= 0)
        {
            healthSlider.reduceRadialValue(Time.deltaTime / healthFallRate * 2);
        }
        else if(hungerSlider.getRadialValue() <= 0 || thirstSlider.getRadialValue() <= 0)
        {
            healthSlider.reduceRadialValue(Time.deltaTime / healthFallRate);
        }

        healthSlider.setRadialBar();

        if(healthSlider.getRadialValue() <= 0)
        {
            killCharacter();
        }

        if(hungerSlider.getRadialValue() >= 0)
        {
            hungerSlider.reduceRadialValue(Time.deltaTime / hungerFallRate);
        }
        else if(hungerSlider.getRadialValue() <= 0)
        {
            hungerSlider.reduceRadialValue(0);
        }
        else if(hungerSlider.getRadialValue() >= hungerSlider.getMaxRadialValue())
        {
            hungerSlider.reduceRadialValue(hungerSlider.getMaxRadialValue());
        }

        hungerSlider.setRadialBar();

        if (thirstSlider.getRadialValue() >= 0)
        {
            thirstSlider.reduceRadialValue(Time.deltaTime / thirstFallRate);
        }
        else if (thirstSlider.getRadialValue() <= 0)
        {
            thirstSlider.reduceRadialValue(0);
        }
        else if (thirstSlider.getRadialValue() >= thirstSlider.getMaxRadialValue())
        {
            thirstSlider.reduceRadialValue(thirstSlider.getMaxRadialValue());
        }

        thirstSlider.setRadialBar();
    }

    private void killCharacter()
    {
        Debug.Log("You are dead!!!");
    }
}
