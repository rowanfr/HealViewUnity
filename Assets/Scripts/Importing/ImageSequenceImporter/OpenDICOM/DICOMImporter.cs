using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Render;
using Microsoft.MixedReality.Toolkit.UI;


namespace UnityVolumeRendering
{
    /// <summary>
    /// DICOM importer.
    /// Reads a 3D DICOM dataset from a list of DICOM files.
    /// </summary>
    public class DICOMImporter
    {
        ProgressIndicatorLoadingBar indicator;
        public class DICOMSliceFile : IImageSequenceFile
        {
            public DicomFile file;
            public string filePath;
            public float location = 0;
            public Vector3 position = Vector3.zero;
            public float intercept = 0.0f;
            public float slope = 1.0f;
            public float pixelSpacingX = 0.0f;
            public float pixelSpacingY = 0.0f;
            public string seriesUID = "";
            public bool missingLocation = false;

            public string GetFilePath()
            {
                return filePath;
            }
        }

        public class DICOMSeries : IImageSequenceSeries
        {
            public List<DICOMSliceFile> dicomFiles = new List<DICOMSliceFile>();

            public IEnumerable<IImageSequenceFile> GetFiles()
            {
                return dicomFiles;
            }
        }

        private int iFallbackLoc = 0;


        public void setIndicator(ProgressIndicatorLoadingBar setIndicator)
        {
            indicator = setIndicator;
        }

        public IEnumerable<IImageSequenceSeries> LoadSeries(List<string> fileCandidates)
        {


            // Load all DICOM files
            List<DICOMSliceFile> files = new List<DICOMSliceFile>();

            fileCandidates.Sort();

            // Split parsed DICOM files into series (by DICOM series UID)
            Dictionary<string, DICOMSeries> seriesByUID = new Dictionary<string, DICOMSeries>();

            if (indicator != null)
            {
                for (int n = 0; n < fileCandidates.Count(); n++)
                {
                    DICOMSliceFile sliceFile = ReadDICOMFile(fileCandidates[n]);
                    if (sliceFile != null)
                    {
                        files.Add(sliceFile);
                    }
                    indicator.Progress = 0.95f * (float)n / (float)fileCandidates.Count();
                }

                for (int n = 0; n < files.Count; n++)
                {
                    if (!seriesByUID.ContainsKey(files[n].seriesUID))
                    {
                        seriesByUID.Add(files[n].seriesUID, new DICOMSeries());
                    }
                    seriesByUID[files[n].seriesUID].dicomFiles.Add(files[n]);
                    indicator.Progress = 0.95f + (0.95f * (float)n / (float)files.Count());
                }
            } else
            {
                for (int n = 0; n < fileCandidates.Count(); n++)
                {
                    DICOMSliceFile sliceFile = ReadDICOMFile(fileCandidates[n]);
                    if (sliceFile != null)
                    {
                        files.Add(sliceFile);
                    }
                }

                for (int n = 0; n < files.Count; n++)
                {
                    if (!seriesByUID.ContainsKey(files[n].seriesUID))
                    {
                        seriesByUID.Add(files[n].seriesUID, new DICOMSeries());
                    }
                    seriesByUID[files[n].seriesUID].dicomFiles.Add(files[n]);
                }
            }

            Debug.Log($"Loaded {seriesByUID.Count} DICOM series");

            return new List<DICOMSeries>(seriesByUID.Values);
        }

