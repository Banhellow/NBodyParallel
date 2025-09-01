using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct BodyJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> _masses;
    [ReadOnly] public NativeArray<Vector3> _positions;
    public NativeArray<Vector3> _velocities;
    
    public float _minDistance;
    public float _gravityConstant;
    public float dt;

    public BodyJob(NativeArray<Vector3> positions, NativeArray<Vector3> velocities,NativeArray<float> masses, float gravityConstant, float minDistance, float dt) : this()
    {
        _positions = positions;
        _velocities = velocities;
        _gravityConstant = gravityConstant;
        _minDistance = minDistance;
        _masses = masses;
        this.dt = dt;
    }


    public void Execute(int index)
    {
        Vector3 force = CalculateForce(index);
        _velocities[index] +=  force / _masses[index] * dt;
    }

    private Vector3 CalculateForce(int currentIndex)
    {
        Vector3 force = Vector3.zero;
        for (int i = 0; i < _positions.Length; i++)
        {
            if (currentIndex == i)
            {
                continue;
            }

            var delta = _positions[i] - _positions[currentIndex];
            var distance = Mathf.Max(delta.magnitude, _minDistance);
            force += _gravityConstant * delta * (_masses[currentIndex] * _masses[i] / Mathf.Pow(distance, 3));
        }

        return force;
    }
}