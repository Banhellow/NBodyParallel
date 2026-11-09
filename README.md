# NBodyParallel

An interactive gravitational N-body simulation in Unity that compares five ways of advancing the same system:

| Backend | How it works |
|---|---|
| **Sequential** | Plain O(N²) C# loop on the main thread. |
| **CPU Parallel** | Unity C# Job System: an `IJobParallelFor` computes forces, then an `IJobParallelForTransform` moves the bodies. |
| **GPU Parallel** | A compute shader (`BodyComputer.compute`) computes the new velocity of every body in parallel. |
| **Prediction** | A learned model predicts the next positions. Each frame is sent over HTTP to a local inference server. |
| **WS Prediction** | Same model, but the server streams predicted frames back over a WebSocket. |

The two prediction backends talk to the graph neural network server in the companion repository [Banhellow/GNN](https://github.com/Banhellow/GNN). The app can also record trajectories to JSON, and those recordings are the training data for that model.

## Requirements

- **Unity 6000.0.23f1** (Unity 6). Other Unity 6 versions will probably work, but this is the one the project was saved with.
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

1. Clone the repository and open the folder in Unity Hub with Unity 6000.0.23f1.
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

| Preset | Bodies | Gravity | Radius constraint | Mass range |
|---|---|---|---|---|
| Default | 1000 | 0.002 | 10 | 1–20 |
| Solar | 2000 | 0.5 | 30 | 1–20 (file: `solar`) |
| High Speed | 1000 | 1 | 10 | 5–100 |

You can create more presets with **Create → Settings → Default**.

The overlay in the top-left corner shows:

- the number of job worker threads
- milliseconds per frame
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

- **Prediction (HTTP)** simulates the first 7 frames locally. After that, every frame sends `POST /predict` with each body's last 7 positions. The call is synchronous, so the frame rate is bound by the request time.
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

## Project structure

```
Assets/
  Scenes/SampleScene.unity          Main scene
  Scripts/
    NBodyController.cs              Spawning, backend selection, HUD
    SimulationBackendFactory.cs     Creates a backend from SimulationType
    SimulationBackend/              Sequential, ParallelCpu, ParallelGpu, PredictionModel, WSPredictionModel
    BodyJob.cs, UpdateJob.cs        Job System jobs for the CPU backend
    BodyComputer.compute            Compute shader for the GPU backend
    Communication/                  WebSocket connection and frame queue
    Requests/, Responses/           JSON contracts shared with the GNN server
    Dataset/                        Trajectory recorder and its settings
    UI/                             Settings panels and inputs
  Settings/                         Presets and dataset settings
  Resources/solar.csv               Sample input file
```

## License

No license file has been added yet.
