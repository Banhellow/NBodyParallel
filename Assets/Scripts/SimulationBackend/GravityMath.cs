using Unity.Mathematics;
using UnityEngine;

namespace SimulationBackend
{
    public static class GravityMath
    {
        public static float3 Acceleration(float3 position, float3 otherPosition, float otherMass, float gravityConstant, float minDistance)
        {
            var delta = otherPosition - position;
            var distance = math.max(math.length(delta), minDistance);
            return gravityConstant * otherMass * delta / (distance * distance * distance);
        }

        public static void ApplyGravity(Vector3[] positions, Vector3[] velocities, float[] masses,
            float gravityConstant, float minDistance, float dt)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                var position = (float3)positions[i];
                var acceleration = float3.zero;
                for (int j = 0; j < positions.Length; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    acceleration += Acceleration(position, positions[j], masses[j], gravityConstant, minDistance);
                }

                velocities[i] += (Vector3)(acceleration * dt);
            }
        }

        public static void Integrate(Vector3[] positions, Vector3[] velocities)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] += velocities[i];
            }
        }
    }
}
