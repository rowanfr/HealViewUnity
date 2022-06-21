using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;
using UnityEngine.Events;


using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;//Provides BoundsControl
using Microsoft.MixedReality.Toolkit.Input;//NearInteractionGrabbable

using FileBrowser;



public class ButtonInterfaceScript : MonoBehaviour
{
    private GameObject currentGameObj;

    private Vector3 objPosition;
    private Quaternion objRotation;
    private Vector3 objScale;
    //Implement defaults in dictionary as swapping between 2 objects will
    Dictionary<string, Vector3> defaultPositions = new Dictionary<string, Vector3>();
    Dictionary<string, Quaternion> defaultRotations = new Dictionary<string, Quaternion>();
    Dictionary<string, Vector3> defaultScales = new Dictionary<string, Vector3>();

    private List<GameObject> attachedObjects = new List<GameObject>();


    [SerializeField]
    private UnityEvent deleteItem;


    //Activates on selecting option in dropdown
    public void onObjectGiven(GameObject givenObject)
    {
        objPosition = new Vector3(0, 0, 0);
        objRotation = new Quaternion(0, 0, 0, 0);
        objScale = new Vector3(0, 0, 0);

        Debug.Log("Given Object");
        Debug.Log(givenObject);
        currentGameObj = givenObject;
        if (defaultPositions.ContainsKey(currentGameObj.GetInstanceID() + "Position"))
        {
            Debug.Log("The Default paramater for this gameobject already exists");
        }
        else
        {
            defaultPositions.Add(currentGameObj.GetInstanceID() + "Position", currentGameObj.transform.position);
        }

        if (defaultRotations.ContainsKey(currentGameObj.GetInstanceID() + "Rotation"))
        {
            Debug.Log("The Default paramater for this gameobject already exists");
        }
        else
        {
            defaultRotations.Add(currentGameObj.GetInstanceID() + "Rotation", currentGameObj.transform.rotation);
        }

        if (defaultScales.ContainsKey(currentGameObj.GetInstanceID() + "Scale"))
        {
            Debug.Log("The Default paramater for this gameobject already exists");
        }
        else
        {
            defaultScales.Add(currentGameObj.GetInstanceID() + "Scale", currentGameObj.transform.localScale);
        }
    }

    public void attachModel(GameObject gameObjectToAttach)
    {
        bool alreadyIncluded = checkIfInAttachedList(gameObjectToAttach);
        if ((gameObjectToAttach != currentGameObj) && (!alreadyIncluded))
        {
            if (gameObjectToAttach.TryGetComponent(out VolumeRenderedObject attachGameVolume) && currentGameObj.TryGetComponent(out VolumeRenderedObject currentGameVolume))
            {
                //Get Default Scale:
                Vector3 defaultImportScale = new Vector3();
                defaultScales.TryGetValue(gameObjectToAttach.GetInstanceID() + "Scale", out defaultImportScale);

                Vector3 defaultScale = new Vector3();
                defaultScales.TryGetValue(currentGameObj.GetInstanceID() + "Scale", out defaultScale);

                Vector3 findalImportScale = new Vector3(defaultImportScale.x / defaultScale.x, defaultImportScale.y / defaultScale.y, defaultImportScale.z / defaultScale.z);

                Destroy(gameObjectToAttach.GetComponent<BoxCollider>());
                Destroy(gameObjectToAttach.GetComponent<BoundsControl>());
                Destroy(gameObjectToAttach.GetComponent<NearInteractionGrabbable>());
                Destroy(gameObjectToAttach.GetComponent<ObjectManipulator>());
                Destroy(gameObjectToAttach.GetComponent<ConstraintManager>());

                gameObjectToAttach.transform.SetParent(currentGameObj.transform);
                gameObjectToAttach.transform.localPosition = new Vector3(0, 0, 0);
                gameObjectToAttach.transform.localRotation = new Quaternion(0, 0, 0, 0);
                gameObjectToAttach.transform.localScale = findalImportScale;

            }
            else
            {
                //Get Default Scale:
                Vector3 defaultImportScale = new Vector3();
                defaultScales.TryGetValue(gameObjectToAttach.GetInstanceID() + "Scale", out defaultImportScale);

                Vector3 defaultScale = new Vector3();
                defaultScales.TryGetValue(currentGameObj.GetInstanceID() + "Scale", out defaultScale);

                Vector3 findalImportScale = new Vector3(defaultImportScale.x / defaultScale.x, defaultImportScale.y / defaultScale.y, defaultImportScale.z / defaultScale.z);

                Destroy(gameObjectToAttach.GetComponent<BoxCollider>());
                Destroy(gameObjectToAttach.GetComponent<BoundsControl>());
                Destroy(gameObjectToAttach.GetComponent<NearInteractionGrabbable>());
                Destroy(gameObjectToAttach.GetComponent<ObjectManipulator>());
                Destroy(gameObjectToAttach.GetComponent<ConstraintManager>());

                gameObjectToAttach.transform.SetParent(currentGameObj.transform);
                gameObjectToAttach.transform.localScale = findalImportScale;
            }
        }
    }

