using System;
using System.Collections.Generic;
using SimulationBackend;
using UnityEngine;

namespace DefaultNamespace
{
    public class SimulationBackendFactory
    {
        public SimulationBackendFactory()
        {
            
        }

        public SimulationBackendBase Create(SimulationType type, List<Transform> objects, float minDistance,
            float gravity, ComputeShader shader = null)
        {
            return type switch
            {
                SimulationType.Sequential => new SequentialBackend(objects, gravity, minDistance),
                SimulationType.ParallelCpu => new ParallelCpuBackend(objects, minDistance, gravity),
                SimulationType.ParallelGpu => new ParallelGpuBackend(objects, minDistance, gravity, shader),
                SimulationType.PredictionModel => new PredictionModelBackend(objects, minDistance, gravity),
                SimulationType.WSPredictionModel => new WSPredictionModelBackend(objects,minDistance, gravity),
                _ => throw new NotImplementedException()
            };
        }
    }
}