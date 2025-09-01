using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Requests;
using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading;
using DefaultNamespace.Responses;
using Extensions;

namespace SimulationBackend
{
    public class PredictionModelBackend : SimulationBackendBase
    {
        private const int WorkingWindowSize = 7;
        private float[] masses;
        private Vector3[] velocities;
        private Vector3[][] workingWindowPositions;
        private int executionCount;
        private readonly HttpClient client = new ();
        private bool executeOnce;
        private CancellationTokenSource cancellationTokenSource;
        
        public PredictionModelBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant) 
            : base(objectsToUpdate, minDistance, gravityConstant)
        {
            
        }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            velocities = newVelocities;
            masses = newMasses;
            cancellationTokenSource = new CancellationTokenSource();
            workingWindowPositions = new Vector3[ObjectsToUpdate.Count][];
            for (int i = 0; i < ObjectsToUpdate.Count; i++)
            {
                workingWindowPositions[i] = new Vector3[WorkingWindowSize];
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            cancellationTokenSource.Cancel();
        }

        public override void SimulateFrame()
        {
            if (executionCount < WorkingWindowSize)
            {
                UpdateForces();
                UpdateBodies();
                AddRecordToWorkingWindow();
                executionCount++;
                return;
            }
            
            var predictedPositions = RequestPredictedPositions();
            UpdateWorkingWindow(predictedPositions);
            for (int i = 0; i < ObjectsToUpdate.Count; i++)
            {
                ObjectsToUpdate[i].position = predictedPositions[i];
            }
        }

        private  Vector3[] RequestPredictedPositions()
        {
            var requestBody = new PredictionRequest { Masses = masses, Positions = ExtractPositions() };
            var parsedBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(parsedBody, Encoding.UTF8, "application/json");
            var url = "http://127.0.0.1:8000/predict";
            try
            {
                var response = client.PostAsync(url, content, cancellationTokenSource.Token).Result;
                var responseData =  response.Content.GetContentData<PredictionResponse>();
                return ConvertToVector(responseData.PredictedPositions);
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"Request Error: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception handling response: {e.Message}");
                throw;
            }
        }

        private Vector3[] ConvertToVector(float[][] vectors)
        {
            var result = new Vector3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                result[i] = new Vector3(vectors[i][0], vectors[i][1], vectors[i][2]);
            }

            return result;
        }

        private float[][][] ExtractPositions()
        {
            var result = new float[workingWindowPositions.Length][][];
            for (int i = 0; i < workingWindowPositions.Length; i++)
            {
                result[i] = new float[WorkingWindowSize][];
                for (int j = 0; j < WorkingWindowSize; j++)
                {
                    var vector = workingWindowPositions[i][j];
                    result[i][j] = new [] { vector.x, vector.y, vector.z };
                }
            }

            return result;
        }

        private void ShiftWorkingWindow()
        {
            foreach (var particlePosition in workingWindowPositions)
            {
                for (int i = 1; i < particlePosition.Length; i++)
                {
                    particlePosition[i - 1] = particlePosition[i];
                }
            }
        }

        private void UpdateWorkingWindow(Vector3[] newPositions)
        {
            ShiftWorkingWindow();
            for (int j = 0; j < newPositions.Length; j++)
            {
                workingWindowPositions[j][^1] = newPositions[j];
            }
        }
        
        private void UpdateForces()
        {
            for (int i = 0; i < ObjectsToUpdate.Count; i++)
            {
                var force = Vector3.zero;
                for (int j = 0; j < ObjectsToUpdate.Count; j++)
                {
                    if (i != j)
                    {
                        var delta = ObjectsToUpdate[j].position - ObjectsToUpdate[i].position;
                        var distance = Mathf.Max(delta.magnitude, MinDistance);
                        force += delta * (masses[i] * masses[j] / Mathf.Pow(distance, 3));
                    }
                }

                velocities[i] += force / masses[i] * (GravityConstant * Time.deltaTime);
            }
        }
        
        private void UpdateBodies()
        {
            for (int i = 0; i < ObjectsToUpdate.Count; i++)
            {
                ObjectsToUpdate[i].position += velocities[i];
            }
        }

        private void AddRecordToWorkingWindow()
        {
            var bodiesCount = ObjectsToUpdate.Count;
            for (int i = 0; i < bodiesCount; i++)
            {
                workingWindowPositions[i][executionCount] = ObjectsToUpdate[i].position;
            }
        }
    }
}