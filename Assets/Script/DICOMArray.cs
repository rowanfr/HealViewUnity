using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EvilDICOM.Core;

public class DICOMArray : MonoBehaviour
{
    public T[,,] DicomMatrixGenerater<T>(uint xsize, uint ysize, uint zsize)
    {
        return new T[xsize, ysize, zsize];
    }
    public dynamic ListTypeDetermination(int n, bool signed)
    {
        switch (n)
        {
            case 1:
                return new List<bool>();
                break;
            case 8:
                if (signed)
                {
                    return new List<sbyte>();
                }
                else
                {
                    return new List<byte>();
                }
                break;
            case 16:
                if (signed)
                {
                    return new List<short>();
                }
                else
                {
                    return new List<ushort>();
                }
                break;
            case 32:
                if (signed)
                {
                    return new List<int>();
                }
                else
                {
                    return new List<uint>();
                }
                break;
            case 64:
                if (signed)
                {
                    return new List<long>();
                }
                else
                {
                    return new List<ulong>();
                }
                break;
            default:
                throw new System.ArgumentException("Either no type or invalid type. Check (0028,0100) and (0028,0103)");
                break;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        

        string path = @"G:\Users\beanf\Documents\DICOM\APOLLO\manifest-1628651393963\APOLLO-5-LSCC\AP-6H6G\10-26-2014-NA-PETCT SKULL-MIDTHIGH-30467\2.000000-CTAC-47844";
        
        string[] files = Directory.GetFiles(path, "*.dcm");
        List<string> fileList = new List<string>(files);
        fileList.Sort();
        
        //var is implicitly typed
        var dcm = DICOMObject.Read(fileList[0]);
        var sel = dcm.GetSelector();
            
        for (int n = 0; n < dcm.AllElements.Count; n++)
        {
            Debug.Log(dcm.AllElements[n]);
        }
        Debug.Log(sel.BitsAllocated.Data);

        //One has to use an IList as an interface for list in order to be able to access list functions without defining the type of the list yet
        IList DicomDataset = ListTypeDetermination(sel.BitsAllocated.Data, System.Convert.ToBoolean(sel.PixelRepresentation.Data));

        switch (sel.BitsAllocated.Data)
        {
            case 1:
                break;
            case 8:
                Debug.Log(dcm.PixelStream.GetBuffer().Length);
                Debug.Log(dcm.PixelStream.GetBuffer().GetValue(1));
                break;
            case 16:
                {
                }  
                break;
            case 32:
                Debug.Log(dcm.PixelStream.GetValues32().Length);
                Debug.Log(dcm.PixelStream.GetValues32().GetValue(1));
                    
                break;
            case 64:
                Debug.Log(dcm.PixelStream.GetValues64().Length);
                Debug.Log(dcm.PixelStream.GetValues64().GetValue(1));
                break;
            default:
            Debug.Log("Error with Bit Allocation Determination, (0028,0100)");
            break;
        }
        //var pixelSlicer = EvilDICOM.Core.Image.PixelSlicer.GetSlice<double>
        Debug.Log(DicomDataset.Count);


    }

    // Update is called once per frame
}