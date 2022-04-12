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

    public void setMax(double maxInput)
    {
        max = maxInput;
    }
    public void setMin(double mixInput)
    {
        min = mixInput;
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
