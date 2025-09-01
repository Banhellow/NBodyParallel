using System;
using Newtonsoft.Json;

namespace Requests
{
    [Serializable]
    public class WebSocketInitMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("particle_types")]
        public float[] Masses { get; set; }
        
        [JsonProperty("positions")]
        public float[][][] Positions { get; set; }
    }
}