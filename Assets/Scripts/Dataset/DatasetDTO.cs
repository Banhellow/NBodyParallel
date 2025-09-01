using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dataset
{
    [Serializable]
    public class DatasetDTO
    {
        public DatasetEntry[] entries;

        public DatasetDTO(int size, float[] masses)
        {
            if (size != masses.Length)
            {
                Debug.LogError("Number of objects is different from masses array");
                return;
            }
            
            entries = new DatasetEntry[size];
            for (int i = 0; i < size; i++)
            {
                entries[i] = new DatasetEntry((int) masses[i]);
            }
        }

        public void AddPositionRecords(List<Transform> positions)
        {
            if (entries.Length != positions.Count)
            {
                Debug.LogError("Entries count and positions count are missmatched");
                return;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i].position;
                var pos = new[] { position.x, position.y, position.z };
                entries[i].positions.Add(pos);
            }
        }
    }
}