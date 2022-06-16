using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;
using Microsoft.MixedReality.Toolkit.UI;

public class DualSliderManagerScript : MonoBehaviour
{
    private GameObject currentGameObj;
    float sliderAValue = 0;
    float sliderBValue = 1;
    Vector2 currentMinMax;

    // Start is called before the first frame update

    public void OnSliderUpdatedA(SliderEventData eventData)
    {
        if (currentGameObj != null)
        {
            if (currentGameObj.TryGetComponent(out VolumeRenderedObject currentGameVolume))
            {
                sliderAValue = eventData.NewValue;
                currentMinMax = orderMinMax(sliderAValue, sliderBValue);
                currentGameVolume.SetVisibilityWindow(currentMinMax);
            }
        }
    }

    public void OnSliderUpdatedB(SliderEventData eventData)
    {
        if (currentGameObj != null)
        {
            if (currentGameObj.TryGetComponent(out VolumeRenderedObject currentGameVolume))
            {
                sliderBValue = eventData.NewValue;
                currentMinMax = orderMinMax(sliderAValue, sliderBValue);
                currentGameVolume.SetVisibilityWindow(currentMinMax);
            }
        }
    }

    private Vector2 orderMinMax(float A, float B)
    {
        Debug.Log("Value A: " + A.ToString());
        Debug.Log("Value B: " + B.ToString());

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

    //Activates on selecting option in dropdown
    public void onObjectGiven(GameObject givenObject)
    {
        if (givenObject.TryGetComponent(out VolumeRenderedObject givenVolume))
        {
            currentGameObj = givenVolume.gameObject;
        } 
        else
        {
            currentGameObj = new GameObject();
        }
    }

}
