using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float minIntensity;
    public float maxIntensity;
    public Light campfireLight;
	
	// Update is called once per frame
	void Update ()
    {
        campfireLight.intensity = Random.Range(minIntensity, maxIntensity);
	}
}
