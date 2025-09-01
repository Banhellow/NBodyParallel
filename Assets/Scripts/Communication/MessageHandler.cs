using System.Collections.Generic;
using System.Linq;
using Dataset;
using Requests;
using UnityEngine;
using Utilities;

namespace Communication
{
    public class MessageHandler
    {
        private WebSocketInitMessage simulatedData;
        private Queue<float[][]> receivedPositions;

        public WebSocketInitMessage SimulatedData => simulatedData;

        public MessageHandler()
        {
            receivedPositions = new Queue<float[][]>();
        }

        public void AddFrame(float[][] positions)
        {
            receivedPositions.Enqueue(positions);
        }

        public float[][] GetFrame()
        {
            return receivedPositions.Count == 0 ? null : receivedPositions.Dequeue();
        }

        public void SetSimulatedData(Vector3[][] predictedFrames, float[] masses)
        {

            var positions = new float[predictedFrames.Length][][];
            for (int t = 0; t < predictedFrames.Length; t++)
            {
                positions[t] = new float[masses.Length][];
                for (int i = 0; i < masses.Length; i++)
                {
                    positions[t][i] = ParseUtilities.Vector3ToFloatArray(predictedFrames[t][i]);
                }
            }

            simulatedData = new WebSocketInitMessage { Positions = positions, Masses = masses, Type = "init" };
        }
        
        
    }
}