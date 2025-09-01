using UnityEngine;

namespace Dataset
{
    [CreateAssetMenu(menuName = "Settings/Dataset", fileName = "Dataset_")]
    public class DatasetSettings : ScriptableObject
    {
        [SerializeField] private int trainSetSize;
        [SerializeField] private string datasetPath;
        [SerializeField] private string datasetName;
        public int DataSetSize => trainSetSize;
        public string DataSetPath => datasetPath;
        public string DataSetName => datasetName;

    }
}