using System;
using System.Collections.Generic;

namespace Dataset
{
    [Serializable]
    public class DatasetEntry
    {
        public int mass;
        public List<float[]> positions;


        public DatasetEntry(int mass)
        {
            this.mass = mass;
            positions = new List<float[]>();
        }
    }
}