        public VolumeDataset ImportSeries(IImageSequenceSeries series, VolumeDataset dataset)
        {
            DICOMSeries dicomSeries = (DICOMSeries)series;
            List<DICOMSliceFile> files = dicomSeries.dicomFiles;

            // Check if the series is missing the slice location tag
            bool needsCalcLoc = false;
            foreach (DICOMSliceFile file in files)
            {
                needsCalcLoc |= file.missingLocation;
            }

            // Calculate slice location from "Image Position" (0020,0032)
            if (needsCalcLoc)
                CalcSliceLocFromPos(files);

            // Sort files by slice location
            files.Sort((DICOMSliceFile a, DICOMSliceFile b) => { return a.location.CompareTo(b.location); });

            Debug.Log($"Importing {files.Count} DICOM slices");

            if (files.Count <= 1)
            {
                Debug.LogError("Insufficient number of slices.");
                return null;
            }

            // Use passed dataset which allows for async as we're not constructing it off the main thread
            dataset.datasetName = Path.GetFileName(files[0].filePath);
            DicomPixelData datasetPixelData = DicomPixelData.Create(files[0].file.Dataset);
            dataset.dimX = datasetPixelData.Width;
            dataset.dimY = datasetPixelData.Height;
            dataset.dimZ = files.Count;

            int dimension = dataset.dimX * dataset.dimY * dataset.dimZ;
            dataset.data = new float[dimension];
            if (indicator != null)
            {
                for (int iSlice = 0; iSlice < files.Count; iSlice++)
                {
                    indicator.Progress = (float)iSlice / (float)files.Count;
                    DICOMSliceFile slice = files[iSlice];
                    DicomPixelData pixelImage = DicomPixelData.Create(slice.file.Dataset);
                    IPixelData pixelData = PixelDataFactory.Create(pixelImage, 0);

                    //int[] pixelArr = ToPixelArray(pixelData);

                    for (int iRow = 0; iRow < pixelData.Height; iRow++)
                    {
                        for (int iCol = 0; iCol < pixelData.Width; iCol++)
                        {
                            //int pixelIndex = (iRow * pixelData.Width) + iCol;
                            int dataIndex = (iSlice * pixelData.Width * pixelData.Height) + (iRow * pixelData.Width) + iCol;

                            float hounsfieldValue = (float)pixelData.GetPixel(iCol, iRow) * slice.slope + slice.intercept;

                            dataset.data[dataIndex] = Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
                        }
                    }
                }
            }
            else
            {
                for (int iSlice = 0; iSlice < files.Count; iSlice++)
                {
                    Debug.Log("Slice" + iSlice.ToString());
                    DICOMSliceFile slice = files[iSlice];
                    DicomPixelData pixelImage = DicomPixelData.Create(slice.file.Dataset);
                    IPixelData pixelData = PixelDataFactory.Create(pixelImage, 0);

                    //int[] pixelArr = ToPixelArray(pixelData);

                    for (int iRow = 0; iRow < pixelData.Height; iRow++)
                    {
                        for (int iCol = 0; iCol < pixelData.Width; iCol++)
                        {
                            //int pixelIndex = (iRow * pixelData.Width) + iCol;
                            int dataIndex = (iSlice * pixelData.Width * pixelData.Height) + (iRow * pixelData.Width) + iCol;

                            float hounsfieldValue = (float)pixelData.GetPixel(iCol, iRow) * slice.slope + slice.intercept;

                            dataset.data[dataIndex] = Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
                        }
                    }
                }
            }
            

            if (files[0].pixelSpacingX > 0.0f || files[0].pixelSpacingY > 0.0f)
            {
                dataset.scaleX = files[0].pixelSpacingX * dataset.dimX / 1000;// / 1000 as original scale is in mm while Unity scale is in m
                dataset.scaleY = files[0].pixelSpacingY * dataset.dimY / 1000;// / 1000 as original scale is in mm while Unity scale is in m
                dataset.scaleZ = Mathf.Abs(files[files.Count - 1].location - files[0].location) / 1000;//Added / 1000 as original scale is in mm while Unity scale is in m
            }

            dataset.FixDimensions();

            return dataset;
        }

