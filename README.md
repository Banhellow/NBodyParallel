# NBodyParallel

An interactive gravitational N-body simulation in Unity that compares five ways of advancing the same system:

| Backend | How it works |
|---|---|
| **Sequential** | Plain O(N²) C# loop on the main thread. |
| **CPU Parallel** | Unity C# Job System: an `IJobParallelFor` computes forces, then an `IJobParallelForTransform` moves the bodies. |
| **GPU Parallel** | A compute shader (`BodyComputer.compute`) computes the new velocity of every body in parallel. |
| **Prediction** | A learned model predicts the next positions. The current state is sent over HTTP to a local inference server, without blocking the main thread. |
| **WS Prediction** | Same model, but the server streams predicted frames back over a WebSocket. |

The two prediction backends talk to the graph neural network server in the companion repository [Banhellow/GNN](https://github.com/Banhellow/GNN). The app can also record trajectories to JSON, and those recordings are the training data for that model.

## Requirements

- **Unity 6000.3.6f1** (Unity 6.3) or newer. This is the version the project was saved with.
- **Git** on your `PATH`. The `NativeWebSocket` package is pulled from a Git URL.
- A GPU with compute shader support, if you want to use the GPU backend.
- For the prediction backends: the GNN server running on `127.0.0.1:8000` (see [Banhellow/GNN](https://github.com/Banhellow/GNN)).

Packages, which Unity resolves automatically from `Packages/manifest.json`:

- Burst
- Collections
- Newtonsoft JSON
- uGUI / TextMesh Pro
- [NativeWebSocket](https://github.com/endel/NativeWebSocket)

## Getting started

1. Clone the repository and open the folder in Unity Hub with Unity 6000.3.6f1.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press **Play**.
4. Pick a backend in the backend dropdown (Sequential, CPU Parallel, GPU Parallel, Prediction, WS Prediction).
5. Start a simulation from the **Random** or **File** panel.

## Using the app

### Panels

- **General**: gravity constant, worker thread count and radius constraint (the minimum distance used in the force calculation, which keeps close encounters from blowing up). Press **Update** to apply. The new gravity and radius values take effect the next time you press **Start**.
- **Random**: spawns bodies at random inside a sphere. You can set the body count, spawn radius, initial speed range and mass range.
- **File**: loads bodies from a CSV file (format below).

A second dropdown loads a preset into all three panels. Presets are ScriptableObjects in `Assets/Settings/`:

| Preset | Bodies | Gravity | Radius constraint | Spawn radius | Speed range | Mass range |
|---|---|---|---|---|---|---|
| Default | 1000 | 1 | 10 | 20 | 0–0.01 | 1–2 |
| High Speed | 1000 | 1 | 10 | 20 | 0–20 | 5–10 |
| Solar System | 2000 | 1 | 30 | 20 | 0–20 | 1–20 (file: `solar`) |

All presets set the worker thread count to 1. Raise it in the **General** panel before comparing the CPU Parallel backend.

You can create more presets with **Create → Settings → Default**.

The overlay in the top-left corner shows:

- the number of job worker threads
- milliseconds per frame
- the time spent in the simulation step alone
- FPS

### Camera

| Input | Action |
|---|---|
| Mouse wheel | Zoom in / out |
| Hold right mouse button | Pan in the direction of the cursor |
| Hold middle mouse button | Rotate horizontally |

### CSV input format

Each body takes seven comma-separated values:

```
mass, x, y, z, vx, vy, vz,
```

- Whitespace and line breaks are ignored.
- Mass is read as an integer.
- Velocity is in units per frame.

The **File** panel accepts two kinds of input:

- a full file path
- the name of a text asset under `Assets/Resources`, without the extension. For example, `solar` loads `Assets/Resources/solar.csv`.

## Prediction backends

Start the server from the GNN repository first. It listens on `127.0.0.1:8000`, and the URLs are hard-coded in the backends.

- **Prediction (HTTP)** simulates the first 7 frames locally. After that, it sends `POST /predict` with each body's last 7 positions and keeps exactly one request in flight. The bodies move when a response arrives, so the frame rate stays independent of the request time. If a request fails, the backend logs the error and stops predicting.
- **WS Prediction** simulates 7 frames up front and sends them in a single `init` message to `ws://127.0.0.1:8000/ws`. From then on it plays back the frames the server streams, one per Unity frame.

The bundled model was trained on bodies with mass `1`, and masses are sent to it as integer "particle types" in the range 0–4. When using these backends, set the mass range to **1–1** in the Random panel. Larger masses fall outside the model's type embedding.

## Recording a training dataset

1. Select the `NBodyController` object in the scene.
2. Assign a dataset asset to its **Dataset Settings** field. Two are included in `Assets/Settings/Dataset/`: `train_datset` (10,000 frames) and `validation_dataset` (2,000 frames). You can create more with **Create → Settings → Dataset**.
3. Change the asset's **Dataset Path**. It defaults to `D:\`.
4. Run a simulation. Positions are recorded every frame until the frame limit is reached.
5. Leave Play mode. The JSON file is written to `Dataset Path / Dataset Name` when the controller is destroyed.

Output format:

```json
{ "entries": [ { "mass": 1, "positions": [[x, y, z], [x, y, z], ...] }, ... ] }
```

## Running the tests

The project uses the Unity Test Framework. Open **Window → General → Test Runner**, select **EditMode** and press **Run All**.

- `GravityMathTests` checks the shared force function: direction, inverse-square falloff, mass scaling, the minimum-distance clamp, and momentum conservation.
- `BackendConsistencyTests` runs the same random system through several backends and checks that CPU Parallel and GPU Parallel end up where Sequential does. It also covers body counts that don't fill a GPU thread group and counts smaller than the number of worker threads. The GPU tests are skipped on machines without compute shader support.

## Design decisions

- **One force formula.** `GravityMath.Acceleration` is used by the Sequential and prediction backends and, through Burst, by the CPU job. The compute shader mirrors it in HLSL, and the consistency tests make sure they agree.
- **A fixed timestep for every backend.** All backends use the same `TimeStep`, so they simulate the same system whatever the frame rate. Velocities are in units per frame: each step adds `acceleration × TimeStep` to the velocity, then adds the velocity to the position.
- **The GPU backend still reads results back every frame.** Its buffers are created once and reused, but each body is a GameObject, so positions have to come back to the CPU. Rendering straight from a GPU buffer would remove the readback. At 1–2k bodies the O(N²) force calculation is expected to dominate, and GPU instancing on the body material already keeps draw calls low, so that rewrite only pays off if profiling shows the readback or rendering as the bottleneck.
- **The HTTP prediction backend never blocks the main thread.** A synchronous request every frame would tie the frame rate to the server's latency, which on mobile is the pattern that leads to ANRs.

Known limitations and next steps:

- The integrator is semi-implicit Euler, which slowly drifts in energy. Velocity Verlet would be more accurate.
- The prediction server URLs are hard-coded.
- One GameObject per body limits how many bodies the scene can handle.

## Project structure

```
Assets/
  Scenes/SampleScene.unity          Main scene
  Scripts/
    NBody.Runtime.asmdef            Runtime assembly definition
    NBodyController.cs              Spawning, backend selection, HUD
    SimulationBackendFactory.cs     Creates a backend from SimulationType
    SimulationBackend/              Sequential, ParallelCpu, ParallelGpu, PredictionModel, WSPredictionModel, GravityMath
    BodyJob.cs, UpdateJob.cs        Job System jobs for the CPU backend
    BodyComputer.compute            Compute shader for the GPU backend
    Communication/                  WebSocket connection and frame queue
    Requests/, Responses/           JSON contracts shared with the GNN server
    Dataset/                        Trajectory recorder and its settings
    UI/                             Settings panels and inputs
  Tests/EditMode/                   Edit mode tests
  Settings/                         Presets and dataset settings
  Resources/solar.csv               Sample input file
```

## License

No license file has been added yet.
