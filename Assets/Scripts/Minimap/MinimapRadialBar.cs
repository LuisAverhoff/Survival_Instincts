using UnityEngine;
using UnityEngine.UI;

public class MinimapRadialBar : MonoBehaviour
{
    [SerializeField]private Image radialBar;
    [SerializeField]private float radialValue = 0;
    [SerializeField]private float maxRadialValue = 100;
    [SerializeField]private float maxRadialRotation = 180;

    void Awake()
    {
        setRadialBar();
    }

    public void setRadialBar()
    {
        float amount = (radialValue / maxRadialValue) * maxRadialRotation / 360.0f;
        radialBar.fillAmount = amount;
    }

    public void reduceRadialValue(float amountToReduceBy)
    {
        radialValue -= amountToReduceBy;
    }

    public float getRadialValue()
    {
        return radialValue;
    }

    public float getMaxRadialValue()
    {
        return maxRadialValue;
    }
}
