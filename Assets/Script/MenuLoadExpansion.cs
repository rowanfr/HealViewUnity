using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLoadExpansion : MonoBehaviour
{
    private GameObject quad;
    private GameObject closeButton;

    // Update is called once per frame
    void Awake()
    {
        quad = GameObject.Find("HandMenuContent/Backplate/Quad");
        closeButton = GameObject.Find("HandMenuContent/HandButtonClose");
    }

    public void FileExpansion()
    {
        closeButton.transform.localPosition = new Vector3(0.11f, 0.05f, 0);
        quad.transform.localPosition = new Vector3(0.2f, 0, 0);
        quad.transform.localScale = new Vector3(1.2f, 0.55f, 1f);
    }
}
