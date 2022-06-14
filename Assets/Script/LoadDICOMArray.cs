using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using FellowOakDicom;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;
using UnityVolumeRendering;//Namespace for VolumeRenderedObject class
using System.Linq;

public class LoadDICOMArray : MonoBehaviour
{
    double[,,] DICOMArray;
    float xfactor;
    float yfactor;
    float zfactor;
    ProgressIndicatorLoadingBar indicator;
    GameObject LoadingIndicator;
    DICOMImporter importer;


    [SerializeField]
    //private UnityEvent<double[,,], float, float, float> ArrayWithDetailsFound;
    private UnityEvent fileProcessed;

    public void Start()
    {
        //As LoadingBar is currently not active we must use the Find method
        LoadingIndicator = GameObject.Find("LoadingBar");
        //The setActive false script will be done on all start entities that rely on this loading bar.
        //In effect in order for later references to work and for this not to be automatically garbage collected we need to set it active initially and then deactive with references to it so it doesn't deallocate due to the garbage collector
        LoadingIndicator.SetActive(false);

    }

    public async void unityVolumeRendering(string localPath)
    {
        LoadingIndicator.SetActive(true);

        //For some reason I can't get the component unless the gameobject is loaded so I set the indicator value here
        indicator = LoadingIndicator.GetComponent<ProgressIndicatorLoadingBar>();

        //Opens loading bar
        await indicator.OpenAsync();
        

        //await Task.Run(() => OnOpenDICOMDatasetResult(localPath));
        VolumeDataset blankDataset = new VolumeDataset();
        indicator.Progress = 0f;
        indicator.Message = "Loading: Getting DICOM File List";
        List<string> fileList = await Task.Run(() => getFileList(localPath));
        indicator.Progress = 0f;
        indicator.Message = "Loading: Convert Files to DICOM Slices";
        IImageSequenceSeries[] seriesList = await Task.Run(() => getSeriesList(fileList));
        indicator.Progress = 0f;
        indicator.Message = "Loading: Assembling DICOM File Slices";
        VolumeDataset[] allDatasets = await Task.Run(() => OnOpenDICOMDatasetResult(seriesList, blankDataset));
        indicator.Progress = 0f;
        indicator.Message = "Loading";
        // Spawn the object
        if (allDatasets != null)
        {
            for(int n = 0; n < allDatasets.Length; n++)
            {
                var dimX = allDatasets[n].dimX;
                var dimY = allDatasets[n].dimY;
                var dimZ = allDatasets[n].dimZ;
                TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
                Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                bool isHalfFloat = texformat == TextureFormat.RHalf;
                indicator.Message = "Loading Model " + (n + 1).ToString() +  ": Volume Texture";
                allDatasets[n].setIndicator(indicator);
                byte[] textureByteArray = await Task.Run(() => allDatasets[n].CreateTextureByteArray(isHalfFloat));
                texture.SetPixelData(textureByteArray, 0);
                texture.Apply();
                
                VolumeRenderedObject obj = VolumeObjectFactory.CreateObjectWTexture(allDatasets[n], texture);
                //This is where it places the object
                obj.transform.position = indicator.transform.position;
            }
        }

        //Closes loading bar
        await indicator.CloseAsync();
        LoadingIndicator.SetActive(false);
        fileProcessed.Invoke();
    }
    private List<string> getFileList(string localPath)
    {
        List<string> fileList = new List<string>();
        //This is checking if the localPath contains files that end in .dcm. It presumes all .dcm files are DICOM files
        if (Directory.GetFiles(localPath, "*.dcm").Length > 0)
        {
            string[] files = Directory.GetFiles(localPath, "*.dcm");
            indicator.Progress = 0.5f;
            fileList.AddRange(files);
        }
        //If their aren't any .dcm files then this presumes they are all DICOM files
        else
        {
            string[] files = Directory.GetFiles(localPath);
            indicator.Progress = 0.5f;
            fileList.AddRange(files);
        }
        indicator.Progress = 0.75f;
        //This organizes the file list as GetFiles is unordered. This also means that the sort function is the source of order
        fileList.Sort();
        indicator.Progress = 1f;
        return fileList;
    }

    private IImageSequenceSeries[] getSeriesList(List<string> fileList)
    {
        importer = new DICOMImporter();//Returns DICOM importer
        importer.setIndicator(indicator);
        IImageSequenceSeries[] seriesList = importer.LoadSeries(fileList).ToArray();//DicomImporter/LoadSeries
        return seriesList;
        
    }

