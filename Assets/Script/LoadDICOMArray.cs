using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using FellowOakDicom;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;

public class LoadDICOMArray : MonoBehaviour
{
    double[,,] DICOMArray;
    float xfactor;
    float yfactor;
    float zfactor;

    [SerializeField]
    private UnityEvent<double[,,], float, float, float> ArrayWithDetailsFound;

    public async void updateDICOMArray(string localPath)
    {
        //Can only use Find in the main thread
        //indicator = indicatorObject.GetComponent<IProgressIndicator>();
        //await indicator.OpenAsync();
        //indicator.Message = "Loading...";
        
        
        
        if ((localPath != null) & (Directory.GetFiles(localPath, "*.dcm").Length > 0))
        {
            string[] files = Directory.GetFiles(localPath, "*.dcm");
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
            DICOMArray = await Task.Run(() => getDICOMArray(localPath, true));


        } 
        else if ((localPath != null) & (Directory.GetFiles(localPath).Length > 0))
        {
            string[] files = Directory.GetFiles(localPath);
            List<string> fileList = new List<string>(files);
            fileList.Sort();

            try 
            {
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
                
            }
            catch
            {
                Debug.Log("File Can't be read by FO DICOM");
            }

            DICOMArray = await Task.Run(() => getDICOMArray(localPath, false));

        } 
        else
        {
            Debug.Log("Fatal Error: No found files");
        }

        Debug.Log(DICOMArray);
        Debug.Log(xfactor);
        Debug.Log(yfactor);
        Debug.Log(zfactor);

        ArrayWithDetailsFound.Invoke(DICOMArray, xfactor, yfactor, zfactor);

        //await indicator.CloseAsync();

    }

    private double[,,] getDICOMArray(string path, bool fileExtension)
    {
        string[] files;

        if (fileExtension)
        {
            files = Directory.GetFiles(path, "*.dcm");
        } 
        else
        {
            files = Directory.GetFiles(path);
        }
        

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

}
