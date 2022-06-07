using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;//Provides constrain Manager
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;//Provides BoundsControl
using Microsoft.MixedReality.Toolkit.Input;//NearInteractionGrabbable

namespace UnityVolumeRendering
{
    public class VolumeObjectFactory
    {
        public static VolumeRenderedObject CreateObject(VolumeDataset dataset)
        {
            Debug.Log("Check 1");
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            
            VolumeRenderedObject volObj = outerObject.AddComponent<VolumeRenderedObject>();
            outerObject.AddComponent<BoxCollider>();
            outerObject.AddComponent<ConstraintManager>();
            BoundsControl boundsSettings = outerObject.AddComponent<BoundsControl>();

            //boundsSettings.RotationHandlesConfig.ShowHandleForX = false;
            //boundsSettings.RotationHandlesConfig.ShowHandleForY = false;
            //boundsSettings.RotationHandlesConfig.ShowHandleForZ = false;


            NearInteractionGrabbable grabSettings = outerObject.AddComponent<NearInteractionGrabbable>();
            //grabSettings.ShowTetherWhenManipulating = true;

            outerObject.AddComponent<ObjectManipulator>();
            
            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            meshContainer.transform.parent = outerObject.transform;
            meshContainer.transform.localScale = Vector3.one;
            meshContainer.transform.localPosition = Vector3.zero;
            meshContainer.transform.parent = outerObject.transform;
            outerObject.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
            volObj.meshRenderer = meshRenderer;
            volObj.dataset = dataset;

            const int noiseDimX = 512;
            const int noiseDimY = 512;
            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            Texture2D tfTexture = tf.GetTexture();
            volObj.transferFunction = tf;
            Debug.Log("Check 2");
            TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();
            Debug.Log("Check 2.5");
            volObj.transferFunction2D = tf2D;

            Debug.Log("GetDataTexture is the problem in loading");
            meshRenderer.sharedMaterial.SetTexture("_DataTex", dataset.GetDataTexture());
            meshRenderer.sharedMaterial.SetTexture("_GradientTex", null);
            meshRenderer.sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
            meshRenderer.sharedMaterial.SetTexture("_TFTex", tfTexture);

            meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
            Debug.Log("Check 3");
            if (dataset.scaleX != 0.0f && dataset.scaleY != 0.0f && dataset.scaleZ != 0.0f)
            {
                //float maxScale = Mathf.Max(dataset.scaleX, dataset.scaleY, dataset.scaleZ);
                //Original obj scale is 1,1,1 m. scale multiplies by those values to get the accurate values for TOTAL SPACE in x, y, and z
                volObj.transform.localScale = new Vector3(dataset.scaleX, dataset.scaleY, dataset.scaleZ);//Changed this to not be / maxScale for each
            }

            return volObj;
        }

        public static VolumeRenderedObject CreateObjectWTexture(VolumeDataset dataset, Texture3D texture)
        {
            //The reason to pass in a texture is to use the GetDataTexture method asynchronously and simply pass in the result to this function
            Debug.Log("Check 1");
            GameObject outerObject = new GameObject("VolumeRenderedObject_" + dataset.datasetName);

            VolumeRenderedObject volObj = outerObject.AddComponent<VolumeRenderedObject>();
            outerObject.AddComponent<BoxCollider>();
            outerObject.AddComponent<ConstraintManager>();
            BoundsControl boundsSettings = outerObject.AddComponent<BoundsControl>();

            //boundsSettings.RotationHandlesConfig.ShowHandleForX = false;
            //boundsSettings.RotationHandlesConfig.ShowHandleForY = false;
            //boundsSettings.RotationHandlesConfig.ShowHandleForZ = false;


            NearInteractionGrabbable grabSettings = outerObject.AddComponent<NearInteractionGrabbable>();
            //grabSettings.ShowTetherWhenManipulating = true;

            outerObject.AddComponent<ObjectManipulator>();

            GameObject meshContainer = GameObject.Instantiate((GameObject)Resources.Load("VolumeContainer"));
            meshContainer.transform.parent = outerObject.transform;
            meshContainer.transform.localScale = Vector3.one;
            meshContainer.transform.localPosition = Vector3.zero;
            meshContainer.transform.parent = outerObject.transform;
            outerObject.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
            volObj.meshRenderer = meshRenderer;
            volObj.dataset = dataset;

            const int noiseDimX = 512;
            const int noiseDimY = 512;
            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            Texture2D tfTexture = tf.GetTexture();
            volObj.transferFunction = tf;
            Debug.Log("Check 2");
            TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();
            Debug.Log("Check 2.5");
            volObj.transferFunction2D = tf2D;

            Debug.Log("GetDataTexture is the problem in loading");
            meshRenderer.sharedMaterial.SetTexture("_DataTex", texture);
            meshRenderer.sharedMaterial.SetTexture("_GradientTex", null);
            meshRenderer.sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
            meshRenderer.sharedMaterial.SetTexture("_TFTex", tfTexture);

            meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
            Debug.Log("Check 3");
            if (dataset.scaleX != 0.0f && dataset.scaleY != 0.0f && dataset.scaleZ != 0.0f)
            {
                //float maxScale = Mathf.Max(dataset.scaleX, dataset.scaleY, dataset.scaleZ);
                //Original obj scale is 1,1,1 m. scale multiplies by those values to get the accurate values for TOTAL SPACE in x, y, and z
                volObj.transform.localScale = new Vector3(dataset.scaleX, dataset.scaleY, dataset.scaleZ);//Changed this to not be / maxScale for each
            }

            return volObj;
        }

        public static void SpawnCrossSectionPlane(VolumeRenderedObject volobj)
        {
            GameObject quad = GameObject.Instantiate((GameObject)Resources.Load("CrossSectionPlane"));
            quad.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CrossSectionPlane csplane = quad.gameObject.GetComponent<CrossSectionPlane>();
            csplane.targetObject = volobj;
            quad.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { quad };
#endif
        }

        public static void SpawnNamedCrossSectionPlane(VolumeRenderedObject volobj, string name)
        {
            GameObject quad = GameObject.Instantiate((GameObject)Resources.Load("CrossSectionPlane"));
            quad.name = name;
            quad.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CrossSectionPlane csplane = quad.gameObject.GetComponent<CrossSectionPlane>();
            csplane.targetObject = volobj;
            quad.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { quad };
#endif
        }

        public static void SpawnCutoutBox(VolumeRenderedObject volobj)
        {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("CutoutBox"));
            obj.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CutoutBox cbox = obj.gameObject.GetComponent<CutoutBox>();
            cbox.targetObject = volobj;
            obj.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { obj };
#endif
        }

        public static void SpawnNamedCutoutBox(VolumeRenderedObject volobj, string name)
        {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("CutoutBox"));
            obj.name = name;
            obj.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            CutoutBox cbox = obj.gameObject.GetComponent<CutoutBox>();
            cbox.targetObject = volobj;
            obj.transform.position = volobj.transform.position;

#if UNITY_EDITOR
            UnityEditor.Selection.objects = new UnityEngine.Object[] { obj };
#endif
        }
    }
}
