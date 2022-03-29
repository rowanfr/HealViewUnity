using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
//For UnityEvents
using UnityEngine.Events;
using FellowOakDicom;
using System.Threading.Tasks;

using Microsoft.MixedReality.Toolkit.UI;



public class loadDicomModel : MonoBehaviour
{

    private Mesh mesh;
    private GameObject selfGameObj;
    private Vector3[] globalVertices;
    private int[] globalTriangle;
    MeshCollider objCollider;
    bool meshFinishRendering = false;

    [SerializeField]
    private GameObject indicatorObject;
    private IProgressIndicator indicator;

    [SerializeField] private UnityEvent DICOMArrayHasValue;
    // Start is called before the first frame update

    [SerializeField] private Material material;
    //This is the material to be referenced
    private void Start()
    {
        Debug.Log("Task Started");
        selfGameObj = GameObject.Find("DICOMMesh");
        Debug.Log("Task Mid");
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        objCollider = selfGameObj.GetComponent<MeshCollider>();
    }

    public async void updateDICOMFolderPath(string localPath)
    {
        //Can only use Find in the main thread
        //indicator = indicatorObject.GetComponent<IProgressIndicator>();
        //await indicator.OpenAsync();
        //indicator.Message = "Loading...";
        Debug.Log(localPath);
        double[,,] DICOMArray;
        string[] files = Directory.GetFiles(localPath, "*.dcm");
        float xfactor;
        float yfactor;
        float zfactor;
        if ((localPath != null) & (files.Length > 0))
        {
            List<string> fileList = new List<string>(files);
            fileList.Sort();

            var dicomFile = DicomFile.Open(fileList[0], FileReadOption.ReadAll);

            var dataset = dicomFile.Dataset;
            var dataimage = FellowOakDicom.Imaging.DicomPixelData.Create(dataset);
            var dicomImage = new FellowOakDicom.Imaging.DicomImage(dataset);
            //dataimage, int32 frame
            var pixelData = FellowOakDicom.Imaging.Render.PixelDataFactory.Create(dataimage, 0);
            xfactor = float.Parse(dataset.GetValues<string>(DicomTag.PixelSpacing)[0]);
            yfactor = float.Parse(dataset.GetValues<string>(DicomTag.PixelSpacing)[1]);
            zfactor = float.Parse(dataset.GetValues<string>(DicomTag.SliceThickness)[0]);

            //Task.Run performs the task asynchronously in the background on the app thread so that it doesn't effect the UI thread
            DICOMArray = await Task.Run(() => getDICOMArray(localPath));

            if (DICOMArray != null)
            {
                DICOMArrayHasValue.Invoke();
                Debug.Log(xfactor);
                Debug.Log(yfactor);
                Debug.Log(zfactor);
                //factors are divided by 1000 as they are given in mm but unity operates on m
                await Task.Run(() => DICOMMeshFunction(DICOMArray, 500d, xfactor/1000, yfactor / 1000, zfactor / 1000));
            }

            

            //

            mesh.vertices = globalVertices;
            mesh.triangles = globalTriangle;



            objCollider.sharedMesh = mesh;
            meshFinishRendering = true;
        }
        //await indicator.CloseAsync();

    }

    private double[,,] getDICOMArray(string path)
    {
        
        

        string[] files = Directory.GetFiles(path, "*.dcm");

        if ((path != null) & (files.Length > 0))
        {
            //@string is verbatum string meaning that no characters can be escaped
            //This results in generic dicomFile type

            

            List<string> fileList = new List<string>(files);
            fileList.Sort();

            var dicomFile = DicomFile.Open(fileList[0], FileReadOption.ReadAll);

            var dataset = dicomFile.Dataset;
            var dataimage = FellowOakDicom.Imaging.DicomPixelData.Create(dataset);
            var dicomImage = new FellowOakDicom.Imaging.DicomImage(dataset);
            //dataimage, int32 frame
            var pixelData = FellowOakDicom.Imaging.Render.PixelDataFactory.Create(dataimage, 0);
            int xNumValues = pixelData.Width;
            int yNumValues = pixelData.Height;
            int zNumValues = fileList.Count;

            Debug.Log(dataset.GetValues<string>(DicomTag.PixelSpacing)[0]);
            //Why must I find DicomDatasetWalker is a random forum post. Please update API

            double[,,] Dicom3DArray = new double[xNumValues, yNumValues, zNumValues];
            for (int z = 0; z < zNumValues; z++)
            {
                //indicator.Progress = (z / zNumValues);
                //Debug.Log(indicator.State);
                var currentDataset = DicomFile.Open(fileList[z], FileReadOption.ReadAll).Dataset;
                var currentImage = FellowOakDicom.Imaging.DicomPixelData.Create(currentDataset);
                var currentPixelData = FellowOakDicom.Imaging.Render.PixelDataFactory.Create(currentImage, 0);

                for (int y = 0; y < yNumValues; y++)
                {
                    for (int x = 0; x < xNumValues; x++)
                    {
                        Dicom3DArray[x, y, z] = currentPixelData.GetPixel(x, y);
                    }
                }
            }


            

            return Dicom3DArray;


        }

        return null;
    }

