using System.Collections.Generic;
using UnityEngine;

namespace SimulationBackend
{
    public class ParallelGpuBackend : SimulationBackendBase
    {
        private const int ThreadGroupSize = 256;
        private const int KernelIndex = 0;

        private static readonly int BodiesId = Shader.PropertyToID("bodies");
        private static readonly int BodiesCountId = Shader.PropertyToID("bodiesCount");
        private static readonly int GravityConstantId = Shader.PropertyToID("gravityConstant");
        private static readonly int ConstraintId = Shader.PropertyToID("constraint");
        private static readonly int DtId = Shader.PropertyToID("dt");

        private readonly ComputeShader parallelShader;
        private Vector3[] velocities;
        private float[] masses;
        private Body[] bodiesData;
        private ComputeBuffer computeBuffer;
        private int groupsCount;

        public ParallelGpuBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant, ComputeShader shader)
            : base(objectsToUpdate, minDistance, gravityConstant)
        {
            parallelShader = shader;
        }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            ReleaseBuffer();
            velocities = newVelocities;
            masses = newMasses;

            var count = ObjectsToUpdate.Count;
            bodiesData = new Body[count];
            if (count == 0)
            {
                return;
            }

            computeBuffer = new ComputeBuffer(count, Body.GetSize());
            groupsCount = Mathf.CeilToInt(count / (float)ThreadGroupSize);

            parallelShader.SetBuffer(KernelIndex, BodiesId, computeBuffer);
            parallelShader.SetInt(BodiesCountId, count);
            parallelShader.SetFloat(GravityConstantId, GravityConstant);
            parallelShader.SetFloat(ConstraintId, MinDistance);
            parallelShader.SetFloat(DtId, TimeStep);
        }

        public override void SimulateFrame()
        {
            if (computeBuffer == null)
            {
                return;
            }

            for (int i = 0; i < bodiesData.Length; i++)
            {
                bodiesData[i].position = ObjectsToUpdate[i].position;
                bodiesData[i].velocity = velocities[i];
                bodiesData[i].mass = masses[i];
            }

            computeBuffer.SetData(bodiesData);
            parallelShader.Dispatch(KernelIndex, groupsCount, 1, 1);
            computeBuffer.GetData(bodiesData);

            for (int i = 0; i < bodiesData.Length; i++)
            {
                ObjectsToUpdate[i].position += bodiesData[i].velocity;
                velocities[i] = bodiesData[i].velocity;
            }
        }

        public override void Dispose()
        {
            ReleaseBuffer();
        }

        private void ReleaseBuffer()
        {
            computeBuffer?.Release();
            computeBuffer = null;
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
