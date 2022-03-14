using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
public class MarchingCubeTranslate : MonoBehaviour
{
    public struct XYZ
    {
        public Vector3 position;//3 array x,y,z in each of those values
    }

    //Can replace XYZ[] with Vector3[] directly. This is only currently being used to mirror http://paulbourke.net/geometry/polygonise/
    public struct TRIANGLE
    {
        public XYZ[] vertices;//3 array values for triangle verts, x,y,z in each of those values
    }

    public struct GRIDCELL
    {
        public XYZ[] vertices;//8 array values for cube verts, x,y,z in each of those values
        public float[] val;//8 val for
    }




    //Isolevel is cutoff point for 


    MarchingCubeTables DataTable = new MarchingCubeTables();

    GRIDCELL makeGrid(float[,,] examinedMatrix, int x, int y, int z)
    {
        GRIDCELL grid = new GRIDCELL();
        //Must call new here as we are dynamically declaring space for array and the compiler doesn't infer that from setting it equal to an array

        Debug.Log("examiniedMatrix");
        for (uint i = 0; i < examinedMatrix.GetLength(0); i++)
        {
            for (uint j = 0; j < examinedMatrix.GetLength(1); j++)
            {
                for (uint k = 0; k < examinedMatrix.GetLength(2); k++)
                {
                    Debug.Log("examiniedMatrix Value [" + i.ToString() + "," + j.ToString() + "," + k.ToString() + "]");
                    Debug.Log(examinedMatrix[i, j, k]);
                }
            }
        }

        grid.val = new float[8] {
            examinedMatrix[x, y, z], //0 point
            examinedMatrix[(x + 1), y, z], //1 x point
            examinedMatrix[(x + 1), (y + 1), z], //1 y and 1 x point
            examinedMatrix[x, (y + 1), z], //1 y point
            examinedMatrix[x, y, (z + 1)], //1 z point
            examinedMatrix[(x + 1), y, (z + 1)], //1 x and 1 z
            examinedMatrix[(x + 1), (y + 1), (z + 1)],// 1 x,y, and z
            examinedMatrix[x, y + 1, (z + 1)]//1 x and 1 y
        };

        Debug.Log("Gridval creation");
        for (uint n = 0; n < grid.val.Length; n++)
        {
            Debug.Log("GridVal " + n.ToString());
            Debug.Log(grid.val[n]);
        }

        grid.vertices = new XYZ[8];
        grid.vertices[0].position = new Vector3(x, y, z);
        grid.vertices[1].position = new Vector3((x + 1), y, z);
        grid.vertices[2].position = new Vector3((x + 1), (y + 1), z);
        grid.vertices[3].position = new Vector3(x, (y + 1), z);
        grid.vertices[4].position = new Vector3(x, y, (z + 1));
        grid.vertices[5].position = new Vector3((x + 1), y, (z + 1));
        grid.vertices[6].position = new Vector3((x + 1), (y + 1), (z + 1));
        grid.vertices[7].position = new Vector3(x, y + 1, (z + 1));


        Debug.Log("Gridvert creation");
        for (uint n = 0; n < grid.val.Length; n++)
        {
            Debug.Log("Gridvert " + n.ToString());
            Debug.Log(grid.vertices[n].position);
        }

        return grid;
    }

    Mesh meshBuild()
    {

        float isolevel = 0.5f;
        //Human [x,y,z]
        //The 100 and 1 should be at different heights if interpolation is working as expected
        float[,,] testArray = { { { 0, 100, 1 }, { 0, 0, 0 }, { 0, 0, 0 } }, { { 0, 1, 0 }, { 0, 0, 0 }, { 0, 1, 0 } }, { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } } };

        Debug.Log("Start Mesh Build");
        Debug.Log(testArray);

        Mesh mesh = new Mesh();

        List<Vector3> triangleVertList = new List<Vector3>(mesh.vertices);
        List<int> trianglesSequence = new List<int>(mesh.triangles);

        Debug.Log("Array Sizes");
        Debug.Log("x");
        Debug.Log(testArray.GetLength(0));
        Debug.Log("y");
        Debug.Log(testArray.GetLength(1));
        Debug.Log("z");
        Debug.Log(testArray.GetLength(2));

