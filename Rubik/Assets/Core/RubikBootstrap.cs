// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CodeGamified.Camera;
using CodeGamified.Time;
using CodeGamified.Settings;
using CodeGamified.Quality;
using CodeGamified.Bootstrap;
using Rubik.Game;
using Rubik.Scripting;
using Rubik.UI;

namespace Rubik.Core
{
    /// <summary>
    /// Bootstrap for Rubik's Cube — code-controlled 3×3 puzzle solver.
    ///
    /// Architecture (same pattern as all CodeGamified games):
    ///   - Instantiate managers → wire cross-references → configure scene
    ///   - .engine submodule gives us TUI + Code Execution for free
    ///   - Players WRITE CODE to rotate faces and solve the cube
    ///   - "Unit test" your solver by watching it run at 100x speed
    ///
    /// Attach to a GameObject. Press Play → Rubik's Cube appears.
    /// </summary>
    public class RubikBootstrap : GameBootstrap, IQualityResponsive
    {
        protected override string LogTag => "RUBIK";

        // =================================================================
        // INSPECTOR
        // =================================================================

        [Header("Cube")]
        [Tooltip("Number of random moves for scramble")]
        public int scrambleMoves = 20;

        [Header("Match")]
        [Tooltip("Auto-restart after solve")]
        public bool autoRestart = true;

        [Tooltip("Delay before restarting (sim-seconds)")]
        public float restartDelay = 3f;

        [Header("Time")]
        [Tooltip("Enable time scale modulation for fast testing")]
        public bool enableTimeScale = true;

        [Header("Scripting")]
        [Tooltip("Enable code execution (.engine)")]
        public bool enableScripting = true;

        [Header("Camera")]
        public bool configureCamera = true;

        // =================================================================
        // RUNTIME REFERENCES
        // =================================================================

        private RubikCube _cube;
        private RubikRenderer _renderer;
        private RubikMatchManager _match;
        private RubikProgram _playerProgram;

        // TUI
        private RubikTUIManager _tuiManager;

        // Camera
        private CameraAmbientMotion _cameraSway;

        // Post-processing
        private Bloom _bloom;
        private Volume _postProcessVolume;

        // =================================================================
        // UPDATE
        // =================================================================

        private void Update()
        {
            UpdateBloomScale();
        }

        private void UpdateBloomScale()
        {
            if (_bloom == null || !_bloom.active) return;
            var cam = Camera.main;
            if (cam == null) return;
            float dist = Vector3.Distance(cam.transform.position, Vector3.zero);
            float defaultDist = 6f;
            float scale = Mathf.Clamp01(defaultDist / Mathf.Max(dist, 0.01f));
            _bloom.intensity.value = Mathf.Lerp(0.5f, 1.0f, scale);
        }

        // =================================================================
        // BOOTSTRAP
        // =================================================================

        private void Start()
        {
            Log("🧊 Rubik Bootstrap starting...");

            // 1. Settings + Quality
            SettingsBridge.Load();
            QualityBridge.SetTier((QualityTier)SettingsBridge.QualityLevel);
            QualityBridge.Register(this);
            Log($"Settings loaded (Quality={SettingsBridge.QualityLevel}, Font={SettingsBridge.FontSize}pt)");

            // 2. Simulation time
            SetupSimulationTime();

            // 3. Camera + post-processing
            SetupCamera();

            // 4. Domain objects (cube)
            CreateCube();

            // 5. Match manager (needs cube)
            CreateMatchManager();

            // 6. Visual renderer (needs cube)
            CreateRenderer();

            // 7. Input provider
            CreateInputProvider();

            // 8. Player program (needs match + cube)
            if (enableScripting) CreatePlayerProgram();

            // 9. TUI
            CreateTUI();

            // 10. Wire events + start
            WireEvents();
            StartCoroutine(RunBootSequence());
        }

        public void OnQualityChanged(QualityTier tier)
        {
            Log($"Quality changed → {tier}");
        }

        // =================================================================
        // SIMULATION TIME
        // =================================================================

        private void SetupSimulationTime()
        {
            EnsureSimulationTime<RubikSimulationTime>();
        }

        // =================================================================
        // CAMERA — 3D perspective view orbiting the cube
        // =================================================================

        private void SetupCamera()
        {
            if (!configureCamera) return;

            var cam = EnsureCamera();

            cam.orthographic = false;
            cam.fieldOfView = 50f;
            // Position to see 3 faces of the cube (front, top, right)
            cam.transform.position = new Vector3(3.5f, 3f, -5f);
            cam.transform.LookAt(Vector3.zero, Vector3.up);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // Ambient sway — orbit around the cube
            _cameraSway = cam.gameObject.AddComponent<CameraAmbientMotion>();
            _cameraSway.lookAtTarget = Vector3.zero;

            // Post-processing: bloom (neon glow)
            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
                camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            var volumeGO = new GameObject("PostProcessVolume");
            _postProcessVolume = volumeGO.AddComponent<Volume>();
            _postProcessVolume.isGlobal = true;
            _postProcessVolume.priority = 1;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _bloom = profile.Add<Bloom>();
            _bloom.threshold.overrideState = true;
            _bloom.threshold.value = 0.8f;
            _bloom.intensity.overrideState = true;
            _bloom.intensity.value = 1.0f;
            _bloom.scatter.overrideState = true;
            _bloom.scatter.value = 0.5f;
            _bloom.clamp.overrideState = true;
            _bloom.clamp.value = 20f;
            _bloom.highQualityFiltering.overrideState = true;
            _bloom.highQualityFiltering.value = true;
            _postProcessVolume.profile = profile;

            Log("Camera: perspective, FOV=50, 3D orbit view + sway + bloom");
        }

