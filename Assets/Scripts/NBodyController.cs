using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

[BurstCompile]
public class NBodyController : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private bool enableParallel;
    [SerializeField] private int bodiesToSimulate;
    [SerializeField] private float gravityConstant;
    [SerializeField] private float minDistance;
    [SerializeField] private GravityBody bodyPrefab;
    public bool useComputeShader;
    private NativeArray<Vector3> velocities = new();
    private NativeArray<float> masses;
    private readonly List<Transform> _transforms = new();
    private bool simulationEnabled;
    
    //Perfomance counting
    private double totalExecutionTime;
    private int executionCount;
    private float deltaTime;
    private float framesCount;
    
    [SerializeField] private ComputeShader parallelShader;

    void Start()
    {
        Reset();
    }

    void Reset()
    {
        ResetBodies();
        totalExecutionTime = 0;
        executionCount = 0;
    }

    private void ResetBodies()
    {
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            Destroy(_transforms[i].gameObject);
        }
        
        _transforms.Clear();
    }

    private void OnDestroy()
    {
        velocities.Dispose();
        masses.Dispose();
    }

    private void StartSimulation()
    {
        simulationEnabled = true;
    }

    private void StopSimulation()
    {
        simulationEnabled = false;
    }


    void Update()
    {
        if (simulationEnabled == false)
        {
            return;
        }

        if (enableParallel)
        {
            var time = DateTime.Now;
            if (useComputeShader)
            {
                UpdateWithComputeShader();
            }
            else
            {
                UpdateInParallel();
            }
 
            totalExecutionTime = (DateTime.Now - time).TotalMilliseconds;
            executionCount++;
        }
        else
        {
            UpdateInSequence();
        }
        
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        framesCount = 1.0f / deltaTime;
    }
    
    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(20, 20, w - 40, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        float msec = deltaTime * 1000.0f;
        float fpsRounded = Mathf.Round(framesCount);
        string text = $"Threads:{JobsUtility.JobWorkerCount}, MS/frame ({msec:0.0} ms), parallel execution ms: {totalExecutionTime:0.0}ms, {fpsRounded:0.} FPS";
        GUI.Label(rect, text, style);
    }

    private void UpdateInSequence()
    {
        UpdateForces();
        UpdateBodies(velocities);
    }

    private void UpdateInParallel()
    {
        if (useComputeShader)
        {
            UpdateWithComputeShader();
            return;
        }

        NativeArray<Vector3> positions = new NativeArray<Vector3>(bodiesToSimulate, Allocator.TempJob);
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            positions[i] = _transforms[i].position;
        }

        TransformAccessArray particles = new TransformAccessArray(_transforms.ToArray());
        var job = new BodyJob(positions, velocities, masses, gravityConstant, minDistance, 1/144f);
        int batchCount = bodiesToSimulate / JobsUtility.JobWorkerCount;
        JobHandle handle = job.Schedule(bodiesToSimulate, batchCount);

        var updateJob = new UpdateJob(velocities);
        JobHandle updateHandle = updateJob.ScheduleByRef(particles, handle);
        updateHandle.Complete();

        positions.Dispose();
        particles.Dispose();
    }

    private void UpdateWithComputeShader()
    {
        if (!simulationEnabled)
        {
            return;
        }
        
        Body[] bodiesData = new Body[_transforms.Count];
        for (int i = 0; i < bodiesData.Length; i++)
        {
            bodiesData[i] = new Body
            {
                position = _transforms[i].position,
                velocity = velocities[i],
                mass = masses[i]
            };
        }

        ComputeBuffer computeBuffer = new ComputeBuffer(bodiesData.Length, Body.GetSize());
        
        computeBuffer.SetData(bodiesData);
        parallelShader.SetBuffer(0, "bodies", computeBuffer);
        parallelShader.SetInt("bodiesCount", bodiesData.Length);
        parallelShader.SetFloat("gravityConstant", gravityConstant);
        parallelShader.SetFloat("constraint", minDistance);
        parallelShader.SetFloat("dt", 1/ 144f);
        
        var groupsCount = Mathf.FloorToInt(bodiesData.Length / 128f);
        parallelShader.Dispatch(0,groupsCount, 1, 1);
        
        computeBuffer.GetData(bodiesData);
        for (int i = 0; i < bodiesData.Length; i++)
        {
            _transforms[i].position += bodiesData[i].velocity;
            velocities[i] = bodiesData[i].velocity;
        }
        
        computeBuffer.Dispose();
    }

    private void UpdateForces()
    {
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            Vector3 force = Vector3.zero;
            for (int j = 0; j < bodiesToSimulate; j++)
            {
                if (i != j)
                {
                    var delta = _transforms[j].position - _transforms[i].position;
                    var distance = Mathf.Max(delta.magnitude, minDistance);
                    force += delta * (10 * 10 / Mathf.Pow(distance, 3));
                }
            }

            velocities[i] += force / 10 * (gravityConstant * Time.deltaTime);
        }
    }

    public void InitializeFromFile(List<DataHandler.BodyInput> inputs)
    {
        Reset();
        bodiesToSimulate = inputs.Count;
        velocities = new NativeArray<Vector3>(bodiesToSimulate, Allocator.Persistent);
        masses = new NativeArray<float>(bodiesToSimulate, Allocator.Persistent);
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            var body = Instantiate(bodyPrefab, inputs[i].position, Quaternion.identity, container);
            body.transform.localScale = 0.1f * inputs[i].mass * Vector3.one;
            _transforms.Add(body.transform);
            masses[i] = inputs[i].mass;
            velocities[i] = inputs[i].velocity;
        }
        
        StartSimulation();
    }

    public void Initialize(int bodyCount, float spawnRange, float velocityRange, float minMass, float maxMass)
    {
        
        Reset();
        bodiesToSimulate = bodyCount;
        velocities = new NativeArray<Vector3>(bodiesToSimulate, Allocator.Persistent);
        masses = new NativeArray<float>(bodiesToSimulate, Allocator.Persistent);
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            var position = Random.insideUnitSphere * spawnRange;
            var mass = Random.Range(minMass, maxMass);
            var velocity = Random.insideUnitSphere * Random.Range(0f,velocityRange);
            var body = Instantiate(bodyPrefab, position, Quaternion.identity, container);
            _transforms.Add(body.transform);
            body.transform.localScale = 0.1f * mass * Vector3.one;
            masses[i] = mass;
            velocities[i] = velocity;
        }
        
        StartSimulation();
    }

    public void UpdateGeneralSettings(int threadCount, float gravityConst, float distanceConstraint, bool useComputeShader)
    {
        JobsUtility.JobWorkerCount = threadCount;
        gravityConstant = gravityConst;
        minDistance = distanceConstraint;
        this.useComputeShader = useComputeShader;
    }

    private void UpdateBodies(NativeArray<Vector3> velocities)
    {
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            _transforms[i].position += velocities[i];
        }
    }


    private struct Body
    {
        public Vector3 position;
        public Vector3 velocity;
        public float mass;

        public static int GetSize()
        {
            return sizeof(float) * 7;
        }
    }
}