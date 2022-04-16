using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

public class SliderValue : MonoBehaviour
{
    private double max;
    private double min;

    [SerializeField]
    private TextMeshPro textMesh = null;

    public void setMaxMin(double maxInput, double minInput)
    {
        max = maxInput;
        min = minInput;
        Debug.Log("value set");
    }

    public double[] getMaxMin()
    {
        
        double[] maxmin  = { max, min};
        return maxmin;
    }

    public void OnSliderUpdated(SliderEventData eventData)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        if (textMesh != null)
        {
            textMesh.text = (((eventData.NewValue) * (max - min)) + min).ToString();
        }
    }

}