    ComputeBuffer marchingCube;

    public struct GRIDCELL
    {
        public Vector3[] vertices;//8 array values for cube verts, x,y,z in each of those values
        public double[] val;//8 val for
    }




    //Isolevel is cutoff point for 


    MarchingCubeTables DataTable = new MarchingCubeTables();

    GRIDCELL makeGrid(double[,,] examinedMatrix, int x, int y, int z, float xfactor, float yfactor, float zfactor)
    {
        GRIDCELL grid = new GRIDCELL();
        //Must call new here as we are dynamically declaring space for array and the compiler doesn't infer that from setting it equal to an array

        grid.val = new double[8] {
            examinedMatrix[x, y, z], //0 point
            examinedMatrix[(x + 1), y, z], //1 x point
            examinedMatrix[(x + 1), (y + 1), z], //1 y and 1 x point
            examinedMatrix[x, (y + 1), z], //1 y point
            examinedMatrix[x, y, (z + 1)], //1 z point
            examinedMatrix[(x + 1), y, (z + 1)], //1 x and 1 z
            examinedMatrix[(x + 1), (y + 1), (z + 1)],// 1 x,y, and z
            examinedMatrix[x, y + 1, (z + 1)]//1 x and 1 y
        };

        //These are all multiplied by their factores with respect to the given axis due to these being the source of positional data. With this the meashes should be accurate
        grid.vertices = new Vector3[8];
        grid.vertices[0] = new Vector3((x * xfactor), (y * yfactor), (z * zfactor));
        grid.vertices[1] = new Vector3(((x + 1) * xfactor), (y * yfactor), (z * zfactor));
        grid.vertices[2] = new Vector3(((x + 1) * xfactor), ((y + 1) * yfactor), (z * zfactor));
        grid.vertices[3] = new Vector3((x * xfactor), ((y + 1) * yfactor), (z * zfactor));
        grid.vertices[4] = new Vector3((x * xfactor), (y * yfactor), ((z + 1) * zfactor));
        grid.vertices[5] = new Vector3(((x + 1) * xfactor), (y * yfactor), ((z + 1) * zfactor));
        grid.vertices[6] = new Vector3(((x + 1) * xfactor), ((y + 1) * yfactor), ((z + 1) * zfactor));
        grid.vertices[7] = new Vector3((x * xfactor), ((y + 1) * yfactor), ((z + 1) * zfactor));

        return grid;
    }