    private bool checkIfInAttachedList(GameObject gameObjectToAttach)
    {
        for (int n = 0; n < attachedObjects.Count; n++)
        {
            if (gameObjectToAttach == attachedObjects[n])
            {
                return true;
            }
        }
        return false;
    }

    public void removeObject()
    {
        GameObject.DestroyImmediate(currentGameObj);//This is necessary because if we only use destroy the deleteItem Event will miscount the number of current items
        deleteItem.Invoke();
    }
    public void changeTransferFunction()
    {
        if (currentGameObj.TryGetComponent(out VolumeRenderedObject currentGameVolume))
        {
            string[] options = { "Transfer Function", "tf,tf2d" };
            string path = new InternalFileBrowser().getFileBrowser(options);
            if ((path != null) && (path != ""))
            {
                if (path.EndsWith("tf2d"))
                {
                    TransferFunction2D newTF = TransferFunctionDatabase.LoadTransferFunction2D(path);
                    if (newTF != null)
                    {
                        currentGameVolume.SetTransferFunctionMode(TFRenderMode.TF2D);
                        currentGameVolume.transferFunction2D = newTF;
                        currentGameVolume.transferFunction2D.GenerateTexture();
                        currentGameVolume.SetTransferFunctionMode(TFRenderMode.TF2D);
                        currentGameVolume.transferFunction2D = newTF;
                        currentGameVolume.transferFunction2D.GenerateTexture();
                    }

                }
                else
                {
                    TransferFunction newTF = TransferFunctionDatabase.LoadTransferFunction(path);
                    if (newTF != null)
                    {
                        currentGameVolume.SetTransferFunctionMode(TFRenderMode.TF1D);
                        currentGameVolume.transferFunction = newTF;
                        currentGameVolume.transferFunction.GenerateTexture();
                        currentGameVolume.SetTransferFunctionMode(TFRenderMode.TF1D);
                    }

                }
            }
        }
    }
    public void spawnCuttingPlane()
    {
        if (currentGameObj.TryGetComponent(out VolumeRenderedObject currentGameVolume))
        {
            GameObject currentCuttingPlane = GameObject.Find(currentGameObj.GetInstanceID() + "CuttingPlane");
            if (currentCuttingPlane == null)
            {
                VolumeObjectFactory.SpawnNamedCrossSectionPlane(currentGameVolume, currentGameObj.GetInstanceID() + "CuttingPlane");
            }
            else
            {
                GameObject.Destroy(currentCuttingPlane);
            }
        }
    }
    public void spawnCutingBox()
    {
        if (currentGameObj.TryGetComponent(out VolumeRenderedObject currentGameVolume))
        {
            GameObject currentCuttingBox = GameObject.Find(currentGameObj.GetInstanceID() + "CuttingBox");
            if (currentCuttingBox == null)
            {
                VolumeObjectFactory.SpawnNamedCutoutBox(currentGameVolume, currentGameObj.GetInstanceID() + "CuttingBox");
            }
            else
            {
                GameObject.Destroy(currentCuttingBox);
            }
        }

    }
    public void resetObject()
    {
        Vector3 relocatePosition;
        Quaternion relocateRotation;
        Vector3 relocateScale;
        defaultPositions.TryGetValue(currentGameObj.GetInstanceID() + "Position", out relocatePosition);
        defaultRotations.TryGetValue(currentGameObj.GetInstanceID() + "Rotation", out relocateRotation);
        defaultScales.TryGetValue(currentGameObj.GetInstanceID() + "Scale", out relocateScale);

        currentGameObj.GetComponent<Transform>().position = relocatePosition;
        currentGameObj.GetComponent<Transform>().rotation = relocateRotation;
        currentGameObj.GetComponent<Transform>().localScale = relocateScale;
    }
}