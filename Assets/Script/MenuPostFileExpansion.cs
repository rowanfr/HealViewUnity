using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPostFileExpansion : MonoBehaviour
{
    private GameObject quad;
    
    // Update is called once per frame
    void Awake()
    {
        quad = GameObject.Find("HandMenuContent/Backplate/Quad");
    }

    public void FileExpansion()
    {
        quad.transform.localPosition = new Vector3(0.3f, 0, 0);
        quad.transform.localScale = new Vector3(1.2f, 0.55f, 1f);
    }
}
