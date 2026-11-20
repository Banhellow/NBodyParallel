using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimulationBackend;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NBody.Tests.EditMode
{
    public class BackendConsistencyTests
    {
        private const int BodyCount = 300;
        private const int Steps = 30;
        private const float Gravity = 1f;
        private const float MinDistance = 10f;
        private const string ComputeShaderPath = "Assets/Scripts/BodyComputer.compute";

        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var gameObject in createdObjects)
            {
                Object.DestroyImmediate(gameObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void ParallelCpu_MatchesSequential()
        {
            var expected = Run(transforms => new SequentialBackend(transforms, MinDistance, Gravity));
            var actual = Run(transforms => new ParallelCpuBackend(transforms, MinDistance, Gravity));

            AssertPositionsMatch(expected, actual, 1e-3f);
        }

        [Test]
        public void ParallelGpu_MatchesSequential()
        {
            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);
            Assert.That(shader, Is.Not.Null, $"Compute shader not found at {ComputeShaderPath}.");

            var expected = Run(transforms => new SequentialBackend(transforms, MinDistance, Gravity));
            var actual = Run(transforms => new ParallelGpuBackend(transforms, MinDistance, Gravity, shader));

            AssertPositionsMatch(expected, actual, 1e-2f);
        }

        [TestCase(1)]
        [TestCase(7)]
        [TestCase(257)]
        public void ParallelGpu_UpdatesEveryBody(int bodyCount)
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Compute shaders are not supported on this device.");
            }

            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);
            var expected = Run(transforms => new SequentialBackend(transforms, MinDistance, Gravity), bodyCount);
            var actual = Run(transforms => new ParallelGpuBackend(transforms, MinDistance, Gravity, shader), bodyCount);

            AssertPositionsMatch(expected, actual, 1e-2f);
        }

        [TestCase(1)]
        [TestCase(3)]
        public void ParallelCpu_HandlesFewerBodiesThanWorkers(int bodyCount)
        {
            var expected = Run(transforms => new SequentialBackend(transforms, MinDistance, Gravity), bodyCount);
            var actual = Run(transforms => new ParallelCpuBackend(transforms, MinDistance, Gravity), bodyCount);

            AssertPositionsMatch(expected, actual, 1e-3f);
        }

        private Vector3[] Run(Func<List<Transform>, SimulationBackendBase> createBackend, int bodyCount = BodyCount)
        {
            Random.InitState(1234);
            var transforms = new List<Transform>(bodyCount);
            var velocities = new Vector3[bodyCount];
            var masses = new float[bodyCount];


            for (int i = 0; i < bodyCount; i++)
            {
                var gameObject = new GameObject($"Body {i}");
                createdObjects.Add(gameObject);
                gameObject.transform.position = RandomVector( 50f);
                transforms.Add(gameObject.transform);
                velocities[i] = RandomVector(0.05f);
                masses[i] = 1f + Random.value * 9f;
            }

            using (var backend = createBackend(transforms))
            {
                backend.Initialize(velocities, masses);
                for (int step = 0; step < Steps; step++)
                {
                    backend.SimulateFrame();
                }
            }

            var result = new Vector3[bodyCount];
            for (int i = 0; i < bodyCount; i++)
            {
                result[i] = transforms[i].position;
            }

            return result;
        }

        private static Vector3 RandomVector(float range)
        {
            return Random.insideUnitSphere * range;
        }

        private static void AssertPositionsMatch(Vector3[] expected, Vector3[] actual, float tolerance)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                var error = Vector3.Distance(expected[i], actual[i]);
                Assert.That(error, Is.LessThan(tolerance), $"Body {i}: expected {expected[i]}, got {actual[i]}.");
            }
        }
    }
}
