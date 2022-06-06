using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class USRender : MonoBehaviour
{
    float TimeInterval;
    int currentUS;
    Mesh[] AllUSMesh;
    public bool USOn;
    public bool USPause;

    [SerializeField]
    private Mesh mesh_01;
    [SerializeField]
    private Mesh mesh_02;
    [SerializeField]
    private Mesh mesh_03;
    [SerializeField]
    private Mesh mesh_04;
    [SerializeField]
    private Mesh mesh_05;
    [SerializeField]
    private Mesh mesh_06;
    [SerializeField]
    private Mesh mesh_07;
    [SerializeField]
    private Mesh mesh_08;
    [SerializeField]
    private Mesh mesh_09;
    [SerializeField]
    private Mesh mesh_10;
    [SerializeField]
    private Mesh mesh_11;
    [SerializeField]
    private Mesh mesh_12;
    [SerializeField]
    private Mesh mesh_13;
    [SerializeField]
    private Mesh mesh_14;
    [SerializeField]
    private Mesh mesh_15;
    [SerializeField]
    private Mesh mesh_16;
    [SerializeField]
    private Mesh mesh_17;
    [SerializeField]
    private Mesh mesh_18;
    [SerializeField]
    private Mesh mesh_19;
    [SerializeField]
    private Mesh mesh_20;
    [SerializeField]
    private Mesh mesh_21;
    [SerializeField]
    private Mesh mesh_22;
    [SerializeField]
    private Mesh mesh_23;


    [SerializeField] private Material currentMaterial;

    void Start()
    {

        AllUSMesh = new Mesh[] { mesh_01, mesh_02, mesh_03, mesh_04, mesh_05, mesh_06, mesh_07, mesh_08, mesh_09, mesh_10, mesh_11, mesh_12, mesh_13, mesh_14, mesh_15, mesh_16, mesh_17, mesh_18, mesh_19, mesh_20, mesh_21, mesh_22, mesh_23 };
        currentUS = 0;
        USOn = true;
        USPause = false;
    }

    public void setOn(bool isOn)
    {
        USOn = isOn;
    }

    public void setPause(bool isPause)
    {
        USPause = isPause;
    }

    // Update is called once per frame
    void Update()
    {
        // ones per in seconds
        TimeInterval += Time.deltaTime;
        if (TimeInterval >= 0.1 && !USPause)
        {
            currentUS += 1;
            currentUS %= 23;
            TimeInterval = 0;
            // Performance friendly code here
        }
        if (USOn)
        {
            Graphics.DrawMesh(AllUSMesh[currentUS], gameObject.transform.position, gameObject.transform.rotation, currentMaterial, 0);
        }
    }
}