        for (int z = 0; z < (testArray.GetLength(0) - 1); z++)
        {
            for (int y = 0; y < (testArray.GetLength(1) - 1); y++)
            {
                for (int x = 0; x < (testArray.GetLength(2) - 1); x++)
                {
                    GRIDCELL grid = makeGrid(testArray, z, y, x);

                    byte cubeindex = 0;
                    if (grid.val[0] < isolevel) cubeindex |= 1;
                    if (grid.val[1] < isolevel) cubeindex |= 2;
                    if (grid.val[2] < isolevel) cubeindex |= 4;
                    if (grid.val[3] < isolevel) cubeindex |= 8;
                    if (grid.val[4] < isolevel) cubeindex |= 16;
                    if (grid.val[5] < isolevel) cubeindex |= 32;
                    if (grid.val[6] < isolevel) cubeindex |= 64;
                    if (grid.val[7] < isolevel) cubeindex |= 128;

                    /* Cube is entirely in/out of the surface */
                    if (DataTable.edgeTable[cubeindex] == 0) ;

                    Debug.Log("GridVal");
                    for (uint n = 0; n < grid.val.Length; n++)
                    {
                        Debug.Log("GridVal " + n.ToString());
                        Debug.Log(grid.val[n]);
                    }

                    /* Find the vertices where the surface intersects the cube */

                    XYZ[] vertlist = new XYZ[12];
                    //One inputs the grid components into a vertexInterp function to get an actual vertex point to make the triangles later on
                    //Consider vertexList as being the edges of the current scanning grid and the output of VertexInterp being the position on that edge
                    //For legibility we can use (DataTable.edgeTable[cubeindex] & n) != 0  or (DataTable.edgeTable[cubeindex] & n) == n as both are testing the same thing
                    //Why is C# like this, one has to set == to a value instead of leaving if(Type)
                    if ((DataTable.edgeTable[cubeindex] & 1) != 0)
                        vertlist[0].position =
                           VertexInterp(isolevel, grid.vertices[0].position, grid.vertices[1].position, grid.val[0], grid.val[1]);
                    if ((DataTable.edgeTable[cubeindex] & 2) != 0)
                        vertlist[1].position =
                           VertexInterp(isolevel, grid.vertices[1].position, grid.vertices[2].position, grid.val[1], grid.val[2]);
                    if ((DataTable.edgeTable[cubeindex] & 4) != 0)
                        vertlist[2].position =
                           VertexInterp(isolevel, grid.vertices[2].position, grid.vertices[3].position, grid.val[2], grid.val[3]);
                    if ((DataTable.edgeTable[cubeindex] & 8) != 0)
                        vertlist[3].position =
                           VertexInterp(isolevel, grid.vertices[3].position, grid.vertices[0].position, grid.val[3], grid.val[0]);
                    if ((DataTable.edgeTable[cubeindex] & 16) != 0)
                        vertlist[4].position =
                           VertexInterp(isolevel, grid.vertices[4].position, grid.vertices[5].position, grid.val[4], grid.val[5]);
                    if ((DataTable.edgeTable[cubeindex] & 32) != 0)
                        vertlist[5].position =
                           VertexInterp(isolevel, grid.vertices[5].position, grid.vertices[6].position, grid.val[5], grid.val[6]);
                    if ((DataTable.edgeTable[cubeindex] & 64) != 0)
                        vertlist[6].position =
                           VertexInterp(isolevel, grid.vertices[6].position, grid.vertices[7].position, grid.val[6], grid.val[7]);
                    if ((DataTable.edgeTable[cubeindex] & 128) != 0)
                        vertlist[7].position =
                           VertexInterp(isolevel, grid.vertices[7].position, grid.vertices[4].position, grid.val[7], grid.val[4]);
                    if ((DataTable.edgeTable[cubeindex] & 256) != 0)
                        vertlist[8].position =
                           VertexInterp(isolevel, grid.vertices[0].position, grid.vertices[4].position, grid.val[0], grid.val[4]);
                    if ((DataTable.edgeTable[cubeindex] & 512) != 0)
                        vertlist[9].position =
                           VertexInterp(isolevel, grid.vertices[1].position, grid.vertices[5].position, grid.val[1], grid.val[5]);
                    if ((DataTable.edgeTable[cubeindex] & 1024) != 0)
                        vertlist[10].position =
                           VertexInterp(isolevel, grid.vertices[2].position, grid.vertices[6].position, grid.val[2], grid.val[6]);
                    if ((DataTable.edgeTable[cubeindex] & 2048) != 0)
                        vertlist[11].position =
                           VertexInterp(isolevel, grid.vertices[3].position, grid.vertices[7].position, grid.val[3], grid.val[7]);

                    Debug.Log("VertList position");
                    for (uint n = 0; n < vertlist.Length; n++)
                    {
                        Debug.Log("VertList " + n.ToString());
                        Debug.Log(vertlist[n].position);
                    }

                    /* Create the triangle */

                    //The maximum number of triangles in a singe marching cube unit is 5
                    List<TRIANGLE> triangles = new List<TRIANGLE>();
                    //Must declare new XYZ to avoid error "object reference not set to instance of object
                    //Can't declare new inside for loop as the context is only inside the for loop and I assume it's garbage collected after

                    //This counts the number of triangles in a single cube unit
                    int ntriang = 0;

                    //It's loading them up into individual triangles to render
                    //-1 is needed at the end of the table because it is the completion condition when iterating by 3
                    for (uint i = 0; DataTable.triTable[cubeindex, i] != -1; i += 3)
                    {
                        Debug.Log("Cube Index");
                        Debug.Log(cubeindex);
                        Debug.Log("i iteration");
                        Debug.Log(i);
                        Debug.Log("tritable value");
                        Debug.Log(DataTable.triTable[cubeindex, i]);
                        Debug.Log(DataTable.triTable[cubeindex, (i + 1)]);
                        Debug.Log(DataTable.triTable[cubeindex, (i + 2)]);
                        //Must place this inside for loop to add reference to triangles within it. Without this or if this was outside of the for loop the object values would be 0,0,0
                        TRIANGLE triangleLoop = new TRIANGLE();
                        triangleLoop.vertices = new XYZ[3];

                        triangleLoop.vertices[0].position = vertlist[DataTable.triTable[cubeindex, i]].position;
                        triangleLoop.vertices[1].position = vertlist[DataTable.triTable[cubeindex, i + 1]].position;
                        triangleLoop.vertices[2].position = vertlist[DataTable.triTable[cubeindex, i + 2]].position;

                        //Used a list here because when previously trying to use an array here variables would not be assigned properly and instead external obj had default 0,0,0
                        triangles.Add(triangleLoop);
                        ntriang++;

                    }

                    Debug.Log(ntriang);

                    Debug.Log("Triangles Array");
                    //n represents number of triangles
                    for (int n = 0; n < ntriang; n++)
                    {
                        Debug.Log("Triangle Point Values " + n.ToString());
                        Debug.Log(triangles[n].vertices[0].position * 10);
                        Debug.Log(triangles[n].vertices[1].position * 10);
                        Debug.Log(triangles[n].vertices[2].position * 10);
                    }




                    //n represents num of triangles
                    for (int n = 0; n < ntriang; n++)
                    {
                        triangleVertList.Add(triangles[n].vertices[0].position * 10);
                        triangleVertList.Add(triangles[n].vertices[1].position * 10);
                        triangleVertList.Add(triangles[n].vertices[2].position * 10);
                    }

                    Debug.Log("Triangles Loaded");
                    for (int n = 0; n < (ntriang * 3); n += 3)
                    {
                        Debug.Log("Triangle Group Loaded: " + n.ToString());
                        Debug.Log(triangleVertList[n]);
                        Debug.Log(triangleVertList[n + 1]);
                        Debug.Log(triangleVertList[n + 2]);
                    }




                    Debug.Log("Mesh Input");
                    for (int n = 0; n < mesh.vertices.Length; n += 3)
                    {
                        Debug.Log("Mesh Input Triangles: " + n.ToString());
                        Debug.Log(mesh.vertices[n]);
                        Debug.Log(mesh.vertices[n + 1]);
                        Debug.Log(mesh.vertices[n + 2]);
                    }

                    //This is what organizes the numbering of the triangle sequence
                    //This is the simplest option, which is in effect count the number of items currently in the list and add 3 sequentially
                    //This is only possible due to the vertex sequnce of triangles consistently being point1 -> point2 -> point3
                    //Were this not the case we would need to organize this for proper arrangements, luckily currently we dont
                    //n is the number of point indeces, because of this ntriang must be multipplied by 3 as it represents a triangle and not a single index point
                    //
                    int currentNumTriangles = trianglesSequence.Count;
                    for (int n = currentNumTriangles; n < ((ntriang * 3) + currentNumTriangles); n += 3)
                    {
                        trianglesSequence.Add(n);
                        trianglesSequence.Add(n + 1);
                        trianglesSequence.Add(n + 2);
                    }

                    Debug.Log("Triangles Loaded");
                    for (int n = 0; n < trianglesSequence.Count; n += 3)
                    {
                        Debug.Log("Triangle Group Order" + n.ToString());
                        Debug.Log(trianglesSequence[n]);
                        Debug.Log(trianglesSequence[n + 1]);
                        Debug.Log(trianglesSequence[n + 2]);
                    }

                    Debug.Log(triangleVertList.Count);

                    Debug.Log(trianglesSequence.Count);

                }
            }
        }
        for (int n = 0; n < triangleVertList.Count; n++)
        {
            Debug.Log(triangleVertList[n]);
        }
        Debug.Log(triangleVertList.Count);
        Debug.Log(trianglesSequence.Count);
        Debug.Log("Hello");

