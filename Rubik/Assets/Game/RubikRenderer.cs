// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeGamified.Procedural;

namespace Rubik.Game
{
    /// <summary>
    /// Visual renderer + queue-based animator for the Rubik's Cube.
    /// Builds cubie geometry via ProceduralAssembler. Animates face rotations
    /// with pivot parenting. No state logic — purely cosmetic.
    /// </summary>
    public class RubikRenderer : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════════
        // CONFIG
        // ═══════════════════════════════════════════════════════════════

        private const float CubieSize = 0.9f;
        private const float StickerSize = 0.8f;
        private const float StickerOffset = 0.451f;
        private const float AnimDuration = 0.25f;

        // ═══════════════════════════════════════════════════════════════
        // STATE
        // ═══════════════════════════════════════════════════════════════

        private RubikCube _cube;
        private ColorPalette _palette;
        private Shader _shader;

        // Visual GO for each cubie: keyed by RubikCubie instance
        private Dictionary<RubikCubie, GameObject> _visuals = new();

        // Animation queue
        private readonly Queue<AnimRequest> _queue = new();
        private bool _animating;

        public bool IsBusy => _animating || _queue.Count > 0;

        struct AnimRequest
        {
            public RubikCubie[] Cubies;
            public Vector3 Axis;
            public float Angle;
        }

        // ═══════════════════════════════════════════════════════════════
        // INIT
        // ═══════════════════════════════════════════════════════════════

        public void Initialize(RubikCube cube)
        {
            _cube = cube;
            _palette = BuildPalette();
            _shader = FindShader();

            BuildVisuals();

            _cube.OnFaceRotating += OnFaceRotating;
            _cube.OnCubeChanged += OnCubeChanged;
        }

