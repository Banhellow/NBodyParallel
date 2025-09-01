using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DefaultNamespace.Responses
{
    [Serializable]
    public class PredictionResponse
    {
        [JsonProperty("predicted_position")]
        public float[][] PredictedPositions { get; set; }
    }
}