        mesh.vertices = triangleVertList.ToArray();
        mesh.triangles = trianglesSequence.ToArray();

        return mesh;
    }

    //This function interpolated the grid vertices and returns the interpolated edge point
    //It first check to see if one value is very close to the isolevel and returns that value or if both values are nearly adentical
    //
    Vector3 VertexInterp(float isolevel, Vector3 pos1, Vector3 pos2, float valuePos1, float valuePos2)
    {
        float mu;
        XYZ point = new XYZ();
        //Due to the edge being determined here we know that one value is above the isolevel while one value is below, but we don't know which
        //Additionally this presumption comes from the marching cube tables that lead to this function call.

        //if value at pos 1 = value at pos 2 return midpoint between them, remember that Math.Abs is involved
        if (Math.Abs(valuePos1 - valuePos2) < 0.00001)
            return ((pos1 + pos2) / 2);

        //if isolevel = valuePos1, return Pos1, remember that Math.Abs is involved
        /*This works becsause this is effectively stating on the edge in question if val1 and val2 aren't equal and we know that 
         * 1 val is above iso and one is below then we know that if one val is equal to iso then the position of the edge
         * should be right at the point on the grid. In this case we are testing for val1.
        */
        if (Math.Abs(isolevel - valuePos1) < 0.00001)
            return (pos1);

        //if isolevel = valuePos2, return Pos2, remember that Math.Abs is involved
        /*This works becsause this is effectively stating on the edge in question if val1 and val2 aren't equal and we know that 
         * 1 val is above iso and one is below then we know that if one val is equal to iso then the position of the edge
         * should be right at the point on the grid. In this case we are testing for val2.
        */
        if (Math.Abs(isolevel - valuePos2) < 0.00001)
            return (pos2);


        /*mu = gradient for edge position placement
         * we have 2 values at point 1 and 2. We know that one of them is above the isolevel and one is below.
         * mu = (isolevel - valuePos1) / (valuePos2 - valuePos1) is first looking at the difference between the isolevel and the value at pos1
         * it then divides this by the total value from these positions. 
         * This is to calculate the point between a linear gradient of these 2 values that it would equal the isolevel.
         * Lets first consider our starting point to be P1. P1 has value V1
         * P2 has value V2. We want to know where a linear gradient between these 2 values equals the isolavel so
         * y = mx + c
         * c = V1 as it is our initial offset for the y axis (y is matrix intensity)
         * m = Rise / Run = (V2 - V1)/d
         * y = Isolevel
         * Therefore:
         * Isolevel = ((V2 - V1)/d)*x + V1 from the rise/run idea multiplied by a factor of x
         * with d being edge distance and x being the actual amount of distance needed to travel
         * (This currently assumes that V1 > isovalue and V2 < isovalue)
         * ((Isolevel - V1)*d) / (V2 - V1) = x
         * d = determinant(pos2 - pos1) or in just one axis: d = (pos2 - pos1)
         * we also need to after this add pos1 as our origin point to ensure that x of the vertex distance on the edge
         * is applied at the correct location, P1. Otherwise it will have no offset and will be applied offset from the origin 0, rather than it's actual position.
         * mu = (Isolevel - V1) / (V2 - V1)
         * So our final point is given by P1 + x = P1 + (Isolevel - V1)*d / (V2 - V1) = mu * d = mu * (pos2 - pos1)
         */
        mu = (isolevel - valuePos1) / (valuePos2 - valuePos1);
        //(pos2.x - pos1.x) is edge midpoint x vector
        //that times mu is
        point.position.x = pos1.x + mu * (pos2.x - pos1.x);
        point.position.y = pos1.y + mu * (pos2.y - pos1.y);
        point.position.z = pos1.z + mu * (pos2.z - pos1.z);

        return point.position;
    }

    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();

        mesh.Clear();
        mesh = meshBuild();
        GetComponent<MeshFilter>().mesh = mesh;
        for (int n = 0; n < mesh.vertices.Length; n++)
        {
            Debug.Log(mesh.vertices[n]);
        }
        for (int n = 0; n < mesh.triangles.Length; n++)
        {
            Debug.Log(mesh.triangles[n]);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
