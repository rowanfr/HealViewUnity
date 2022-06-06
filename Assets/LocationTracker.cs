using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationTracker : MonoBehaviour
{
    private Vector3 objPosition;
    private Quaternion objRotation;
    private Vector3 objScale;
    // Start is called before the first frame update

    private void Start()
    {
        objPosition = new Vector3( 0, 0, 0 );
        objRotation = new Quaternion(0, 0, 0, 0);
        objScale = new Vector3(0, 0, 0);
    }
    public void setTransformData()
    {
        objPosition = gameObject.GetComponent<Transform>().position;
        objRotation = gameObject.GetComponent<Transform>().rotation;
        objScale = gameObject.GetComponent<Transform>().localScale;
    }

    // Update is called once per frame
    public void repositionObjectFromTransform()
    {
        if (objScale.x != 0) {
            gameObject.GetComponent<Transform>().position = objPosition;
            gameObject.GetComponent<Transform>().rotation = objRotation;
            gameObject.GetComponent<Transform>().localScale = objScale;
        }
    }
}
