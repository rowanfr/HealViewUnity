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
using UnityMeshImporter;

using Microsoft.MixedReality.Toolkit.UI.BoundsControl;//Provides BoundsControl
using Microsoft.MixedReality.Toolkit.Input;//NearInteractionGrabbable

using FileBrowser;

public class LoadDICOMArray : MonoBehaviour
{
    ProgressIndicatorLoadingBar indicator;
    GameObject LoadingIndicator;
    GameObject NonDicomObjects;
    DICOMImporter importer;

    [SerializeField]
    private GameObject NonDicomObject;

    [SerializeField]
    //private UnityEvent<double[,,], float, float, float> ArrayWithDetailsFound;
    private UnityEvent fileProcessed;
    

    FileLoadResult currentFileLoadResult = FileLoadResult.Unknown;

    public enum FileLoadResult
    {
        DICOM,
        Import,
        Unknown
    }

    public void Start()
    {
        //As LoadingBar is currently not active we must use the Find method
        LoadingIndicator = GameObject.Find("LoadingBar");
        NonDicomObjects = GameObject.Find("NonDicomObjects");
        //The setActive false script will be done on all start entities that rely on this loading bar.
        //In effect in order for later references to work and for this not to be automatically garbage collected we need to set it active initially and then deactive with references to it so it doesn't deallocate due to the garbage collector
        LoadingIndicator.SetActive(false);

    }

    public void loadMeshFromFolder()
    {
        string path = new InternalFileBrowser().getFolderBrowser();
        if(path != null)
        {
            unityImportObjects(path);
        }
    }

    public async void unityImportObjects(string localPath)
    {
        LoadingIndicator.SetActive(true);

        //For some reason I can't get the component unless the gameobject is loaded so I set the indicator value here
        indicator = LoadingIndicator.GetComponent<ProgressIndicatorLoadingBar>();
        Debug.Log(indicator);
        //Opens loading bar
        await indicator.OpenAsync();
        

        
        indicator.Progress = 0f;
        indicator.Message = "Loading: Getting File List";
        List<string> fileList = await Task.Run(() => getFileList(localPath));
        LoadingIndicator.SetActive(true);
        switch (currentFileLoadResult)
        {
            case FileLoadResult.DICOM:
                {
                    Debug.Log("Loading DICOM Files");
                    loadDicom(fileList);
                    break;
                }
            case FileLoadResult.Import:
                {
                    Debug.Log("Loading DICOM Files");
                    loadMesh(fileList);
                    break;
                }
            case FileLoadResult.Unknown:
                {
                    Debug.Log("Loading Unknown Files as DICOM");
                    try
                    {
                        loadDicom(fileList);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                        Debug.Log("Incapable of parsing data in folder provided");
                    }
                    break;
                }
            default:
                {
                    try
                    {
                        Debug.Log("Default Reached: Loading Unknown Files as DICOM");
                        loadDicom(fileList);
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.Message);
                        Debug.Log("Incapable of parsing data in folder provided");
                    }
                    break;
                }
        }
        //!! Warnign, code here executes before functions in switch statement are complete. If wanting to add any further closing code put that in the closeLoading function which is appended to the end of the other loading functions
        
        
    }

    //Closes loading bar
    private async void closeLoading()
    {
        Debug.Log("Complete Operation");
        await indicator.CloseAsync();
        LoadingIndicator.SetActive(false);
        fileProcessed.Invoke();
    }

    private async void loadMesh(List<string> fileList)
    {
        for(int n = 0; n < fileList.Count(); n++)
        {
            Debug.Log("Got to beginning of obj import");
            Debug.Log(fileList[n]);
            GameObject obj = MeshImporter.Load(fileList[n]);
            Debug.Log(obj);

            obj.AddComponent<BoxCollider>();
            obj.AddComponent<ConstraintManager>();
            obj.AddComponent<BoundsControl>();
            obj.AddComponent<NearInteractionGrabbable>();
            obj.AddComponent<ObjectManipulator>();
            obj.transform.SetParent(NonDicomObjects.transform);

            //Instantiate(obj, indicator.transform.position, new Quaternion(0,0,0,0), NonDicomObject.transform);
            Debug.Log("After Instatiate");
        }
        closeLoading();

    }

    private async void loadDicom(List<string> fileList)
    {
        //await Task.Run(() => OnOpenDICOMDatasetResult(localPath));
        VolumeDataset blankDataset = new VolumeDataset();
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
            for (int n = 0; n < allDatasets.Length; n++)
            {
                var dimX = allDatasets[n].dimX;
                var dimY = allDatasets[n].dimY;
                var dimZ = allDatasets[n].dimZ;
                TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
                Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                bool isHalfFloat = texformat == TextureFormat.RHalf;
                indicator.Message = "Loading Model " + (n + 1).ToString() + ": Volume Texture";
                allDatasets[n].setIndicator(indicator);
                byte[] textureByteArray = await Task.Run(() => allDatasets[n].CreateTextureByteArray(isHalfFloat));
                texture.SetPixelData(textureByteArray, 0);
                texture.Apply();

                VolumeRenderedObject obj = VolumeObjectFactory.CreateObjectWTexture(allDatasets[n], texture);
                //This is where it places the object
                obj.transform.position = indicator.transform.position;
            }
        }
        closeLoading();
    }

    private List<string> getFileList(string localPath)
    {
        List<string> fileList = new List<string>();

        string[] filteredDicomFiles = Directory.GetFiles(localPath, "*.*").Where(file => file.ToLower().EndsWith(".dcm") || file.ToLower().EndsWith(".dic") || file.ToLower().EndsWith(".dicom")).ToArray<string>();
        string[] filtered3DModelFiles = Directory.GetFiles(localPath, "*.*").Where(file => file.ToLower().EndsWith(".obj")).ToArray<string>();


        //This is checking if the localPath contains files that end in .dcm. It presumes all .dcm files are DICOM files
        if (filteredDicomFiles.Length > 0)
        {
            indicator.Progress = 0.5f;
            fileList.AddRange(filteredDicomFiles);
            currentFileLoadResult = FileLoadResult.DICOM;
        }
        //If their aren't any .dcm files then this presumes they are all DICOM files
        else if (filtered3DModelFiles.Length > 0)
        {
            indicator.Progress = 0.5f;
            fileList.AddRange(filtered3DModelFiles);
            currentFileLoadResult = FileLoadResult.Import;
        }
        else
        {
            string[] files = Directory.GetFiles(localPath);
            indicator.Progress = 0.5f;
            fileList.AddRange(files);
            currentFileLoadResult = FileLoadResult.Unknown;
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



}
