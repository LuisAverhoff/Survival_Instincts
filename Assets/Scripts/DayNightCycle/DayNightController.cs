using UnityEngine;

[System.Serializable]
public class DayNightController : MonoBehaviour
{
    public Gradient nightDayColor;
    //Speed of the cycle (if you set this to 1 the one hour in the cycle will pass in 1 real life second)
    public float daySpeedMultiplier = 0.1f;
	//main directional light
	public Light sunLight;
	//control intensity of sun?
	public bool controlIntensity = true;
    //what time this cycle should start
    [Range(0, 24)]
    public float startTime = 12.0f;
	//what's the current time
	private float currentTime = 0.0f;
	public string timeString = "00:00 AM";
	//x rotation value of the light
	private float xValueOfSun = 90.0f;
	//Rotation speed of spheres
	[SerializeField]	public Transform starSphere;
    private ParticleSystem starParticleSystem;
	//star's rotation speed
	public float starRotationSpeed = 0.15f;

 	// Use this for initialization
	void Start ()
    {
		//set the start time
		currentTime = startTime;
        starParticleSystem = starSphere.GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		//increment time
		currentTime += Time.deltaTime*daySpeedMultiplier;
		//reset time
		if (currentTime >= 24.0f)
        {
			currentTime %= 24.0f;
		}
		//Check for sunlight
		if (sunLight)
        {
			ControlLight();
		}

		//Check for starsphere
		if (starSphere)
        {
			enableOrDisableStars();
		}

		//Gets The timeString;
		CalculateTime ();
	}

	private void ControlLight()
    {
		//Rotate light
		xValueOfSun = -(90.0f+currentTime*15.0f);
		sunLight.transform.eulerAngles = sunLight.transform.right*xValueOfSun;
        sunLight.color = nightDayColor.Evaluate(currentTime/24.0f);
        RenderSettings.ambientLight = sunLight.color;

		//reset angle
		if (xValueOfSun >= 360.0f)
        {
			xValueOfSun = 0.0f;
		}
		//This basically turn on and off the sun light based on day / night
		if (controlIntensity && sunLight && (currentTime >= 18.0f || currentTime <= 5.5f))
        {
			sunLight.intensity = Mathf.MoveTowards(sunLight.intensity,0.0f,Time.deltaTime*daySpeedMultiplier*10.0f);
		}
        else if (controlIntensity && sunLight)
        {
			sunLight.intensity = Mathf.MoveTowards(sunLight.intensity,1.0f,Time.deltaTime*daySpeedMultiplier*10.0f);
		}

	}

	private void enableOrDisableStars()
    {
		starSphere.transform.Rotate(Vector3.forward*starRotationSpeed*daySpeedMultiplier*Time.deltaTime);

        Color currentColor = starParticleSystem.GetComponent<Renderer>().material.GetColor("_TintColor");
        Color newColor;

        if (currentTime > 5.5f && currentTime < 18.0f && starParticleSystem)
        {
		    newColor = new Color (currentColor.r,currentColor.g,currentColor.b,Mathf.Lerp(currentColor.a , 0.0f,Time.deltaTime*50.0f*daySpeedMultiplier));
        }
        else
        {
            newColor = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(currentColor.a, 1.0f, Time.deltaTime * 50.0f * daySpeedMultiplier));
        }

        starParticleSystem.GetComponent<Renderer>().material.SetColor("_TintColor", newColor);
    }

	private void CalculateTime ()
    {
		//Is it am of pm?
		string AMPM = "";
		float minutes = ((currentTime) - (Mathf.Floor(currentTime)))*60.0f;

		if (currentTime <= 12.0f)
        {
			AMPM = "AM";

		}
        else
        {
			AMPM = "PM";
		}

		//Make the final string
		timeString = Mathf.Floor(currentTime).ToString() + " : " + minutes.ToString("F0") + " "+AMPM ;

	}

}
