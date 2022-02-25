using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MarchingCube : MonoBehaviour
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

    int i,ntriang;
    byte cubeindex = 0;
    XYZ[] vertlist;

    //Isolevel is cutoff point for 
    float isolevel = 0.5f;

    float[,,] testArray = { { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } }, { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } }, { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } } };
    float zSpacing = 10;
    float ySpacing = 10;
    float xSpacing = 10;

    MarchingCubeTables DataTable = new MarchingCubeTables();

    GRIDCELL makeGrid()
    {
        GRIDCELL grid = new GRIDCELL();
        int zSize = testArray.GetLength(0);
        int ySize = testArray.GetLength(1);
        int xSize = testArray.GetLength(2);
        return grid;
    }
    uint meshBuild()
    {
        GRIDCELL grid = makeGrid();
        

        TRIANGLE tri = new TRIANGLE();
        tri.vertices[0].position = new Vector3(0, 0, 0);
        
        if (grid.val[0] < isolevel) cubeindex |= 1;
        if (grid.val[1] < isolevel) cubeindex |= 2;
        if (grid.val[2] < isolevel) cubeindex |= 4;
        if (grid.val[3] < isolevel) cubeindex |= 8;
        if (grid.val[4] < isolevel) cubeindex |= 16;
        if (grid.val[5] < isolevel) cubeindex |= 32;
        if (grid.val[6] < isolevel) cubeindex |= 64;
        if (grid.val[7] < isolevel) cubeindex |= 128;

        /* Cube is entirely in/out of the surface */
        if (DataTable.edgeTable[cubeindex] == 0)
            return 0;

        /* Find the vertices where the surface intersects the cube */
        //Why is C# like this
        if ((DataTable.edgeTable[cubeindex] & 1) == 1)
            vertlist[0].position =
               VertexInterp(isolevel, grid.vertices[0].position, grid.vertices[1].position, grid.val[0], grid.val[1], false);
        if ((DataTable.edgeTable[cubeindex] & 2) == 2)
            vertlist[1].position =
               VertexInterp(isolevel, grid.vertices[1].position, grid.vertices[2].position, grid.val[1], grid.val[2], false);
        if ((DataTable.edgeTable[cubeindex] & 4) == 4)
            vertlist[2].position =
               VertexInterp(isolevel, grid.vertices[2].position, grid.vertices[3].position, grid.val[2], grid.val[3], false);
        if ((DataTable.edgeTable[cubeindex] & 8) == 8)
            vertlist[3].position =
               VertexInterp(isolevel, grid.vertices[3].position, grid.vertices[0].position, grid.val[3], grid.val[0], false);
        if ((DataTable.edgeTable[cubeindex] & 16) == 16)
            vertlist[4].position =
               VertexInterp(isolevel, grid.vertices[4].position, grid.vertices[5].position, grid.val[4], grid.val[5], false);
        if ((DataTable.edgeTable[cubeindex] & 32) == 32)
            vertlist[5].position =
               VertexInterp(isolevel, grid.vertices[5].position, grid.vertices[6].position, grid.val[5], grid.val[6], false);
        if ((DataTable.edgeTable[cubeindex] & 64) == 64)
            vertlist[6].position =
               VertexInterp(isolevel, grid.vertices[6].position, grid.vertices[7].position, grid.val[6], grid.val[7], false);
        if ((DataTable.edgeTable[cubeindex] & 128) == 128)
            vertlist[7].position =
               VertexInterp(isolevel, grid.vertices[7].position, grid.vertices[4].position, grid.val[7], grid.val[4], false);
        if ((DataTable.edgeTable[cubeindex] & 256) == 256)
            vertlist[8].position =
               VertexInterp(isolevel, grid.vertices[0].position, grid.vertices[4].position, grid.val[0], grid.val[4], false);
        if ((DataTable.edgeTable[cubeindex] & 512) == 512)
            vertlist[9].position =
               VertexInterp(isolevel, grid.vertices[1].position, grid.vertices[5].position, grid.val[1], grid.val[5], false);
        if ((DataTable.edgeTable[cubeindex] & 1024) == 1024)
            vertlist[10].position =
               VertexInterp(isolevel, grid.vertices[2].position, grid.vertices[6].position, grid.val[2], grid.val[6], false);
        if ((DataTable.edgeTable[cubeindex] & 2048) == 2048)
            vertlist[11].position =
               VertexInterp(isolevel, grid.vertices[3].position, grid.vertices[7].position, grid.val[3], grid.val[7], false);

        TRIANGLE[] triangles = { new TRIANGLE(), new TRIANGLE(), new TRIANGLE() };

        /* Create the triangle */
        uint ntriang = 0;
        for (uint i = 0; DataTable.triTable[cubeindex, i] != -1; i += 3)
        {
            triangles[ntriang].vertices[0].position = vertlist[DataTable.triTable[cubeindex,i]].position;
            triangles[ntriang].vertices[1].position = vertlist[DataTable.triTable[cubeindex,i + 1]].position;
            triangles[ntriang].vertices[2].position = vertlist[DataTable.triTable[cubeindex,i + 2]].position;
            ntriang++;
        }

        return (ntriang);
    }

    Vector3 VertexInterp(float isolevel, Vector3 pos1, Vector3 pos2, float valuePos1, float valuePos2, bool interpolate)
    {
        float mu;
        XYZ point = new XYZ();
        
        if (Math.Abs(isolevel - valuePos1) < 0.00001)
            return (pos1);
        if (Math.Abs(isolevel - valuePos2) < 0.00001)
            return (pos2);
        if (Math.Abs(valuePos1 - valuePos2) < 0.00001)
            return (pos1);
        mu = (isolevel - valuePos1) / (valuePos2 - valuePos1);
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
