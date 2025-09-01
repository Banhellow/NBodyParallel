using UnityEngine;

namespace Utilities
{
    public static class ParseUtilities
    {
        public static Vector3[] ConvertToVector(float[][] vectors)
        {
            var result = new Vector3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                result[i] = new Vector3(vectors[i][0], vectors[i][1], vectors[i][2]);
            }

            return result;
        }
        
        public static float[][] VectorArrayToFloatArray(Vector3[] vectors)
        {
            var result = new float[vectors.Length][];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vec = new float[] { vectors[i].x, vectors[i].y, vectors[i].z };
                result[i] = vec;
            }

            return result;
        }

        public static float[] Vector3ToFloatArray(Vector3 vector)
        {
            return new[] { vector.x, vector.y, vector.z };
        }
    }
}