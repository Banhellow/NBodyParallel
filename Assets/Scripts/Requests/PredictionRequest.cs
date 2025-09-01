using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Requests
{
    [Serializable]
    public class PredictionRequest
    {
        [JsonProperty("particle_types")]
        public float[] Masses { get; set; }
        
        [JsonProperty("positions")]
        public float[][][] Positions { get; set; }
    }
}