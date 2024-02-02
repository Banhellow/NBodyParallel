using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;

public struct UpdateJob : IJobParallelForTransform
{
    [ReadOnly] private NativeArray<Vector3> _velocities;
    public UpdateJob(NativeArray<Vector3> velocities)
    {
        _velocities = velocities;
    }

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += _velocities[index];
    }
}