using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

#if WINDOWS_UWP
using System;
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
#endif

//Must attach script to unity object to enable function to be called as event
public class FileBrowser : MonoBehaviour
{
    public string path;
    

    private void getFolder()
    {
#if UNITY_EDITOR
        path = EditorUtility.OpenFolderPanel("Select DICOM Folder", "", "");
#endif
        //UNITY_WSA_10_0 	Scripting symbol for Universal Windows Platform. Additionally WINDOWS_UWP is defined when compiling C# files against .NET Core.
#if !UNITY_EDITOR && UNITY_WSA_10_0
        //=> is lambda expression to make method with input. For InvokeOnUIThread we are passing in an asynchronous operation with no input. 
        //Additionally this particular function comes from "WSA" or windows subsystem apps from unity. This means that it is using AppCallback class to abide by windows requirements here: https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh994635(v=win.10)?redirectedfrom=MSDN
        //We are assigning the file browser to the UI as it is the current UI element that we want users to interact with in a responsive manner
        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            //FolderPicker functions are here: https://docs.microsoft.com/en-us/uwp/api/Windows.Storage.Pickers.FolderPicker?view=winrt-22000
            var folderpicker = new FolderPicker();
            folderpicker.FileTypeFilter.Add("*.dcm");


            //PickSingleFolderAsync returns storage folder obj
            var folder = await folderpicker.PickSingleFolderAsync();
            path = folder.Path;
        }, false);
#endif
        Debug.Log(path);
    }
}
