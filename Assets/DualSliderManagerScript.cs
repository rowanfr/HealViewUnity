using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;
using Microsoft.MixedReality.Toolkit.UI;

public class DualSliderManagerScript : MonoBehaviour
{
    private VolumeRenderedObject currentGameobj;
    float sliderAValue = 0;
    float sliderBValue = 1;
    Vector2 currentMinMax;

    // Start is called before the first frame update

    public void OnSliderUpdatedA(SliderEventData eventData)
    {
        sliderAValue = eventData.NewValue;
        currentMinMax = orderMinMax(sliderAValue, sliderBValue);
        currentGameobj.SetVisibilityWindow(currentMinMax);
    }

    public void OnSliderUpdatedB(SliderEventData eventData)
    {
        sliderBValue = eventData.NewValue;
        currentMinMax = orderMinMax(sliderAValue, sliderBValue);
        currentGameobj.SetVisibilityWindow(currentMinMax);
    }

    private Vector2 orderMinMax(float A, float B)
    {
        if (A == B)
        {
            return new Vector2(0, 1);
        } else if (A>B)
        {
            return new Vector2(B, A);
        } else
        {
            return new Vector2(A, B);
        }
    }

    public void onObjectGiven(VolumeRenderedObject givenObject)
    {
        currentGameobj = givenObject;
    }

}
