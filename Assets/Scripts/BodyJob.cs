using SimulationBackend;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BodyJob : IJobParallelFor
{
    [ReadOnly] private NativeArray<float> _masses;
    [ReadOnly] private NativeArray<float3> _positions;
    private NativeArray<float3> _velocities;

    private float _minDistance;
    private float _gravityConstant;
    private float _dt;

    public BodyJob(NativeArray<float3> positions, NativeArray<float3> velocities, NativeArray<float> masses, float gravityConstant, float minDistance, float dt) : this()
    {
        _positions = positions;
        _velocities = velocities;
        _gravityConstant = gravityConstant;
        _minDistance = minDistance;
        _masses = masses;
        _dt = dt;
    }

    public void Execute(int index)
    {
        var position = _positions[index];
        var acceleration = float3.zero;
        for (int i = 0; i < _positions.Length; i++)
        {
            if (index == i)
            {
                continue;
            }

            acceleration += GravityMath.Acceleration(position, _positions[i], _masses[i], _gravityConstant, _minDistance);
        }

        _velocities[index] += acceleration * _dt;
    }
}
