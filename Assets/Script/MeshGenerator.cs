using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;//Count Method

//These are requiring the referenced Unity object have these components
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshGenerator : MonoBehaviour
{
    //Mesh is a unity engine class
    Mesh mesh;

    //Vector3 is unities 3D vector and point representation
    Vector3[] vertices;
    int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        //Putting Mesh onto heap, no need to deallocate
        mesh = new Mesh();

        //"GetComponent" is a generic method for "GameObject"s class. Syntax is function<Type>(ref/out/_ Type args). Method declared with type for different effects. For GetComponent the type is the member class of the current unity GameObject you want to access properties from.
        //Generic methods can also be considered Function(typeof(Type)) as Type;
        GetComponent<MeshFilter>().mesh = mesh;
        

        CreateShape();
        UpdateMesh();

        GetComponent<MeshCollider>().convex = true;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void CreateShape ()
    {
        vertices = new Vector3[]
        {
            new Vector3 (0,0,0),
            new Vector3 (0,0,10),
            new Vector3 (10,0,0),
            new Vector3 (0,10,0)
        };
        //Triangles references Vector 0 then 1 then 2
        //Remember when declaring triangles backfaces are determined by order. Clockwise order for points is front and anti-clockwise order is backface
        triangles = new int[]
        {//X and Z make up ground plane, y is vertical plane, 0 = 0, 1 = z, 2=x,3=y
            2, 1, 0,
            3, 1, 2,
            3, 0, 1,
            0, 3, 2,
            -1, -1, -1
        };
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        int totalNum = triangles.Count(value => value == -1);
        Debug.Log(totalNum);
        
        triangles = triangles.Where(value => value != -1).ToArray();
        
        mesh.triangles = triangles;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
