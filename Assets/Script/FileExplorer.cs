/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using System;
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
#endif

public class OpenFilePicker : MonoBehaviour
{

    public void OpenFile()
    {
        FilePicker();
    }

    private void FilePicker()
    {
#if !UNITY_EDITOR && UNITY_WSA_10_0

        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            var DICOMpicker = new FolderPicker();
            DICOMpicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            DICOMpicker.FileTypeFilter.Add(".dcm");

            var folder = await filepicker.PickSingleFolderAsync();
            UnityEngine.WSA.Application.InvokeOnAppThread(() => 
            {
                // do something with file

            }, false);
        }, false);

#endif
    }
}*/