        // =================================================================
        // CUBE
        // =================================================================

        private void CreateCube()
        {
            var go = new GameObject("RubikCube");
            _cube = go.AddComponent<RubikCube>();
            _cube.Initialize();
            Log("Created Cube (3×3×3, 54 stickers)");
        }

        // =================================================================
        // MATCH MANAGER
        // =================================================================

        private void CreateMatchManager()
        {
            var go = new GameObject("MatchManager");
            _match = go.AddComponent<RubikMatchManager>();
            _match.Initialize(_cube, scrambleMoves, autoRestart, restartDelay);
            Log($"Created MatchManager (scramble={scrambleMoves} moves)");
        }

        // =================================================================
        // RENDERER
        // =================================================================

        private void CreateRenderer()
        {
            _renderer = _cube.gameObject.AddComponent<RubikRenderer>();
            _renderer.Initialize(_cube);
            Log("Created Renderer (3D cubies with colored sticker quads)");
        }

        // =================================================================
        // INPUT PROVIDER
        // =================================================================

        private void CreateInputProvider()
        {
            var go = new GameObject("InputProvider");
            go.AddComponent<RubikInputProvider>();
            Log("Created RubikInputProvider (keyboard → face rotations)");
        }

        // =================================================================
        // PLAYER SCRIPTING (.engine powered)
        // =================================================================

        private void CreatePlayerProgram()
        {
            var go = new GameObject("PlayerProgram");
            _playerProgram = go.AddComponent<RubikProgram>();
            _playerProgram.Initialize(_match, _cube);
            Log("Created PlayerProgram (code-controlled Rubik solver)");
        }

        // =================================================================
        // TUI (.engine powered)
        // =================================================================

        private void CreateTUI()
        {
            var go = new GameObject("RubikTUI");
            _tuiManager = go.AddComponent<RubikTUIManager>();
            _tuiManager.Initialize(_match, _playerProgram);
            Log("Created TUI (left debugger + right status panel)");
        }

        // =================================================================
        // EVENT WIRING
        // =================================================================

        private void WireEvents()
        {
            if (SimulationTime.Instance != null)
            {
                SimulationTime.Instance.OnTimeScaleChanged += s => Log($"Time scale → {s:F0}x");
                SimulationTime.Instance.OnPausedChanged += p => Log(p ? "⏸ PAUSED" : "▶ RESUMED");
            }

            if (_match != null)
            {
                _match.OnMatchStarted += () =>
                {
                    Log("MATCH STARTED — Cube scrambled, solve it!");
                    _renderer?.MarkDirty();
                };

                _match.OnScoreChanged += score =>
                {
                    Log($"Score: {score} │ Stickers: {_cube.SolvedStickerCount()}/54 │ Faces: {_cube.SolvedFaceCount()}/6");
                    _renderer?.MarkDirty();
                };

                _match.OnSolved += () =>
                {
                    Log($"🎉 SOLVED in {_cube.MoveCount} moves! │ Score: {_match.Score}");
                };

                _match.OnGameOver += () =>
                {
                    Log($"GAME OVER │ Score: {_match.Score} │ Moves: {_cube.MoveCount}");
                    if (autoRestart)
                        StartCoroutine(RestartAfterDelay());
                };

                _match.OnCubeChanged += () => _renderer?.MarkDirty();
            }
        }

        // =================================================================
        // BOOT SEQUENCE
        // =================================================================

        private IEnumerator RunBootSequence()
        {
            yield return null;
            yield return null;

            LogDivider();
            Log("🧊 RUBIK'S CUBE — Code Your Solver");
            LogDivider();
            LogStatus("SCRAMBLE", $"{scrambleMoves} moves");
            LogEnabled("SCRIPTING", enableScripting);
            LogEnabled("TIME SCALE", enableTimeScale);
            LogEnabled("AUTO RESTART", autoRestart);
            LogDivider();

            _match.StartMatch();
            Log("Cube scrambled — GO!");
        }

        private IEnumerator RestartAfterDelay()
        {
            float waited = 0f;
            while (waited < restartDelay)
            {
                if (SimulationTime.Instance != null && !SimulationTime.Instance.isPaused)
                    waited += Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);
                yield return null;
            }

            _match.StartMatch();
            _playerProgram?.ResetExecution();
            Log("Match restarted — new scramble");
        }

        private void OnDestroy()
        {
            QualityBridge.Unregister(this);
        }
    }
}
