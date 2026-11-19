using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct BodyJob : IJobParallelFor
{
    [ReadOnly] private NativeArray<float> _masses;
    [ReadOnly] private NativeArray<float3> _positions;
    private NativeArray<float3> _velocities;

    private float _minDistance;
    private float _gravityConstant;
    private float _dt;

    public BodyJob(NativeArray<float3> positions, NativeArray<float3> velocities,NativeArray<float> masses, float gravityConstant, float minDistance, float dt) : this()
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
        float3 force = CalculateForce(index);
        _velocities[index] +=  force / _masses[index] * _dt;
    }

    private float3 CalculateForce(int currentIndex)
    {
        var force = float3.zero;
        for (int i = 0; i < _positions.Length; i++)
        {
            if (currentIndex == i)
            {
                continue;
            }

            var delta = _positions[i] - _positions[currentIndex];
            var distance = Mathf.Max(math.length(delta), _minDistance);
            force += _gravityConstant * delta * (_masses[currentIndex] * _masses[i] / math.pow(distance, 3));
        }

        return force;
    }
}