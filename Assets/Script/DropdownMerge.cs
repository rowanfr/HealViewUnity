using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityVolumeRendering;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Dropdown))]

public class DropdownMerge : MonoBehaviour
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
        TMPDropNameList.Add(new TMP_Dropdown.OptionData(""));
        listedObjects = new List<GameObject>();//On Value change in dropdown only works if value is changed from default 0 so this is put here as junk data to force a user change
        for (int n = 0; n < volumeObjsComponent.Length; n++)
        {
            TMPDropNameList.Add(new TMP_Dropdown.OptionData(volumeObjsComponent[n].name));
            listedObjects.Add(volumeObjsComponent[n].gameObject);
        }
        Debug.Log(parentObjImport.transform.childCount);
        foreach (Transform child in parentObjImport.transform)
        {
            TMPDropNameList.Add(new TMP_Dropdown.OptionData(child.gameObject.name));
            listedObjects.Add(child.gameObject);
        }

        TextMeshProDropdown.AddOptions(TMPDropNameList);

        Debug.Log(listedObjects.Count);


        if (!(listedObjects.Count > 0))
        {
            shutdownMenu.Invoke();
        }

    }

    //Activates upon selection of result
    public void useSelectedMesh(System.Int32 Result)
    {

        Debug.Log(Result);
        updateMenu.Invoke(listedObjects[Result - 1]);
        shutdownMenu.Invoke();

    }

}
