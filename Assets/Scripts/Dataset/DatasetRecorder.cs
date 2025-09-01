using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Dataset
{
    public class DatasetRecorder
    {
        private DatasetSettings settings;

        private DatasetDTO dataset;
        private int recordsCount;
        
        public DatasetRecorder(DatasetSettings settings)
        {
            this.settings = settings;
        }

        public void Initialize(int size, float[] masses)
        {
            dataset = new DatasetDTO(size, masses);
            Debug.Log("Data set recording started");
        }

        public void Record(List<Transform> positions)
        {
            if (recordsCount >= settings.DataSetSize)
            {
                Debug.Log("Dataset recording finished");
                return;
            }
                
            dataset.AddPositionRecords(positions);
            recordsCount++;
        }
        
        public void TrySaveDataSet()
        {
            var filePath = Path.Combine(settings.DataSetPath, settings.DataSetName);
            ExportDataSet(filePath);
        }

        private void ExportDataSet(string path)
        {
            var json = JsonConvert.SerializeObject(dataset);
            File.WriteAllText(path, json);
        }
    }
}