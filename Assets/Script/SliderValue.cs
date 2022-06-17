using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityVolumeRendering;

public class SliderValue : MonoBehaviour
{
    private float max;
    private float min;

    [SerializeField]
    private TextMeshPro textMesh = null;

    [SerializeField]
    private PinchSlider currentSlider;

    [SerializeField]
    private bool isMinSlider;

    private Dictionary<int, float> sliderValueStore;
    private GameObject currentGameObj;

    public void setMaxMin(float maxInput, float minInput)
    {
        max = maxInput;
        min = minInput;
        Debug.Log("value set");
    }

    public float[] getMaxMin()
    {

        float[] maxmin = { max, min };
        return maxmin;
    }

    //Activates on selecting option in dropdown
    public void onObjectGiven(GameObject givenObject)
    {
        if (sliderValueStore == null)
        {
            sliderValueStore = new Dictionary<int, float>();
        }
        currentGameObj = givenObject;
        if (currentGameObj.TryGetComponent(out VolumeRenderedObject currentVolume))
        {
            max = currentVolume.dataset.GetMaxDataValue();
            min = currentVolume.dataset.GetMinDataValue();
            Debug.Log(max);
            Debug.Log(min);
            if (sliderValueStore.ContainsKey(currentGameObj.GetInstanceID()))
            {
                float sliderValue;
                sliderValueStore.TryGetValue(currentGameObj.GetInstanceID(), out sliderValue);
                currentSlider.SliderValue = sliderValue;
            }
            else
            {
                if (isMinSlider)
                {
                    sliderValueStore.Add(currentGameObj.GetInstanceID(), 0f);
                    currentSlider.SliderValue = 0f;
                }
                else
                {
                    sliderValueStore.Add(currentGameObj.GetInstanceID(), 1f);
                    currentSlider.SliderValue = 1f;
                }
            }
        }
        else
        {
            max = 0f;
            min = 0f;
            if (isMinSlider)
            {
                currentSlider.SliderValue = 0f;
            }
            else
            {
                currentSlider.SliderValue = 1f;
            }
        }

    }

    public void OnSliderUpdated(SliderEventData eventData)
    {
        if (sliderValueStore == null)
        {
            sliderValueStore = new Dictionary<int, float>();
        }
        if ((max == 0) && (min == 0))
        {
            textMesh.text = "Max and Min values not set for slider";
        }
        else
        {
            textMesh.text = (((eventData.NewValue) * (max - min)) + min).ToString();
            if (sliderValueStore.ContainsKey(currentGameObj.GetInstanceID()))
            {
                sliderValueStore[currentGameObj.GetInstanceID()] = eventData.NewValue;
            }
            else
            {
                Debug.Log("Slider Key not yet set");
            }

        }

    }

}