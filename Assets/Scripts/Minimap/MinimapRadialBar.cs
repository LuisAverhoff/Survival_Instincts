using UnityEngine;
using UnityEngine.UI;

public class MinimapRadialBar : MonoBehaviour
{
    [SerializeField]private Image radialBar;
    [SerializeField]private float radialValue = 100;
    private float maxRadialValue;
    [SerializeField]private float radialRotation = 180;
    private float maxRadialRotation;

    void Awake()
    {
        maxRadialValue = radialValue;
        maxRadialRotation = radialRotation / 360.0f;
        setRadialBar(0);
    }

    public void setRadialBar(float amountToReduceBy)
    {
        radialValue = Mathf.Clamp(radialValue - amountToReduceBy, 0, maxRadialValue);

        if(radialValue > 0 && radialValue < maxRadialValue)
        {
            radialBar.fillAmount = Mathf.Clamp01(radialValue / maxRadialValue * maxRadialRotation);
        }
    }

    public float getRadialValue()
    {
        return radialValue;
    }
}