        private DICOMSliceFile ReadDICOMFile(string filePath)
        {
            DicomFile file = LoadFile(filePath);

            if (file != null && file.Dataset.Contains(DicomTag.PixelData))
            {
                DICOMSliceFile slice = new DICOMSliceFile();
                slice.file = file;
                slice.filePath = filePath;


                // Read location (optional)
                if (file.Dataset.Contains(DicomTag.SliceLocation))
                {
                    slice.location = file.Dataset.GetValues<float>(DicomTag.SliceLocation)[0];
                }
                // If no location tag, read position tag (will need to calculate location afterwards)
                else if (file.Dataset.Contains(DicomTag.ImagePositionPatient))
                {
                    float[] elemLoc = file.Dataset.GetValues<float>(DicomTag.ImagePositionPatient);
                    Vector3 pos = Vector3.zero;
                    pos.x = elemLoc[0];
                    pos.y = elemLoc[1];
                    pos.z = elemLoc[2];
                    slice.position = pos;
                    slice.missingLocation = true;
                }
                else
                {
                    Debug.LogError($"Missing location/position tag in file: {filePath}.\n The file will not be imported correctly.");
                    // Fallback: use counter as location
                    slice.location = (float)iFallbackLoc++;
                }

                // Read intercept
                if (file.Dataset.Contains(DicomTag.RescaleIntercept))
                {
                    slice.intercept = file.Dataset.GetValues<float>(DicomTag.RescaleIntercept)[0];
                }
                else
                    Debug.LogWarning($"The file {filePath} is missing the intercept element. As a result, the default transfer function might not look good.");

                // Read slope
                if (file.Dataset.Contains(DicomTag.RescaleSlope))
                {
                    slice.slope = file.Dataset.GetValues<float>(DicomTag.RescaleSlope)[0];
                }
                else
                    Debug.LogWarning($"The file {filePath} is missing the intercept element. As a result, the default transfer function might not look good.");

                // Read pixel spacing
                if (file.Dataset.Contains(DicomTag.PixelSpacing))
                {
                    slice.pixelSpacingX = file.Dataset.GetValues<float>(DicomTag.PixelSpacing)[0];//Only x value
                    slice.pixelSpacingY = file.Dataset.GetValues<float>(DicomTag.PixelSpacing)[1];//Only y value
                }

                // Read series UID
                if (file.Dataset.Contains(DicomTag.SeriesInstanceUID))
                {
                    slice.seriesUID = file.Dataset.GetValues<string>(DicomTag.SeriesInstanceUID)[0];
                }

                return slice;
            }
            return null;
        }

        private DicomFile LoadFile(string filePath)
        {
            DicomFile file = null;
            try
            {
                if (DicomFile.HasValidHeader(filePath))//This checks if this is a DICOM file
                    file = DicomFile.Open(filePath, FileReadOption.ReadAll);//This makes DicomFile Object
                else
                    Debug.LogError("Selected file is not a DICOM file.");
            }
            catch (Exception dicomFileException)
            {
                Debug.LogError($"Problems processing the DICOM file {filePath} :\n {dicomFileException}");
                return null;
            }
            return file;
        }

        private static int[] ToPixelArray(DicomPixelData pixelData)
        {
            int[] intArray;

                byte[] bytesArray = pixelData.GetFrame(0).Data;
                if (bytesArray != null && bytesArray.Length > 0)
                {
                    int cellSize = pixelData.BitsAllocated / 8;
                    int pixelCount = bytesArray.Length / cellSize;

                    intArray = new int[pixelCount];
                    int pixelIndex = 0;

                    // Byte array for a single cell/pixel value
                    byte[] cellData = new byte[cellSize];
                    for (int iByte = 0; iByte < bytesArray.Length; iByte++)
                    {
                        // Collect bytes for one cell (sample)
                        int index = iByte % cellSize;
                        cellData[index] = bytesArray[iByte];
                        // We have collected enough bytes for one cell => convert and add it to pixel array
                        if (index == cellSize - 1)
                        {
                            int cellValue = 0;
                            if (pixelData.BitsAllocated == 8)
                                cellValue = cellData[0];
                            else if (pixelData.BitsAllocated == 16)
                                cellValue = BitConverter.ToInt16(cellData, 0);
                            else if (pixelData.BitsAllocated == 32)
                                cellValue = BitConverter.ToInt32(cellData, 0);
                            else
                                Debug.LogError("Invalid format!");

                            intArray[pixelIndex] = cellValue;
                            pixelIndex++;
                        }
                    }
                    return intArray;
            }
            else
            {
                Debug.LogError("Pixel array is invalid");
                return null;
            }
        }

        private void CalcSliceLocFromPos(List<DICOMSliceFile> slices)
        {
            // We use the first slice as a starting point (a), andthe normalised vector (v) between the first and second slice as a direction.
            Vector3 v = (slices[1].position - slices[0].position).normalized;
            Vector3 a = slices[0].position;
            slices[0].location = 0.0f;

            for (int i = 1; i < slices.Count; i++)
            {
                // Calculate the vector between a and p (ap) and dot it with v to get the distance along the v vector (distance when projected onto v)
                Vector3 p = slices[i].position;
                Vector3 ap = p - a;
                float dot = Vector3.Dot(ap, v);
                slices[i].location = dot;
            }
        }
    }
}
