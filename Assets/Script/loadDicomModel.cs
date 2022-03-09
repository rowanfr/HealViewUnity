/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FellowOakDicom;



public class loadDicomModel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        //@string is verbatum string meaning that no characters can be escaped
        //This results in generic dicomFile type
        var dicomFile = DicomFile.Open(@"G:\Users\beanf\Documents\DICOM\APOLLO\manifest-1628651393963\APOLLO-5-LSCC\AP-6H6G\10-26-2014-NA-PETCT SKULL-MIDTHIGH-30467\2.000000-CTAC-47844\1-016.dcm", FileReadOption.ReadAll);
        var dataset = dicomFile.Dataset;
        var dataimage = FellowOakDicom.Imaging.DicomPixelData.Create(dataset);
        var dicomImage = new FellowOakDicom.Imaging.DicomImage(dataset);
        //dataimage, int32 frame
        var pixelData = FellowOakDicom.Imaging.Render.PixelDataFactory.Create(dataimage, 0);
        Debug.Log(dataset.GetValues<string>(DicomTag.PixelSpacing)[0]);
        //Why must I find DicomDatasetWalker is a random forum post. Please update API
        var datasetWalker = new DicomDatasetWalker(dataset);
        
        Debug.Log(pixelData.Components);
        Debug.Log(dicomImage.NumberOfFrames);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/