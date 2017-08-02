using UnityEngine;
using UnityEngine.UI;

public class CalculateGameTime : MonoBehaviour
{
    [SerializeField] private DayNightController dayNightController;
    [SerializeField] private Text timeString;

	// Update is called once per frame
	void Update ()
    {
        float currentTime = dayNightController.getCurrentTimeOfDay();
        timeString.text = constructTimeString(currentTime);
	}

    private string constructTimeString(float currentTime)
    {
        //Is it am of pm?
        string AMPM = "";
        float minutes = ((currentTime) - (Mathf.Floor(currentTime))) * 60.0f;

        if (currentTime <= 12.0f)
        {
            AMPM = "AM";

        }
        else
        {
            AMPM = "PM";
        }

        //Make the final string
        return Mathf.Floor(currentTime).ToString() + " : " + minutes.ToString("00") + " " + AMPM;
    }
}
