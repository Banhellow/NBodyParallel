using System.Collections.Generic;
using System.Diagnostics;
using Dataset;
using SimulationBackend;
using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Random = UnityEngine.Random;


public class NBodyController : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private int bodiesToSimulate;
    [SerializeField] private float gravityConstant;
    [SerializeField] private float minDistance;
    [SerializeField] private GravityBody bodyPrefab;
    [SerializeField] private DatasetSettings datasetSettings;
    [SerializeField] private SimulationType simulationType;
    [SerializeField] private TMP_Dropdown dropdown;
    private readonly List<Transform> _transforms = new();
    private bool simulationEnabled;
    private bool shouldRecordData;
    
    //Performance counting
    private double simulationMs;
    private float deltaTime;
    private float framesCount;
    private SimulationBackendBase simulation;
    private SimulationBackendFactory simulationBackendFactory;
    private DatasetRecorder datasetRecorder;
    
    [SerializeField] private ComputeShader parallelShader;

    void Start()
    {
        Application.targetFrameRate = 60;
        simulationBackendFactory = new SimulationBackendFactory();
        shouldRecordData = datasetSettings != null;
        Reset();
        dropdown.onValueChanged.AddListener(OnBackendDropdownChanged);
    }

    void Reset()
    {
        ResetBodies();
        simulation?.Dispose();
        simulation = null;
        simulationMs = 0;
    }
    
    public void OnBackendDropdownChanged(int value)
    {
        simulationType = (SimulationType)value;
    }

    private void ResetBodies()
    {
        // Iterate the spawned bodies, not bodiesToSimulate: the serialized count can differ
        // from what is actually in the scene.
        foreach (var body in _transforms)
        {
            Destroy(body.gameObject);
        }

        _transforms.Clear();
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnBackendDropdownChanged);
        // simulation is null if Play mode ends before a simulation was started.
        simulation?.Dispose();
        datasetRecorder?.TrySaveDataSet();
    }

    void Update()
    {
        if (simulationEnabled == false)
        {
            return;
        }
        
        var stopwatch = Stopwatch.StartNew();
        simulation.SimulateFrame();
        stopwatch.Stop();
        simulationMs = stopwatch.Elapsed.TotalMilliseconds;

        if (shouldRecordData)
        {
            datasetRecorder.Record(_transforms);
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
        string text = $"Threads:{JobsUtility.JobWorkerCount}, MS/frame ({msec:0.0} ms), simulation step: {simulationMs:0.00} ms, {fpsRounded:0.} FPS";
        GUI.Label(rect, text, style);
    }

    public void InitializeFromFile(List<DataHandler.BodyInput> inputs)
    {
        Reset();

        bodiesToSimulate = inputs.Count;
        var velocities = new Vector3[bodiesToSimulate];
        var masses = new float[bodiesToSimulate];
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            var body = Instantiate(bodyPrefab, inputs[i].position, Quaternion.identity, container);
            body.transform.localScale = 0.1f * inputs[i].mass * Vector3.one;
            _transforms.Add(body.transform);
            masses[i] = inputs[i].mass;
            velocities[i] = inputs[i].velocity;
        }

        StartSimulation(velocities, masses);
    }

    public void Initialize(int bodyCount, float spawnRange, float velocityRange, float minMass, float maxMass)
    {
        
        Reset();
        bodiesToSimulate = bodyCount;

        var velocities = new Vector3[bodiesToSimulate];
        var masses = new float[bodiesToSimulate];
        for (int i = 0; i < bodiesToSimulate; i++)
        {
            var position = Random.insideUnitSphere * spawnRange;
            var mass = Random.Range(minMass, maxMass);
            var velocity = Random.insideUnitSphere * Random.Range(0f,velocityRange);
            var body = Instantiate(bodyPrefab, position, Quaternion.identity, container);
            _transforms.Add(body.transform);
            body.transform.localScale = 0.1f * 20 * Vector3.one;
            masses[i] = mass;
            velocities[i] = velocity;
        }

        StartSimulation(velocities, masses);
    }

    private void StartSimulation(Vector3[] velocities, float[] masses)
    {
        if (datasetSettings != null)
        {
            datasetRecorder = new DatasetRecorder(datasetSettings);
            datasetRecorder.Initialize(bodiesToSimulate, masses);
        }

        simulation = simulationBackendFactory.Create(simulationType, _transforms, minDistance, gravityConstant, parallelShader);
        simulation.Initialize(velocities, masses);
        simulationEnabled = true;
    }

    public void UpdateGeneralSettings(int threadCount, float gravityConst, float distanceConstraint)
    {
        // Setting a value outside [0, JobWorkerMaximumCount] throws.
        JobsUtility.JobWorkerCount = Mathf.Clamp(threadCount, 0, JobsUtility.JobWorkerMaximumCount);
        gravityConstant = gravityConst;
        minDistance = distanceConstraint;
    }
}