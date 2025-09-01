using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Jobs;

namespace SimulationBackend
{
    public class ParallelCpuBackend : SimulationBackendBase
    {
        private readonly int bodiesToSimulate;
        private NativeArray<Vector3> velocities;
        private NativeArray<float> masses;
        public ParallelCpuBackend(List<Transform> objectsToUpdate, float minDistance, float gravityConstant)
            : base(objectsToUpdate, minDistance, gravityConstant)
        {
            bodiesToSimulate = objectsToUpdate.Count;
            velocities = new NativeArray<Vector3>(bodiesToSimulate, Allocator.Persistent);
            masses = new NativeArray<float>(bodiesToSimulate, Allocator.Persistent);
        }


        public override void Initialize(Vector3[] newVelocities, float[] newMasses)
        {
            velocities = new NativeArray<Vector3>(bodiesToSimulate, Allocator.Persistent);
            masses = new NativeArray<float>(bodiesToSimulate, Allocator.Persistent);
            for (int i = 0; i < bodiesToSimulate; i++)
            {
                velocities[i] = newVelocities[i];
                masses[i] = newMasses[i];
            }
        }

        public override void SimulateFrame()
        {
            var positions = new NativeArray<Vector3>(bodiesToSimulate, Allocator.TempJob);
            for (int i = 0; i < bodiesToSimulate; i++)
            {
                positions[i] = ObjectsToUpdate[i].position;
            }

            TransformAccessArray particles = new TransformAccessArray(ObjectsToUpdate.ToArray());
            var job = new BodyJob(positions, velocities, masses, GravityConstant, MinDistance, 1/144f);
            var batchCount = bodiesToSimulate / JobsUtility.JobWorkerCount;
            var handle = job.Schedule(bodiesToSimulate, batchCount);

            var updateJob = new UpdateJob(velocities);
            var updateHandle = updateJob.ScheduleByRef(particles, handle);
            updateHandle.Complete();
            positions.Dispose();
            particles.Dispose();
        }

        public override void Dispose()
        {
            velocities.Dispose();
            masses.Dispose();
        }
    }
}