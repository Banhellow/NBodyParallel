using System.Collections.Generic;
using System.Linq;
using Communication;
using UnityEngine;
using Utilities;

namespace SimulationBackend
{
    public class WSPredictionModelBackend : SimulationBackendBase
    {
        private const int WorkingWindowSize = 7;
        private const string URL = "ws://127.0.0.1:8000/ws";
        private float[] masses;
        private Vector3[] velocities;
        private MessageHandler messageHandler;
        private Vector3[] lastPositions;
        private WebSocketConnection connection;

        public WSPredictionModelBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant)
            : base(objectsToUpdate, minDistance, gravityConstant) { }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            velocities = newVelocities;
            masses = newMasses;
            lastPositions = ObjectsToUpdate.Select(x => x.position).ToArray();
        }

        public void InitializeConnection()
        {
            messageHandler = new MessageHandler();
            var predictedFrames = PredictFrames(WorkingWindowSize);
            messageHandler.SetSimulatedData(predictedFrames, masses);
            connection = new WebSocketConnection(URL, messageHandler);
            connection.InitializeConnection();
        }

        public override void Dispose()
        {
            connection.CloseConnection();
            connection = null;
            base.Dispose();
        }

        public override void SimulateFrame()
        {
            connection.AcceptMessage();
            var predictedPositions = RequestPredictedPositions();
            for (int i = 0; i < ObjectsToUpdate.Count; i++)
            {
                ObjectsToUpdate[i].position = predictedPositions[i];
            }
        }

        private Vector3[] RequestPredictedPositions()
        {
            var positions = messageHandler.GetFrame();
            var parsedPositions = positions == null ? lastPositions : ParseUtilities.ConvertToVector(positions);
            if (positions != null)
            {
                lastPositions = parsedPositions;
            }
            
            return parsedPositions;
        }


        
        private Vector3[] PredictForces(Vector3[] positions)
        {
            var frameVelocities = new Vector3[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                var force = Vector3.zero;
                for (int j = 0; j < positions.Length; j++)
                {
                    if (i != j)
                    {
                        var delta = positions[j] - positions[i];
                        var distance = Mathf.Max(delta.magnitude, MinDistance);
                        force += delta * (masses[i] * masses[j] / Mathf.Pow(distance, 3));
                    }
                }

                frameVelocities[i] += force / masses[i] * (GravityConstant * Time.deltaTime);
            }

            return frameVelocities;
        }

        private Vector3[] PredictNewPositions(Vector3[] oldPositions, Vector3[] currentVelocities)
        {
            var newPositions = new Vector3[oldPositions.Length];
            for (int i = 0; i < newPositions.Length; i++)
            {
                newPositions[i] = oldPositions[i] + currentVelocities[i];
            }

            return newPositions;
        }

        private Vector3[][] PredictFrames(int framesCount)
        {
            var prediction = new Vector3[framesCount][];
            var currentPositions= ObjectsToUpdate.Select(x => x.position).ToArray();
            prediction[0] = currentPositions;
            for (int i = 1; i < framesCount; i++)
            {
                var newVelocities = PredictForces(prediction[i - 1]);
                prediction[i] = PredictNewPositions(prediction[i - 1], newVelocities);
            }

            return prediction;
        }
    }
}