using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimulationBackend
{
    public abstract class SimulationBackendBase : IDisposable
    {
        protected List<Transform> ObjectsToUpdate;
        protected readonly float MinDistance;
        protected readonly float GravityConstant;
        public SimulationBackendBase(List<Transform> objectsToUpdate, float minDistance, float gravityConstant)
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
    }
}