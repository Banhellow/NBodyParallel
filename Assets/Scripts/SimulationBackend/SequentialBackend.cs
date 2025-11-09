using System.Collections.Generic;
using UnityEngine;

namespace SimulationBackend
{
    public class SequentialBackend : SimulationBackendBase
    {
        private Vector3[] velocities;
        private float[] masses;
        public SequentialBackend(List<Transform> objectToUpdate, float gravityConstant, float minDistance) 
            : base(objectToUpdate, minDistance, gravityConstant)
        {
            velocities = new Vector3[objectToUpdate.Count];
        }

        public override void SimulateFrame()
        {
            UpdateForces();
            UpdateBodies();
        }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            velocities = newVelocities;
            masses = newMasses;
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
    }
}