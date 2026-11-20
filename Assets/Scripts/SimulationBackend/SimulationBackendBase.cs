using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimulationBackend
{
    public abstract class SimulationBackendBase : IDisposable
    {
        protected const float TimeStep = 1f / 144f;

        protected readonly List<Transform> ObjectsToUpdate;
        protected readonly float MinDistance;
        protected readonly float GravityConstant;

        protected SimulationBackendBase(List<Transform> objectsToUpdate, float minDistance, float gravityConstant)
        {
            ObjectsToUpdate = objectsToUpdate;
            MinDistance = minDistance;
            GravityConstant = gravityConstant;
        }

        public abstract void Initialize(Vector3[] velocities, float[] masses);
        public abstract void SimulateFrame();

        public virtual void Dispose()
        {
        }

        protected void ReadPositions(Vector3[] target)
        {
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = ObjectsToUpdate[i].position;
            }
        }

        protected void WritePositions(Vector3[] source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                ObjectsToUpdate[i].position = source[i];
            }
        }
    }
}