    void DICOMMeshFunction(double[,,] DICOMArray, double isolevel, float xfactor, float yfactor, float zfactor)
    {
        Debug.Log("Task Started");
        //Using find at this point in this script fails

        List<Vector3> vertices = new List<Vector3>();
        //When calling compute buffer one must input the number of elements of the buffer
        //DICOMArray.Length is all elements and 8 corresponds to the number of bytes in a double
        //marchingCube = new ComputeBuffer(DICOMArray.Length, 8);
        Debug.Log("Task End");
        for (int z = 0; z < (DICOMArray.GetLength(2) - 1); z++)
        {
            Debug.Log(((float)z) / ((float)DICOMArray.GetLength(2)));
            for (int y = 0; y < (DICOMArray.GetLength(1) - 1); y++)
            {
                
                for (int x = 0; x < (DICOMArray.GetLength(0) - 1); x++)
                {
                    
                    GRIDCELL grid = makeGrid(DICOMArray, x, y, z, xfactor, yfactor, zfactor);

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


                    /* Find the vertices where the surface intersects the cube */

                    Vector3[] vertlist = new Vector3[12];
                    //One inputs the grid components into a vertexInterp function to get an actual vertex point to make the triangles later on
                    //Consider vertexList as being the edges of the current scanning grid and the output of VertexInterp being the position on that edge
                    //For legibility we can use (DataTable.edgeTable[cubeindex] & n) != 0  or (DataTable.edgeTable[cubeindex] & n) == n as both are testing the same thing
                    //Why is C# like this, one has to set == to a value instead of leaving if(Type)
                    if ((DataTable.edgeTable[cubeindex] & 1) != 0)
                        vertlist[0] =
                           VertexInterp(isolevel, grid.vertices[0], grid.vertices[1], grid.val[0], grid.val[1]);
                    if ((DataTable.edgeTable[cubeindex] & 2) != 0)
                        vertlist[1] =
                           VertexInterp(isolevel, grid.vertices[1], grid.vertices[2], grid.val[1], grid.val[2]);
                    if ((DataTable.edgeTable[cubeindex] & 4) != 0)
                        vertlist[2] =
                           VertexInterp(isolevel, grid.vertices[2], grid.vertices[3], grid.val[2], grid.val[3]);
                    if ((DataTable.edgeTable[cubeindex] & 8) != 0)
                        vertlist[3] =
                           VertexInterp(isolevel, grid.vertices[3], grid.vertices[0], grid.val[3], grid.val[0]);
                    if ((DataTable.edgeTable[cubeindex] & 16) != 0)
                        vertlist[4] =
                           VertexInterp(isolevel, grid.vertices[4], grid.vertices[5], grid.val[4], grid.val[5]);
                    if ((DataTable.edgeTable[cubeindex] & 32) != 0)
                        vertlist[5] =
                           VertexInterp(isolevel, grid.vertices[5], grid.vertices[6], grid.val[5], grid.val[6]);
                    if ((DataTable.edgeTable[cubeindex] & 64) != 0)
                        vertlist[6] =
                           VertexInterp(isolevel, grid.vertices[6], grid.vertices[7], grid.val[6], grid.val[7]);
                    if ((DataTable.edgeTable[cubeindex] & 128) != 0)
                        vertlist[7] =
                           VertexInterp(isolevel, grid.vertices[7], grid.vertices[4], grid.val[7], grid.val[4]);
                    if ((DataTable.edgeTable[cubeindex] & 256) != 0)
                        vertlist[8] =
                           VertexInterp(isolevel, grid.vertices[0], grid.vertices[4], grid.val[0], grid.val[4]);
                    if ((DataTable.edgeTable[cubeindex] & 512) != 0)
                        vertlist[9] =
                           VertexInterp(isolevel, grid.vertices[1], grid.vertices[5], grid.val[1], grid.val[5]);
                    if ((DataTable.edgeTable[cubeindex] & 1024) != 0)
                        vertlist[10] =
                           VertexInterp(isolevel, grid.vertices[2], grid.vertices[6], grid.val[2], grid.val[6]);
                    if ((DataTable.edgeTable[cubeindex] & 2048) != 0)
                        vertlist[11] =
                           VertexInterp(isolevel, grid.vertices[3], grid.vertices[7], grid.val[3], grid.val[7]);

                    

                    for (uint i = 0; DataTable.triTable[cubeindex, i] != -1; i += 3)
                    {
                        vertices.Add(vertlist[DataTable.triTable[cubeindex, i]]);
                        vertices.Add(vertlist[DataTable.triTable[cubeindex, i + 1]]);
                        vertices.Add(vertlist[DataTable.triTable[cubeindex, i + 2]]);

                    }
                }

            }

        }

        Debug.Log("Partial Finish");
        Debug.Log(vertices.Count);
        List<int> triangles = new List<int>();
        for (int n = 0; n < vertices.Count; n++)
        {
            triangles.Add(n);
        }
        globalTriangle = triangles.ToArray();
        globalVertices = vertices.ToArray();
        Debug.Log("Finish");
    }

    Vector3 VertexInterp(double isolevel, Vector3 pos1, Vector3 pos2, double valuePos1, double valuePos2)
    {
        double mu;
        Vector3 point = new Vector3();
        if (Math.Abs(valuePos1 - valuePos2) < 0.00001)
            return ((pos1 + pos2) / 2);

        if (Math.Abs(isolevel - valuePos1) < 0.00001)
            return (pos1);

        if (Math.Abs(isolevel - valuePos2) < 0.00001)
            return (pos2);


        mu = (isolevel - valuePos1) / (valuePos2 - valuePos1);
        
        point.x = (float)(pos1.x + mu * (pos2.x - pos1.x));
        point.y = (float)(pos1.y + mu * (pos2.y - pos1.y));
        point.z = (float)(pos1.z + mu * (pos2.z - pos1.z));

        return point;
    }

    // Update is called once per frame
    void DICOMMeshFunctionEnd()
    {
        marchingCube.Release();
        marchingCube = null;
    }
    public void Update()
    {
        // will make the mesh appear in the Scene at origin position
        if ( meshFinishRendering )
        {
            Debug.Log("ReachedRender");
            //This function renders for only 1 frame so it needs to be repeated with every update and update its location to the game object
            Graphics.DrawMesh(mesh, selfGameObj.transform.position, selfGameObj.transform.rotation, material, 0);
        }
        
    }

}