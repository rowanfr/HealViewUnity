using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityVolumeRendering;

public class SliderValue : MonoBehaviour
{
    private double max;
    private double min;

    [SerializeField]
    private TextMeshPro textMesh = null;

    private VolumeRenderedObject currentGameobj;

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

    public void onObjectGiven(VolumeRenderedObject givenObject)
    {
        max = givenObject.dataset.GetMaxDataValue();
        min = givenObject.dataset.GetMinDataValue();
    }

    public void OnSliderUpdated(SliderEventData eventData)
    {
        if((max == 0) && (min == 0)){
            textMesh.text = "Error: Max and Min values not set for slider";
        }
        else
        {
            textMesh.text = (((eventData.NewValue) * (max - min)) + min).ToString();
        }

        if(currentGameobj != null)
        {
            //currentGameobj.
        }
        
    }

}
