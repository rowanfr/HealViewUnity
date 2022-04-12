using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class NewMeshAddedButtonsShift : MonoBehaviour
{
    private GameObject slateGrid;
    private GameObject slateGUI;
    private GameObject optionsMenu;
    private double arrayMin;
    private double arrayMax;

    [SerializeField]
    public GameObject optionsPrefab;

    public async void setNecessaryValFromArray(double[,,] DICOMArray, float xscale, float yscale, float zscale)
    {
        double[] minMax = await Task.Run(() => getArrayMinMax(DICOMArray));
        arrayMin = minMax[0];
        arrayMax = minMax[1];
        Debug.Log(arrayMin);
        Debug.Log(arrayMax);
    }

    public double[] getArrayMinMax(double[,,] DICOMArray)
    {
        double min = DICOMArray[0, 0, 0];
        double max = DICOMArray[0, 0, 0];
        for (int z = 0; z < (DICOMArray.GetLength(2)); z++)
        {
            for (int y = 0; y < (DICOMArray.GetLength(1)); y++)
            {
                for (int x = 0; x < (DICOMArray.GetLength(0)); x++)
                {
                    if (max < DICOMArray[x, y, z])
                    {
                        max = DICOMArray[x, y, z];
                    }

                    if (min > DICOMArray[x, y, z])
                    {
                        min = DICOMArray[x, y, z];
                    }
                }
            }
        }
        double[] minMax = { min, max };
        return minMax;

    }
    public void shiftButtonsHost()
    {
        slateGrid = GameObject.Find("OptionsMenu/OptionsMenuContent/SlateContainer/SlateUGUI/UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout");
        optionsMenu = GameObject.Find("OptionsMenu");
        slateGUI = GameObject.Find("OptionsMenu/OptionsMenuContent/SlateContainer/SlateUGUI");

        RectTransform slateArea = GameObject.Find("OptionsMenu/OptionsMenuContent/SlateContainer/SlateUGUI/UGUIScrollViewContent").GetComponent<RectTransform>();

        BoxCollider slateCollider = slateGUI.GetComponent<BoxCollider>();

        GameObject Column = Instantiate(optionsPrefab) as GameObject;

        Column.transform.parent = slateGrid.transform;
        //For some reason these were appearing at a z coordinate 650 units off
        Column.transform.localPosition = new Vector3(Column.transform.localPosition.x, Column.transform.localPosition.y, 0);
        //For some reason these were appearing at a scale of 2500
        Column.transform.localScale = new Vector3(1, 1, 1);
        Column.GetComponentInChildren<SliderValue>().setMax(arrayMax);
        Column.GetComponentInChildren<SliderValue>().setMin(arrayMin);

        int numGrids = slateGrid.transform.childCount;

        slateArea.sizeDelta = new Vector2(500 * numGrids, 800);
        slateCollider.size = new Vector3(0.5f * numGrids, 0.8f, slateCollider.size.z);
        
        Debug.Log(numGrids);
        this.transform.position = optionsMenu.transform.position + new Vector3(0.075f + (0.1f * numGrids), 0,0);
    }
}
