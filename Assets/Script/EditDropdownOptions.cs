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
    private List<GameObject> listedObjects;

    [SerializeField]
    //private UnityEvent<double[,,], float, float, float> ArrayWithDetailsFound;
    private UnityEvent<GameObject> updateMenu;

    [SerializeField]
    private UnityEvent shutdownMenu;

    public void getNewList()
    {
        VolumeRenderedObject[] volumeObjsComponent = GameObject.FindObjectsOfType<VolumeRenderedObject>();
        GameObject parentObjImport = GameObject.Find("NonDicomObjects");

        TextMeshProDropdown = gameObject.GetComponent(typeof(TMP_Dropdown)) as TMP_Dropdown;
        TextMeshProDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> TMPDropNameList = new List<TMP_Dropdown.OptionData>();
        Debug.Log(volumeObjsComponent.Length);
        listedObjects = new List<GameObject>();
        for (int n = 0; n < volumeObjsComponent.Length; n++)
        {
            if (TMPDropNameList.Contains(new TMP_Dropdown.OptionData(volumeObjsComponent[n].name)))
            {
                TMPDropNameList.Add(new TMP_Dropdown.OptionData(volumeObjsComponent[n].name + volumeObjsComponent[n].GetInstanceID()));
            }
            else
            {
                TMPDropNameList.Add(new TMP_Dropdown.OptionData(volumeObjsComponent[n].name));
            }
            
            listedObjects.Add(volumeObjsComponent[n].gameObject);
        }
        Debug.Log(parentObjImport.transform.childCount);
        foreach (Transform child in parentObjImport.transform)
        {
            if (TMPDropNameList.Contains(new TMP_Dropdown.OptionData(child.gameObject.name)))
            {
                TMPDropNameList.Add(new TMP_Dropdown.OptionData(child.gameObject.name + child.gameObject.GetInstanceID()));
            } else
            {
                TMPDropNameList.Add(new TMP_Dropdown.OptionData(child.gameObject.name));
            }
            
            listedObjects.Add(child.gameObject);
        }

        TextMeshProDropdown.AddOptions(TMPDropNameList);
        
        Debug.Log(listedObjects.Count);


        if (listedObjects.Count > 0)
        {
            updateMenu.Invoke(listedObjects[0]);
        } else
        {
            shutdownMenu.Invoke();
        }
        
    }

    //Activates upon selection of result
    public void useSelectedMesh(System.Int32 Result)
    {
        
        Debug.Log(Result);
        if (listedObjects.Count > 1)
        {
            updateMenu.Invoke(listedObjects[Result]);
        }
        else if (listedObjects.Count == 1)
        {
            updateMenu.Invoke(listedObjects[0]);
        } else
        {
            shutdownMenu.Invoke();
        }
        
    }

}