using NUnit.Framework;
using SimulationBackend;
using Unity.Mathematics;
using UnityEngine;

namespace NBody.Tests.EditMode
{
    public class GravityMathTests
    {
        private const float Gravity = 1f;
        private const float MinDistance = 0.5f;

        [Test]
        public void Acceleration_PointsTowardsOtherBody()
        {
            var acceleration = GravityMath.Acceleration(float3.zero, new float3(2f, 0f, 0f), 1f, Gravity, MinDistance);

            Assert.That(acceleration.x, Is.GreaterThan(0f));
            Assert.That(acceleration.y, Is.EqualTo(0f));
            Assert.That(acceleration.z, Is.EqualTo(0f));
        }

        [Test]
        public void Acceleration_FollowsInverseSquareLaw()
        {
            var near = GravityMath.Acceleration(float3.zero, new float3(2f, 0f, 0f), 1f, Gravity, MinDistance);
            var far = GravityMath.Acceleration(float3.zero, new float3(4f, 0f, 0f), 1f, Gravity, MinDistance);

            Assert.That(near.x / far.x, Is.EqualTo(4f).Within(1e-4f));
        }

        [Test]
        public void Acceleration_ScalesWithOtherMass()
        {
            var light = GravityMath.Acceleration(float3.zero, new float3(3f, 0f, 0f), 1f, Gravity, MinDistance);
            var heavy = GravityMath.Acceleration(float3.zero, new float3(3f, 0f, 0f), 5f, Gravity, MinDistance);

            Assert.That(heavy.x / light.x, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void Acceleration_IsClampedBelowMinDistance()
        {
            var atMinDistance = GravityMath.Acceleration(float3.zero, new float3(MinDistance, 0f, 0f), 1f, Gravity, MinDistance);
            var closer = GravityMath.Acceleration(float3.zero, new float3(MinDistance * 0.1f, 0f, 0f), 1f, Gravity, MinDistance);

            Assert.That(closer.x, Is.LessThanOrEqualTo(atMinDistance.x));
        }

        [Test]
        public void Acceleration_IsFiniteForCoincidentBodies()
        {
            var acceleration = GravityMath.Acceleration(new float3(1f, 2f, 3f), new float3(1f, 2f, 3f), 1f, Gravity, MinDistance);

            Assert.That(math.all(math.isfinite(acceleration)), Is.True);
            Assert.That(math.length(acceleration), Is.EqualTo(0f));
        }

        [Test]
        public void ApplyGravity_ConservesMomentum()
        {
            var positions = new[] { new Vector3(-3f, 0f, 0f), new Vector3(2f, 1f, 0f), new Vector3(0f, -4f, 2f) };
            var velocities = new Vector3[3];
            var masses = new[] { 1f, 3f, 7f };

            for (int step = 0; step < 100; step++)
            {
                GravityMath.ApplyGravity(positions, velocities, masses, Gravity, MinDistance, 1f / 144f);
                GravityMath.Integrate(positions, velocities);
            }

            var momentum = Vector3.zero;
            for (int i = 0; i < masses.Length; i++)
            {
                momentum += masses[i] * velocities[i];
            }

            Assert.That(momentum.magnitude, Is.LessThan(1e-4f));
        }
    }
}