    private VolumeDataset[] OnOpenDICOMDatasetResult(IImageSequenceSeries[] seriesList, VolumeDataset blankDataset)
    {
        Debug.Log("Got to beginning of loading Dataset from file");
        //This is checking if the fileList contains files. If it does it tries to process them.
        if (seriesList != null)
        {
            VolumeDataset[] allDatasets = new VolumeDataset[seriesList.Length];

            for (int n = 0; n < seriesList.Length; n++)
            {
                indicator.Progress = (float)n / (float) seriesList.Length;
                Debug.Log(n);

                allDatasets[n] = importer.ImportSeries(seriesList[n], blankDataset);//DicomImporter/ImportSeries as VolumeDataset
                
            }
            return allDatasets;
        }
        else
        {
            return null;
        }
    }

    private void DespawnAllDatasets()
    {
        VolumeRenderedObject[] volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
        foreach (VolumeRenderedObject volobj in volobjs)
        {
            GameObject.Destroy(volobj.gameObject);
        }
    }


    //This is the start point for the loading of the DICOM Array
    public async void updateDICOMArray(string localPath)
    {
        //Can only use Find in the main thread

        LoadingIndicator.SetActive(true);
        //For some reason I can't get the component unless the gameobject is loaded so I set the indicator value here
        indicator = LoadingIndicator.GetComponent<ProgressIndicatorLoadingBar>();

        //Opens loading bar
        await indicator.OpenAsync();
        indicator.Message = "Loading Array";

        //This is checking if the localPath contains files. If it does it tries to process them.
        if (localPath != null)
        {
            List<string> fileList = new List<string>();
            //This is checking if the localPath contains files that end in .dcm. It presumes all .dcm files are DICOM files
            if (Directory.GetFiles(localPath, "*.dcm").Length > 0)
            {
                string[] files = Directory.GetFiles(localPath, "*.dcm");
                fileList.AddRange(files);
            }
            //If their aren't any .dcm files then this presumes they are all DICOM files
            else
            {
                string[] files = Directory.GetFiles(localPath);
                fileList.AddRange(files);
            }
            //This organizes the file list as GetFiles is unordered. This also means that the sort function is the source of order
            fileList.Sort();
            //Due to some potential issues with the DICOM files from the sheer volume of the standard to corrupt files we implement a basic try catch system for checking if our library can handle the files.
            //Two major error with this method of error solving is that it's very rudementary and if expanded will quickly baloon. Second is we're only checking the first few files which can still allow for corrupt files and other problems in later files.
            try
            {
                //This is using the FO DICOM library to make a DICOM file from the first DICOM image
                var dicomFile = DicomFile.Open(fileList[0], FileReadOption.ReadAll);
                //We convert that file into a dataset
                var dataset = dicomFile.Dataset;

                //From the dataset we get these tags for x, y, and z spacing from the first file provided
                xfactor = float.Parse(dataset.GetValues<string>(DicomTag.PixelSpacing)[0]);
                yfactor = float.Parse(dataset.GetValues<string>(DicomTag.PixelSpacing)[1]);
                zfactor = float.Parse(dataset.GetValues<string>(DicomTag.SliceThickness)[0]);
            }
            catch
            {
                Debug.Log("Files in folder Can't be read by FO DICOM");
            }

            //Task.Run performs the task asynchronously in the background on the app thread so that it doesn't effect the UI thread
            DICOMArray = await Task.Run(() => getDICOMArray(localPath, true));


        }
        
        
        else
        {
            Debug.Log("Fatal Error: No found files");
        }

        Debug.Log(DICOMArray);
        Debug.Log(xfactor);
        Debug.Log(yfactor);
        Debug.Log(zfactor);

        //Closes loading bar
        await indicator.CloseAsync();
        LoadingIndicator.SetActive(false);

        //ArrayWithDetailsFound.Invoke(DICOMArray, xfactor, yfactor, zfactor);

        

    }
    //This is within an async event so that it doesn't freeze the screen
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
            //From our dataset we create a pixel dataset
            var dataimage = FellowOakDicom.Imaging.DicomPixelData.Create(dataset);
            //using the pixel dataset we create a single frame or layer of pixel data to find the x and y values in the array
            var pixelData = FellowOakDicom.Imaging.Render.PixelDataFactory.Create(dataimage, 0);
            int xNumValues = pixelData.Width;
            int yNumValues = pixelData.Height;

            //Using the number of files we determine the z value
            int zNumValues = fileList.Count;

            //This is the 3D array that will store the DICOM file. It is preallocated for speed
            double[,,] Dicom3DArray = new double[xNumValues, yNumValues, zNumValues];


            for (int z = 0; z < zNumValues; z++)
            {
                //This updates the loader as to the progress of loading the array files. We must convert to float for a division percentage
                indicator.Progress = ((float)z / (float)zNumValues);
                
                //Similar to before we create a pixel dataset for each image slice
                ////however instead of simply looking at the length and height of each we iterate through all values in the array and add them to our 3D double array
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
