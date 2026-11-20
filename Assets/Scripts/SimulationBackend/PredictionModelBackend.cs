using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Requests;
using Responses;
using UnityEngine;
using Utilities;

namespace SimulationBackend
{
    public class PredictionModelBackend : SimulationBackendBase
    {
        private const int WorkingWindowSize = 7;
        private const string Url = "http://127.0.0.1:8000/predict";

        private readonly HttpClient client = new();
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private float[] masses;
        private Vector3[] positions;
        private Vector3[] velocities;
        private Vector3[][] workingWindowPositions;
        private int warmupFrames;
        private Task<Vector3[]> pendingRequest;
        private bool requestFailed;

        public PredictionModelBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant)
            : base(objectsToUpdate, minDistance, gravityConstant) { }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            velocities = newVelocities;
            masses = newMasses;
            positions = new Vector3[ObjectsToUpdate.Count];
            workingWindowPositions = new Vector3[ObjectsToUpdate.Count][];
            for (int i = 0; i < ObjectsToUpdate.Count; i++)
            {
                workingWindowPositions[i] = new Vector3[WorkingWindowSize];
            }
        }

        public override void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            client.Dispose();
            base.Dispose();
        }

        public override void SimulateFrame()
        {
            if (warmupFrames < WorkingWindowSize)
            {
                SimulateWarmupFrame();
                return;
            }

            if (requestFailed)
            {
                return;
            }

            if (pendingRequest == null)
            {
                pendingRequest = RequestPredictedPositionsAsync(cancellationTokenSource.Token);
                return;
            }

            if (!pendingRequest.IsCompleted)
            {
                return;
            }

            if (!pendingRequest.IsCompletedSuccessfully)
            {
                requestFailed = true;
                Debug.LogError($"Prediction request failed, stopping the prediction backend: {pendingRequest.Exception?.GetBaseException().Message}");
                return;
            }

            var predictedPositions = pendingRequest.Result;
            UpdateWorkingWindow(predictedPositions);
            WritePositions(predictedPositions);
            pendingRequest = RequestPredictedPositionsAsync(cancellationTokenSource.Token);
        }

        private void SimulateWarmupFrame()
        {
            ReadPositions(positions);
            GravityMath.ApplyGravity(positions, velocities, masses, GravityConstant, MinDistance, TimeStep);
            GravityMath.Integrate(positions, velocities);
            WritePositions(positions);

            for (int i = 0; i < positions.Length; i++)
            {
                workingWindowPositions[i][warmupFrames] = positions[i];
            }

            warmupFrames++;
        }

        private async Task<Vector3[]> RequestPredictedPositionsAsync(CancellationToken token)
        {
            var requestBody = new PredictionRequest { Masses = masses, Positions = ExtractPositions() };
            var json = JsonConvert.SerializeObject(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(Url, content, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseData = JsonConvert.DeserializeObject<PredictionResponse>(responseBody);
            return ParseUtilities.ConvertToVector(responseData.PredictedPositions);
        }

        private float[][][] ExtractPositions()
        {
            var result = new float[workingWindowPositions.Length][][];
            for (int i = 0; i < workingWindowPositions.Length; i++)
            {
                result[i] = new float[WorkingWindowSize][];
                for (int j = 0; j < WorkingWindowSize; j++)
                {
                    result[i][j] = ParseUtilities.Vector3ToFloatArray(workingWindowPositions[i][j]);
                }
            }

            return result;
        }

        private void UpdateWorkingWindow(Vector3[] newPositions)
        {
            for (int i = 0; i < workingWindowPositions.Length; i++)
            {
                var window = workingWindowPositions[i];
                Array.Copy(window, 1, window, 0, window.Length - 1);
                window[^1] = newPositions[i];
            }
        }
    }
}
