using UnityEngine;
using System.Collections.Generic;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Cross section plane.
    /// Used for cutting a model (cross section view).
    /// </summary>
    [ExecuteInEditMode]
    public class CrossSectionPlane : MonoBehaviour
    {
        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        public VolumeRenderedObject targetObject;

        private void OnDisable()
        {
            if (targetObject != null)
                targetObject.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_PLANE");
        }

        private void Update()
        {
            if (targetObject == null)
                return;
            
            Material mat = targetObject.meshRenderer.sharedMaterial;

            mat.EnableKeyword("CUTOUT_PLANE");

            //List<Matrix4x4> currentCrossectionPlaneList = new List<Matrix4x4>();

            //mat.GetMatrixArray("_CrossSectionMatrixPlaneArray", currentCrossectionPlaneList);
            //mat.SetMatrixArray("_CrossSectionMatrixPlaneArray", currentCrossectionPlaneList);
            //Applies the matrix transformation operation to transform the targetPbject to the CrossSectionPlane matrix space
            mat.SetMatrix("_CrossSectionMatrix", transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix);
        }
    }
}
