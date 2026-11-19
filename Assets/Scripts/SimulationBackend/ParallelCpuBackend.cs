using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace SimulationBackend
{
    [BurstCompile]
    public class ParallelCpuBackend : SimulationBackendBase
    {
        private NativeArray<float3> velocities;
        private NativeArray<float> masses;
        private NativeArray<float3>  positions;
        private TransformAccessArray particles;
        
        public ParallelCpuBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant)
            : base(objectsToUpdate, minDistance, gravityConstant){ }

        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            DisposeBuffers();
            var count = ObjectsToUpdate.Count;
            velocities = new NativeArray<float3>(count, Allocator.Persistent);
            masses = new NativeArray<float>(count, Allocator.Persistent);
            positions = new NativeArray<float3>(count, Allocator.Persistent);
            particles = new TransformAccessArray(ObjectsToUpdate.ToArray());
            
            for (int i = 0; i < count; i++)
            {
                velocities[i] = newVelocities[i];
                masses[i] = newMasses[i];
                positions[i] = ObjectsToUpdate[i].position;
            }
        }

        public override void SimulateFrame()
        {
            var forcesJob = new BodyJob(positions, velocities, masses, GravityConstant, MinDistance, 1/144f);
            var batchCount = ObjectsToUpdate.Count / JobsUtility.JobWorkerCount;
            var forceHandle = forcesJob.Schedule( ObjectsToUpdate.Count, batchCount);

            var updateJob = new UpdateJob(velocities, positions);
            var updateHandle = updateJob.ScheduleByRef(particles, forceHandle);
            updateHandle.Complete();
        }

        public override void Dispose()
        {
            DisposeBuffers();
        }

        private void DisposeBuffers()
        {
            if(velocities.IsCreated) velocities.Dispose();
            if(masses.IsCreated) masses.Dispose();
            if(positions.IsCreated) positions.Dispose();
            if(particles.isCreated) particles.Dispose();
        }
    }
}