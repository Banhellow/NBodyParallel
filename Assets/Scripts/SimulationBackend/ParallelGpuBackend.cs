using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimulationBackend
{
    public class ParallelGpuBackend : SimulationBackendBase
    {
        private int bodiesToSimulate;
        private ComputeShader parallelShader;
        private Vector3[] velocities;
        private float[] masses;

        public ParallelGpuBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant, ComputeShader shader) 
            : base(objectsToUpdate, minDistance, gravityConstant)
        {
            bodiesToSimulate = objectsToUpdate.Count;
            parallelShader = shader;
        }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            velocities = newVelocities;
            masses = newMasses;
        }

        public override void SimulateFrame()
        {
            var bodiesData = new Body[bodiesToSimulate];
            for (int i = 0; i < bodiesData.Length; i++)
            {
                bodiesData[i] = new Body
                {
                    position = ObjectsToUpdate[i].position,
                    velocity = velocities[i],
                    mass = masses[i]
                };
            }

            using var computeBuffer = new ComputeBuffer(bodiesData.Length, Body.GetSize());
        
            computeBuffer.SetData(bodiesData);
            parallelShader.SetBuffer(0, "bodies", computeBuffer);
            parallelShader.SetInt("bodiesCount", bodiesData.Length);
            parallelShader.SetFloat("gravityConstant", GravityConstant);
            parallelShader.SetFloat("constraint", MinDistance);
            parallelShader.SetFloat("dt", 1/ 144f);
        
            var groupsCount = Mathf.FloorToInt(bodiesData.Length / 128f);
            parallelShader.Dispatch(0,groupsCount, 1, 1);
        
            computeBuffer.GetData(bodiesData);
            for (int i = 0; i < bodiesData.Length; i++)
            {
                ObjectsToUpdate[i].position += bodiesData[i].velocity;
                velocities[i] = bodiesData[i].velocity;
            }
            
        }
        
        public override void Dispose()
        {

        }
        
        private struct Body
        {
            public Vector3 position;
            public Vector3 velocity;
            public float mass;

            public static int GetSize()
            {
                return sizeof(float) * 7;
            }
        }
    }
}