        private void OnDestroy()
        {
            if (_cube != null)
            {
                _cube.OnFaceRotating -= OnFaceRotating;
                _cube.OnCubeChanged -= OnCubeChanged;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // VISUAL CONSTRUCTION (ProceduralAssembler)
        // ═══════════════════════════════════════════════════════════════

        private void BuildVisuals()
        {
            // Destroy previous visuals
            foreach (var kv in _visuals)
                if (kv.Value != null) Destroy(kv.Value);
            _visuals.Clear();

            int n = RubikCube.N;
            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                    for (int z = 0; z < n; z++)
                    {
                        var cubie = _cube.GetCubieAt(x, y, z);
                        if (cubie == null) continue;

                        var blueprint = new CubieBlueprint(cubie, x, y, z);
                        var result = ProceduralAssembler.Build(blueprint, _palette, _shader);
                        if (result.Root == null) continue;

                        result.Root.transform.SetParent(transform, false);
                        result.Root.transform.localPosition = RubikCube.GridToLocal(x, y, z);
                        _visuals[cubie] = result.Root;
                    }
        }

        // ═══════════════════════════════════════════════════════════════
        // CUBIE BLUEPRINT (IProceduralBlueprint)
        // ═══════════════════════════════════════════════════════════════

        private class CubieBlueprint : IProceduralBlueprint
        {
            private readonly RubikCubie _cubie;
            private readonly int _x, _y, _z;

            public CubieBlueprint(RubikCubie cubie, int x, int y, int z)
            {
                _cubie = cubie;
                _x = x; _y = y; _z = z;
            }

            public string DisplayName => $"Cubie_{_x}_{_y}_{_z}";
            public string PaletteId => "rubik";
            public ProceduralLODHint LODHint => ProceduralLODHint.Lightweight;

            public ProceduralPartDef[] GetParts()
            {
                var parts = new List<ProceduralPartDef>();

                // Body — dark cube
                parts.Add(new ProceduralPartDef("body", PrimitiveType.Cube,
                    Vector3.zero, Vector3.one * CubieSize, "cubie_black"));

                // Stickers — colored quads on exposed faces
                int n = RubikCube.N;
                int last = n - 1;
                int idx = 0;

                void TrySticker(bool condition, Vector3 normal, int faceIndex)
                {
                    if (!condition) return;
                    var rot = QuadRotation(normal);
                    parts.Add(new ProceduralPartDef(
                        $"sticker_{idx++}", PrimitiveType.Quad,
                        normal * StickerOffset,
                        new Vector3(StickerSize, StickerSize, 1f),
                        rot,
                        RubikCube.FaceColorKeys[faceIndex],
                        "sticker", ColliderMode.None, "body"));
                }

                TrySticker(_y == last, Vector3.up, RubikCube.U);
                TrySticker(_y == 0,    Vector3.down, RubikCube.D);
                TrySticker(_z == last, Vector3.forward, RubikCube.F);
                TrySticker(_z == 0,    Vector3.back, RubikCube.B);
                TrySticker(_x == 0,    Vector3.left, RubikCube.L);
                TrySticker(_x == last, Vector3.right, RubikCube.R);

                return parts.ToArray();
            }
        }

        /// <summary>
        /// Rotation to make a quad face the given direction.
        /// Unity Quad default normal is +Z (forward).
        /// </summary>
        private static Quaternion QuadRotation(Vector3 normal)
        {
            if (normal == Vector3.up)      return Quaternion.Euler(-90f, 0f, 0f);
            if (normal == Vector3.down)    return Quaternion.Euler(90f, 0f, 0f);
            if (normal == Vector3.forward) return Quaternion.identity;
            if (normal == Vector3.back)    return Quaternion.Euler(0f, 180f, 0f);
            if (normal == Vector3.left)    return Quaternion.Euler(0f, -90f, 0f);
            if (normal == Vector3.right)   return Quaternion.Euler(0f, 90f, 0f);
            return Quaternion.LookRotation(normal);
        }

        // ═══════════════════════════════════════════════════════════════
        // ANIMATION
        // ═══════════════════════════════════════════════════════════════

        private void OnFaceRotating(int face, int direction, RubikCubie[] layerCubies, Vector3 axis)
        {
            _queue.Enqueue(new AnimRequest
            {
                Cubies = layerCubies,
                Axis = axis,
                Angle = direction * 90f
            });

            if (!_animating)
                StartCoroutine(ProcessQueue());
        }

        private void OnCubeChanged()
        {
            // Snap positions after state change (handles ResetToSolved).
            // Only snap when not animating — animation handles its own snapping.
            if (!_animating && _queue.Count == 0)
                RebuildIfNeeded();
        }

        private void RebuildIfNeeded()
        {
            // Check if visuals still map to valid cubies; if not, full rebuild.
            int n = RubikCube.N;
            bool needsRebuild = false;
            for (int x = 0; x < n && !needsRebuild; x++)
                for (int y = 0; y < n && !needsRebuild; y++)
                    for (int z = 0; z < n && !needsRebuild; z++)
                    {
                        var cubie = _cube.GetCubieAt(x, y, z);
                        if (cubie != null && !_visuals.ContainsKey(cubie))
                            needsRebuild = true;
                    }

            if (needsRebuild)
                BuildVisuals();
        }

        private IEnumerator ProcessQueue()
        {
            _animating = true;
            while (_queue.Count > 0)
            {
                var req = _queue.Dequeue();
                yield return AnimateRotation(req);
            }
            _animating = false;
        }

        private IEnumerator AnimateRotation(AnimRequest req)
        {
            // Create pivot at cube center
            var pivot = new GameObject("Pivot");
            pivot.transform.SetParent(transform, false);
            pivot.transform.localPosition = Vector3.zero;

            // Parent visual GOs to pivot
            var movedVisuals = new List<(RubikCubie cubie, GameObject visual)>();
            for (int i = 0; i < req.Cubies.Length; i++)
            {
                var cubie = req.Cubies[i];
                if (cubie != null && _visuals.TryGetValue(cubie, out var visual))
                {
                    visual.transform.SetParent(pivot.transform, true);
                    movedVisuals.Add((cubie, visual));
                }
            }

            // Interpolate rotation
            Quaternion startRot = Quaternion.identity;
            Quaternion endRot = Quaternion.AngleAxis(req.Angle, req.Axis);
            float elapsed = 0f;

            while (elapsed < AnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / AnimDuration));
                pivot.transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            // Final rotation
            pivot.transform.localRotation = endRot;

            // Unparent and snap to grid target positions
            for (int i = 0; i < movedVisuals.Count; i++)
            {
                var (cubie, visual) = movedVisuals[i];
                visual.transform.SetParent(transform, true);
                // Snap to the cubie's new grid position
                visual.transform.localPosition = _cube.GetCubieTargetPosition(cubie);
                // Snap rotation to clean values (avoid floating-point drift)
                visual.transform.localRotation = SnapRotation(visual.transform.localRotation);
            }

            Destroy(pivot);
        }

        /// <summary>Snap euler angles to nearest 90° to prevent drift.</summary>
        private static Quaternion SnapRotation(Quaternion q)
        {
            Vector3 euler = q.eulerAngles;
            euler.x = Mathf.Round(euler.x / 90f) * 90f;
            euler.y = Mathf.Round(euler.y / 90f) * 90f;
            euler.z = Mathf.Round(euler.z / 90f) * 90f;
            return Quaternion.Euler(euler);
        }

        // ═══════════════════════════════════════════════════════════════
        // PALETTE + SHADER
        // ═══════════════════════════════════════════════════════════════

        private static ColorPalette BuildPalette()
        {
            return ColorPalette.CreateRuntime(new Dictionary<string, Color>
            {
                { "cubie_black", new Color(0.08f, 0.08f, 0.08f) },
                { "white",      Color.white },
                { "yellow",     Color.yellow },
                { "green",      new Color(0f, 0.8f, 0f) },
                { "blue",       new Color(0f, 0.3f, 1f) },
                { "orange",     new Color(1f, 0.5f, 0f) },
                { "red",        new Color(0.9f, 0f, 0f) },
            });
        }

        private static Shader FindShader()
        {
            var s = Shader.Find("Universal Render Pipeline/Unlit");
            if (s != null) return s;
            s = Shader.Find("Universal Render Pipeline/Lit");
            if (s != null) return s;
            return Shader.Find("Standard");
        }

        // ═══════════════════════════════════════════════════════════════
        // EMISSION PULSE (used by MatchManager start sequence)
        // ═══════════════════════════════════════════════════════════════

        public IEnumerator PulseEmission(float duration)
        {
            // Gather all sticker renderers
            var renderers = new List<Renderer>();
            foreach (var kv in _visuals)
            {
                if (kv.Value == null) continue;
                var childRenderers = kv.Value.GetComponentsInChildren<Renderer>();
                renderers.AddRange(childRenderers);
            }

            // Pulse: ramp emission up then down
            float half = duration * 0.5f;
            float elapsed = 0f;
            var block = new MaterialPropertyBlock();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed < half
                    ? elapsed / half
                    : 1f - (elapsed - half) / half;
                t = Mathf.Clamp01(t);

                Color emissive = Color.white * t * 2f;
                block.SetColor("_EmissionColor", emissive);

                for (int i = 0; i < renderers.Count; i++)
                {
                    if (renderers[i] != null)
                        renderers[i].SetPropertyBlock(block);
                }
                yield return null;
            }

            // Clear
            block.SetColor("_EmissionColor", Color.black);
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                    renderers[i].SetPropertyBlock(block);
            }
        }
    }
}
