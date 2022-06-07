using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;

public class ButtonInterfaceScript : MonoBehaviour
{
    private VolumeRenderedObject currentGameobj;

    private Vector3 objPosition;
    private Quaternion objRotation;
    private Vector3 objScale;
    //Implement defaults in dictionary as swapping between 2 objects will
    Dictionary<string, Vector3> defaultPositions = new Dictionary<string, Vector3>();
    Dictionary<string, Quaternion> defaultRotations = new Dictionary<string, Quaternion>();
    Dictionary<string, Vector3> defaultScales = new Dictionary<string, Vector3>();
    // Start is called before the first frame update


    public void onObjectGiven(VolumeRenderedObject givenObject)
    {
        objPosition = new Vector3(0, 0, 0);
        objRotation = new Quaternion(0, 0, 0, 0);
        objScale = new Vector3(0, 0, 0);

        Debug.Log("Given Object");
        Debug.Log(givenObject);
        currentGameobj = givenObject;
        if(defaultPositions.ContainsKey(currentGameobj.name + "Position"))
        {
            Debug.Log("The Default paramater for this gameobject already exists");
        }
        else
        {
            defaultPositions.Add(currentGameobj.name + "Position", currentGameobj.transform.position);
        }

        if (defaultRotations.ContainsKey(currentGameobj.name + "Rotation"))
        {
            Debug.Log("The Default paramater for this gameobject already exists");
        }
        else
        {
            defaultRotations.Add(currentGameobj.name + "Rotation", currentGameobj.transform.rotation);
        }

        if (defaultScales.ContainsKey(currentGameobj.name + "Scale"))
        {
            Debug.Log("The Default paramater for this gameobject already exists");
        }
        else
        {
            defaultScales.Add(currentGameobj.name + "Scale", currentGameobj.transform.localScale);
        }
    }

    public void storeLocation()
    {
        objPosition = currentGameobj.GetComponent<Transform>().position;
        objRotation = currentGameobj.GetComponent<Transform>().rotation;
        objScale = currentGameobj.GetComponent<Transform>().localScale;
    }
    public void resetLocation()
    {
        currentGameobj.GetComponent<Transform>().position = objPosition;
        currentGameobj.GetComponent<Transform>().rotation = objRotation;
        currentGameobj.GetComponent<Transform>().localScale = objScale;
    }
    public void spawnCuttingPlane()
    {
        GameObject currentCuttingPlane = GameObject.Find(currentGameobj.name + "CuttingPlane");
        if (currentCuttingPlane == null)
        {
            VolumeObjectFactory.SpawnNamedCrossSectionPlane(currentGameobj, currentGameobj.name + "CuttingPlane");
        }
        else
        {
            GameObject.Destroy(currentCuttingPlane);
        }
    }
    public void spawnCutingBox()
    {
        GameObject currentCuttingBox = GameObject.Find(currentGameobj.name + "CuttingBox");
        if (currentCuttingBox == null)
        {
            VolumeObjectFactory.SpawnNamedCutoutBox(currentGameobj, currentGameobj.name + "CuttingBox");
        }
        else
        {
            GameObject.Destroy(currentCuttingBox);
        }

    }
    public void relocateObjectPosition()
    {
        Vector3 relocatePosition;
        Quaternion relocateRotation;
        Vector3 relocateScale;
        defaultPositions.TryGetValue(currentGameobj.name + "Position", out relocatePosition);
        defaultRotations.TryGetValue(currentGameobj.name + "Rotation", out relocateRotation);
        defaultScales.TryGetValue(currentGameobj.name + "Scale", out relocateScale);

        currentGameobj.GetComponent<Transform>().position = relocatePosition;
        currentGameobj.GetComponent<Transform>().rotation = relocateRotation;
        currentGameobj.GetComponent<Transform>().localScale = relocateScale;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
