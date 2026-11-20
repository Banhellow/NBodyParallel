using System.Collections.Generic;
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
            lastPositions = new Vector3[ObjectsToUpdate.Count];
            ReadPositions(lastPositions);

            messageHandler = new MessageHandler();
            messageHandler.SetSimulatedData(PredictFrames(WorkingWindowSize), masses);
            connection = new WebSocketConnection(URL, messageHandler);
            connection.InitializeConnection();
        }

        public override void Dispose()
        {
            connection?.CloseConnection();
            connection = null;
            base.Dispose();
        }

        public override void SimulateFrame()
        {
            connection.AcceptMessage();
            var positions = messageHandler.GetFrame();
            if (positions != null)
            {
                lastPositions = ParseUtilities.ConvertToVector(positions);
            }

            WritePositions(lastPositions);
        }

        private Vector3[][] PredictFrames(int framesCount)
        {
            var prediction = new Vector3[framesCount][];
            prediction[0] = (Vector3[])lastPositions.Clone();
            var frameVelocities = (Vector3[])velocities.Clone();
            for (int i = 1; i < framesCount; i++)
            {
                var frame = (Vector3[])prediction[i - 1].Clone();
                GravityMath.ApplyGravity(frame, frameVelocities, masses, GravityConstant, MinDistance, TimeStep);
                GravityMath.Integrate(frame, frameVelocities);
                prediction[i] = frame;
            }

            return prediction;
        }
    }
}
