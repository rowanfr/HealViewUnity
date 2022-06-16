using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;


#if WINDOWS_UWP
using System;
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
#endif
namespace FileBrowser
{
    //Must attach script to unity object to enable function to be called as event
    public class InternalFileBrowser : MonoBehaviour
    {
        public string getFolderBrowser()
        {
#if UNITY_EDITOR || !UNITY_WSA_10_0
            string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");



            //UNITY_WSA_10_0 	Scripting symbol for Universal Windows Platform. Additionally WINDOWS_UWP is defined when compiling C# files against .NET Core.
#elif !UNITY_EDITOR && UNITY_WSA_10_0
        

        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            UnityEngine.WSA.Application.InvokeOnAppThread(() => 
            {

                path = folder.Path;

            }, false);
        }, false);

        
        
#endif
            path = path.Replace('/', '\\');
            if ((path != null) && (path != ""))
            {
                return path;
            }
            return null;

        }

        public string getFileBrowser(string[] extensions)
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Select File", "", extensions);

            path = path.Replace('/', '\\');
            if ((path != null) && (path != ""))
            {
                return path;
            }
            return null;

        }


    }
}

/*
 * //UNITY_WSA_10_0 	Scripting symbol for Universal Windows Platform. Additionally WINDOWS_UWP is defined when compiling C# files against .NET Core.
#if !UNITY_EDITOR && UNITY_WSA_10_0
        //=> is lambda expression to make method with input. For InvokeOnUIThread we are passing in an asynchronous operation with no input. 
        //Additionally this particular function comes from "WSA" or windows subsystem apps from unity. This means that it is using AppCallback class to abide by windows requirements here: https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh994635(v=win.10)?redirectedfrom=MSDN
        //We are assigning the file browser to the UI as it is the current UI element that we want users to interact with in a responsive manner
        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            //FolderPicker functions are here: https://docs.microsoft.com/en-us/uwp/api/Windows.Storage.Pickers.FolderPicker?view=winrt-22000
            var folderpicker = new FolderPicker();
            folderpicker.FileTypeFilter.Add("");


            //PickSingleFolderAsync returns storage folder obj
            var folder = await folderpicker.PickSingleFolderAsync();
            path = folder.Path;
        }, false);
#endif
        Debug.Log(path);
        if (path != null)
        {
            PathFound.Invoke();
        }
    }


Also look into: https://docs.microsoft.com/en-us/windows/mixed-reality/develop/native/holographic-remoting-overview
https://www.youtube.com/watch?v=cB_ZpXx_sqo
https://www.youtube.com/watch?v=OVvCI5KCndw
https://www.youtube.com/watch?v=MWjVQN4y_oM
 */