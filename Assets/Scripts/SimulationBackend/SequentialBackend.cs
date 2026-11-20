using System.Collections.Generic;
using UnityEngine;

namespace SimulationBackend
{
    public class SequentialBackend : SimulationBackendBase
    {
        private Vector3[] positions;
        private Vector3[] velocities;
        private float[] masses;

        public SequentialBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant)
            : base(objectsToUpdate, minDistance, gravityConstant) { }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            velocities = newVelocities;
            masses = newMasses;
            positions = new Vector3[ObjectsToUpdate.Count];
        }

        public override void SimulateFrame()
        {
            ReadPositions(positions);
            GravityMath.ApplyGravity(positions, velocities, masses, GravityConstant, MinDistance, TimeStep);
            GravityMath.Integrate(positions, velocities);
            WritePositions(positions);
        }
    }
}
