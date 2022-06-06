using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;

public class pawnCutsReset : MonoBehaviour
{

    Transform SliceTransform;
    Transform gameObjTransform;

    VolumeRenderedObject volObj;
    void Start()
    {
        volObj = gameObject.GetComponent<VolumeRenderedObject>();
        gameObjTransform = gameObject.transform;
    }

    // Start is called before the first frame update
    public void spawnCut()
    {
        SlicingPlane sliceplane = volObj.CreateSlicingPlane();
        SliceTransform = sliceplane.gameObject.transform;
    }

    public void spawnBox()
    {
        
    }
}
