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

public class FileBrowser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR && UNITY_WSA_10_0
        Debug.Log("***********************************");
        Debug.Log("File Picker start.");
        Debug.Log("***********************************");

        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            var filepicker = new FileOpenPicker();
            // filepicker.FileTypeFilter.Add("*");
            filepicker.FileTypeFilter.Add(".txt");

            var file = await filepicker.PickSingleFileAsync();
            UnityEngine.WSA.Application.InvokeOnAppThread(() => 
            {
                Debug.Log("***********************************");
                string name = (file != null) ? file.Name : "No data";
                Debug.Log("Name: " + name);
                Debug.Log("***********************************");
                string path = (file != null) ? file.Path : "No data";
                Debug.Log("Path: " + path);
                Debug.Log("***********************************");

                

                //This section of code reads through the file (and is covered in the link)
                // but if you want to make your own parcing function you can 
                // ReadTextFile(path);
                //StartCoroutine(ReadTextFileCoroutine(path));

            }, false);
        }, false);

        
        Debug.Log("***********************************");
        Debug.Log("File Picker end.");
        Debug.Log("***********************************");
#endif

    }

    // Update is called once per frame
    void Update()
    {

    }
}
