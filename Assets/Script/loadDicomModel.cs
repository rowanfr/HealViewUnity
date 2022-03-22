using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using FellowOakDicom;
using System.Threading.Tasks;




public class loadDicomModel : MonoBehaviour
{
    MarchingCube MarchCubeRef = null;

    FileBrowser FileBrowserClassInstance = null;
    string localPath;
    // Start is called before the first frame update
    public async void updateMesh()
    {
        //Can only use Find in the main thread
        GameObject FileBrowserObj = GameObject.Find("FileBrowser");
        FileBrowserClassInstance = FileBrowserObj.GetComponent<FileBrowser>();
        string localPath = FileBrowserClassInstance.path;
        Debug.Log(localPath);
        double[,,] DICOMArray;
        //Task.Run performs the task asynchronously in the background on the app thread so that it doesn't effect the UI thread
        DICOMArray = await Task.Run(() => getDICOMArray(localPath));
        
        Debug.Log(DICOMArray[255, 255, 100]);

    }

    private double[,,] getDICOMArray(string path)
    {
        
        string[] files = Directory.GetFiles(path, "*.dcm");

        if ((path != null) & (files.Length > 0))
        {
            //@string is verbatum string meaning that no characters can be escaped
            //This results in generic dicomFile type

            //string path = @"G:\Users\beanf\Documents\DICOM\APOLLO\manifest-1628651393963\APOLLO-5-LSCC\AP-6H6G\10-26-2014-NA-PETCT SKULL-MIDTHIGH-30467\2.000000-CTAC-47844";

            List<string> fileList = new List<string>(files);
            fileList.Sort();

            var dicomFile = DicomFile.Open(fileList[0], FileReadOption.ReadAll);

            var dataset = dicomFile.Dataset;
            var dataimage = FellowOakDicom.Imaging.DicomPixelData.Create(dataset);
            var dicomImage = new FellowOakDicom.Imaging.DicomImage(dataset);
            //dataimage, int32 frame
            var pixelData = FellowOakDicom.Imaging.Render.PixelDataFactory.Create(dataimage, 0);
            Debug.Log(dataset.GetValues<string>(DicomTag.PixelSpacing)[0]);
            //Why must I find DicomDatasetWalker is a random forum post. Please update API

            double[,,] Dicom3DArray = new double[pixelData.Width, pixelData.Height, fileList.Count];
            for (int z = 0; z < fileList.Count; z++)
            {
                var currentDataset = DicomFile.Open(fileList[z], FileReadOption.ReadAll).Dataset;
                var currentImage = FellowOakDicom.Imaging.DicomPixelData.Create(currentDataset);
                var currentPixelData = FellowOakDicom.Imaging.Render.PixelDataFactory.Create(currentImage, 0);

                for (int y = 0; y < pixelData.Height; y++)
                {
                    for (int x = 0; x < pixelData.Width; x++)
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

    void Awake()
    {
        //When calling compute buffer one must input the number of elements of the buffer
        //marchingCube = new ComputeBuffer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}