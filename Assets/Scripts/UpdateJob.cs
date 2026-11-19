using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct UpdateJob : IJobParallelForTransform
{
    [ReadOnly] private NativeArray<float3> _velocities;
    private NativeArray<float3> _positions;
    public UpdateJob(NativeArray<float3> velocities, NativeArray<float3> positions)
    {
        _velocities = velocities;
        _positions = positions;
    }

    public void Execute(int index, TransformAccess transform)
    {
        float3 newPosition = _positions[index] + _velocities[index];
        _positions[index] = newPosition;
        transform.position = newPosition;
    }
}