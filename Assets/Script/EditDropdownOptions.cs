using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityVolumeRendering;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Dropdown))]

public class EditDropdownOptions : MonoBehaviour
{
    private TMP_Dropdown TextMeshProDropdown;
    private VolumeRenderedObject[] volobjs;

    [SerializeField]
    //private UnityEvent<double[,,], float, float, float> ArrayWithDetailsFound;
    private UnityEvent<VolumeRenderedObject> updateMenu;

    

    public void getNewList()
    {
        TextMeshProDropdown = gameObject.GetComponent(typeof(TMP_Dropdown)) as TMP_Dropdown;
        TextMeshProDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> TMPDropNameList = new List<TMP_Dropdown.OptionData>();
        volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
        for (int n = 0; n < volobjs.Length; n++)
        {
            TMPDropNameList.Add(new TMP_Dropdown.OptionData(volobjs[n].name));
        }
        TextMeshProDropdown.AddOptions(TMPDropNameList);

        updateMenu.Invoke(volobjs[0]);
    }

    public void useSelectedMesh(System.Int32 Result)
    {
        Debug.Log(Result);
        if (volobjs.Length > 1)
        {
            updateMenu.Invoke(volobjs[Result]);
        } else
        {
            updateMenu.Invoke(volobjs[0]);
        }
        
    }
}
