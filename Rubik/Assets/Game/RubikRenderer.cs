// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CodeGamified.Quality;
using CodeGamified.Time;

namespace Rubik.Game
{
    /// <summary>
    /// 3D Rubik's Cube renderer with smooth face-rotation animation.
    ///
    /// ARCHITECTURE — two decoupled representations kept in sync:
    ///
    ///   LOGICAL (RubikCube): 54 ints in _stickers[face, pos].
    ///     Permuted instantly by RotateFace().
    ///
    ///   PHYSICAL (this class): 26 cubie GameObjects on a 3×3×3 grid.
    ///     Each cubie has 1-3 sticker-quad children whose material color
    ///     is the *truth* for what the player sees.
    ///
    /// On a face rotation:
    ///   1. OnFaceRotating fires BEFORE the logical permutation.
    ///   2. We reparent the 9 layer cubies under a pivot and animate a
    ///      90° rotation.  Sticker quads ride with their cubies — fully
    ///      visible throughout.
    ///   3. After the animation completes we snap cubies to the grid,
    ///      reset their rotations to identity, and recreate sticker quads
    ///      from the logical state (already permuted).  Single-sided quads
    ///      face wrong directions after physical rotation, so repainting
    ///      is required.
    ///
    /// On a non-animated state change (scramble / reset) the
    /// OnCubeChanged event fires and we do a full rebuild: destroy all
    /// cubie GameObjects and recreate them from scratch with the correct
    /// colors read from the logical state.  This is cheap (happens once
    /// per scramble, not per frame) and avoids any index desync.
    /// </summary>
    public class RubikRenderer : MonoBehaviour, IQualityResponsive
    {
        private RubikCube _cube;

        public const float CubeSize = 1.0f;
        private const float Gap = 0.05f;
        private const float StickerInset = 0.02f;

        // 3×3×3 grid index; rebuilt from world positions after each animation.
        private GameObject[,,] _cubies;

        /// <summary>True while a face-rotation animation is playing.</summary>
        public bool IsAnimating => _animating;

        // ── Animation state ──
        private const float BaseAnimDuration = 0.2f;
        private bool _animating;
        private float _animElapsed;
        private float _animDuration;
        private Vector3 _animAxis;
        private float _animTargetAngle;
        private Transform _animPivot;
        private readonly List<Transform> _animCubies = new List<Transform>();

        // True when the logical state changed without animation (scramble/reset).
        // Cleared after a full rebuild.
        private bool _needsFullRebuild;
        // Set by OnFaceRotating, consumed by OnCubeChanged, to distinguish
        // the animated RotateFace's OnCubeChanged from a scramble/reset.
        private bool _expectingAnimatedChange;

        // ── Colors ──
        private static readonly Color[] StickerColors =
        {
            new Color(1f, 1f, 1f),                  // U white
            new Color(1f, 1f, 0f),                  // D yellow
            new Color(0f, 1f, 0.3f),                // F green
            new Color(0.3f, 0.5f, 1f),              // B blue
            new Color(1f, 0.6f, 0f),                // L orange
            new Color(1f, 0.1f, 0.1f),              // R red
        };
        private static readonly Color CubieColor = new Color(0.05f, 0.05f, 0.08f);

        // ═══════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ═══════════════════════════════════════════════════════════════

        public void Initialize(RubikCube cube)
        {
            _cube = cube;

            var pivotGO = new GameObject("RotationPivot");
            pivotGO.transform.SetParent(transform, false);
            pivotGO.transform.localPosition = Vector3.zero;
            _animPivot = pivotGO.transform;

            _cubies = new GameObject[3, 3, 3];
            BuildCubeFromState();

            _cube.OnFaceRotating += OnFaceRotating;
            _cube.OnCubeChanged += OnCubeChanged;
            QualityBridge.Register(this);
        }

        private void OnDisable() => QualityBridge.Unregister(this);
        public void OnQualityChanged(QualityTier tier) { }

        private void Update()
        {
            if (_animating) UpdateAnimation();
        }

        private void LateUpdate()
        {
            if (_animating) return;
            if (!_needsFullRebuild) return;
            _needsFullRebuild = false;
            FullRebuild();
        }

        public void MarkDirty() => _needsFullRebuild = true;

        // ═══════════════════════════════════════════════════════════════
        // CUBE CHANGED (non-animated: scramble, reset, external)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// OnCubeChanged fires after EVERY state change, including animated
        /// rotations.  We only want a full rebuild for non-animated changes.
        /// Animated rotations are self-contained: the physical state matches
        /// the logical state after FinishAnimation, no repaint needed.
        /// </summary>
        private void OnCubeChanged()
        {
            if (_expectingAnimatedChange)
            {
                // This is the OnCubeChanged from a RotateFace that we're
                // already animating.  Physical state is handled by the
                // animation system — no rebuild needed.
                _expectingAnimatedChange = false;
                return;
            }
            _needsFullRebuild = true;
        }

        /// <summary>
        /// Destroy all cubies and recreate from the current logical state.
        /// Used after scramble / reset / any non-animated state change.
        /// </summary>
        private void FullRebuild()
        {
            DestroyAllCubies();
            BuildCubeFromState();
        }

        private void DestroyAllCubies()
        {
            if (_cubies == null) return;
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            for (int z = 0; z < 3; z++)
            {
                if (_cubies[x, y, z] != null)
                {
                    DestroyImmediate(_cubies[x, y, z]);
                    _cubies[x, y, z] = null;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ANIMATION — cubies + stickers rotate together, nothing hidden
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Called BEFORE the logical sticker permutation.  Cubies are still
        /// at their pre-move grid positions, so GatherLayerCubies picks the
        /// correct 9.
        /// </summary>
        private void OnFaceRotating(int face, int direction)
        {
            if (_animating)
                FinishAnimationInstant();
            // Mark that the next OnCubeChanged is from this animated rotation
            // and should NOT trigger a full rebuild.
            _expectingAnimatedChange = true;
            StartAnimation(face, direction);
        }

        private void StartAnimation(int face, int direction)
        {
            _animElapsed = 0f;

            float ts = SimulationTime.Instance?.timeScale ?? 1f;
            _animDuration = ts > 5f
                ? BaseAnimDuration / Mathf.Min(ts * 0.2f, 10f)
                : BaseAnimDuration;

            _animAxis = FaceAxis(face);
            bool flipSign = face == RubikCube.D || face == RubikCube.B || face == RubikCube.L;
            float cwAngle = flipSign ? 90f : -90f;
            _animTargetAngle = direction == 1 ? cwAngle : -cwAngle;

            _animPivot.localPosition = Vector3.zero;
            _animPivot.localRotation = Quaternion.identity;

            _animCubies.Clear();
            GatherLayerCubies(face, _animCubies);
            foreach (var c in _animCubies)
                c.SetParent(_animPivot, true);

            _animating = true;
        }

        private void UpdateAnimation()
        {
            _animElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_animElapsed / _animDuration);

            float eased = t < 0.5f
                ? 2f * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;

            _animPivot.localRotation =
                Quaternion.AngleAxis(eased * _animTargetAngle, _animAxis);

            if (t >= 1f)
                FinishAnimation();
        }

        /// <summary>
        /// Snap cubies to grid, reset rotations, recreate sticker quads
        /// from the logical state so every quad faces outward correctly.
        /// </summary>
        private void FinishAnimation()
        {
            _animPivot.localRotation =
                Quaternion.AngleAxis(_animTargetAngle, _animAxis);

            foreach (var c in _animCubies)
            {
                c.SetParent(transform, true);
                c.localPosition = SnapToGrid(c.localPosition);
            }

            // Snapshot before clearing
            var moved = new List<Transform>(_animCubies);
            _animCubies.Clear();
            _animPivot.localRotation = Quaternion.identity;
            _animating = false;

            RebuildCubieIndex();

            // Reset rotation and recreate sticker quads from logical state.
            // Single-sided quads face wrong directions after physical rotation;
            // the logical state (already permuted) is the authoritative source.
            foreach (var c in moved)
            {
                c.localRotation = Quaternion.identity;
                DestroyChildStickers(c);
                var (x, y, z) = GridFromLocalPos(c.localPosition);
                AttachStickers(x, y, z, c.gameObject);
            }

            // Prevent redundant FullRebuild from MarkDirty calls
            // triggered by the match manager's event chain.
            _needsFullRebuild = false;

            DumpPhysical();
        }

        private void FinishAnimationInstant()
        {
            if (!_animating) return;
            FinishAnimation();
        }

        // ═══════════════════════════════════════════════════════════════
        // DEBUG — physical cubie state
        // ═══════════════════════════════════════════════════════════════

        private void DumpPhysical()
        {
            var sb = new StringBuilder();
            sb.Append("[RUBIK-PHYS] Cubie grid after animation:\n");
            float step = CubeSize + Gap;
            float offset = -step;
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            for (int z = 0; z < 3; z++)
            {
                var cubie = _cubies[x, y, z];
                if (cubie == null) continue;
                var t = cubie.transform;
                sb.Append($"  [{x},{y},{z}] pos={t.localPosition:F2} rot={t.localRotation.eulerAngles:F0}");
                // List sticker children and their world-forward directions
                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    // Get the quad's -Z direction in renderer-local space
                    Vector3 worldN = child.TransformDirection(Vector3.back);
                    Vector3 localN = transform.InverseTransformDirection(worldN);
                    // Read current material color
                    string colorLabel = "?";
                    var rend = child.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        Color c = rend.material.HasProperty("_BaseColor")
                            ? rend.material.GetColor("_BaseColor")
                            : rend.material.color;
                        colorLabel = MatchColorLabel(c);
                    }
                    sb.Append($" [{colorLabel}→{FormatDir(localN)}]");
                }
                sb.Append('\n');
            }
            Debug.Log(sb.ToString());
        }

        private static string MatchColorLabel(Color c)
        {
            float bestDist = float.MaxValue;
            int bestIdx = -1;
            for (int i = 0; i < StickerColors.Length; i++)
            {
                float dist = (c.r - StickerColors[i].r) * (c.r - StickerColors[i].r)
                           + (c.g - StickerColors[i].g) * (c.g - StickerColors[i].g)
                           + (c.b - StickerColors[i].b) * (c.b - StickerColors[i].b);
                if (dist < bestDist) { bestDist = dist; bestIdx = i; }
            }
            if (bestDist > 0.1f) return "?";
            return RubikCube.FaceNames[bestIdx];
        }

        private static string FormatDir(Vector3 v)
        {
            if (v.y >  0.9f) return "+Y";
            if (v.y < -0.9f) return "-Y";
            if (v.z >  0.9f) return "+Z";
            if (v.z < -0.9f) return "-Z";
            if (v.x >  0.9f) return "+X";
            if (v.x < -0.9f) return "-X";
            return $"({v.x:F1},{v.y:F1},{v.z:F1})";
        }

        // ═══════════════════════════════════════════════════════════════
        // STICKER REPAINT HELPERS
        // ═══════════════════════════════════════════════════════════════

        private static void DestroyChildStickers(Transform cubie)
        {
            for (int i = cubie.childCount - 1; i >= 0; i--)
                DestroyImmediate(cubie.GetChild(i).gameObject);
        }

        private (int x, int y, int z) GridFromLocalPos(Vector3 pos)
        {
            float step = CubeSize + Gap;
            float offset = -step;
            int x = Mathf.Clamp(Mathf.RoundToInt((pos.x - offset) / step), 0, 2);
            int y = Mathf.Clamp(Mathf.RoundToInt((pos.y - offset) / step), 0, 2);
            int z = Mathf.Clamp(Mathf.RoundToInt((pos.z - offset) / step), 0, 2);
            return (x, y, z);
        }

        // ═══════════════════════════════════════════════════════════════
        // GRID INDEX
        // ═══════════════════════════════════════════════════════════════

        private void RebuildCubieIndex()
        {
            float step = CubeSize + Gap;
            float offset = -step;

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    for (int z = 0; z < 3; z++)
                        _cubies[x, y, z] = null;

            foreach (Transform child in transform)
            {
                if (child == _animPivot) continue;
                if (!child.name.StartsWith("Cubie_")) continue;

                Vector3 p = child.localPosition;
                int xi = Mathf.Clamp(Mathf.RoundToInt((p.x - offset) / step), 0, 2);
                int yi = Mathf.Clamp(Mathf.RoundToInt((p.y - offset) / step), 0, 2);
                int zi = Mathf.Clamp(Mathf.RoundToInt((p.z - offset) / step), 0, 2);
                _cubies[xi, yi, zi] = child.gameObject;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER GATHER
        // ═══════════════════════════════════════════════════════════════

        private static Vector3 FaceAxis(int face)
        {
            return face switch
            {
                RubikCube.U or RubikCube.D => Vector3.up,
                RubikCube.F or RubikCube.B => Vector3.forward,
                RubikCube.L or RubikCube.R => Vector3.right,
                _ => Vector3.up
            };
        }

        private void GatherLayerCubies(int face, List<Transform> result)
        {
            float step = CubeSize + Gap;
            float threshold = step * 0.4f;

            float target = face switch
            {
                RubikCube.U =>  step,
                RubikCube.D => -step,
                RubikCube.F =>  step,
                RubikCube.B => -step,
                RubikCube.L => -step,
                RubikCube.R =>  step,
                _ => 0f
            };

            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            for (int z = 0; z < 3; z++)
            {
                var cubie = _cubies[x, y, z];
                if (cubie == null) continue;

                float coord = face switch
                {
                    RubikCube.U or RubikCube.D => cubie.transform.localPosition.y,
                    RubikCube.F or RubikCube.B => cubie.transform.localPosition.z,
                    RubikCube.L or RubikCube.R => cubie.transform.localPosition.x,
                    _ => 0f
                };

                if (Mathf.Abs(coord - target) < threshold)
                    result.Add(cubie.transform);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // GRID SNAP
        // ═══════════════════════════════════════════════════════════════

        private Vector3 SnapToGrid(Vector3 pos)
        {
            float step = CubeSize + Gap;
            float offset = -step;
            pos.x = offset + Mathf.Round((pos.x - offset) / step) * step;
            pos.y = offset + Mathf.Round((pos.y - offset) / step) * step;
            pos.z = offset + Mathf.Round((pos.z - offset) / step) * step;
            return pos;
        }

        private static Quaternion SnapRotation(Quaternion rot)
        {
            Vector3 e = rot.eulerAngles;
            e.x = Mathf.Round(e.x / 90f) * 90f;
            e.y = Mathf.Round(e.y / 90f) * 90f;
            e.z = Mathf.Round(e.z / 90f) * 90f;
            return Quaternion.Euler(e);
        }

        // ═══════════════════════════════════════════════════════════════
        // BUILD — create 26 cubies + sticker quads from logical state
        // ═══════════════════════════════════════════════════════════════

        private void BuildCubeFromState()
        {
            float step = CubeSize + Gap;
            float offset = -step;

            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            for (int z = 0; z < 3; z++)
            {
                if (x == 1 && y == 1 && z == 1) continue;

                var cubie = CreateCubie($"Cubie_{x}{y}{z}");
                cubie.transform.SetParent(transform, false);
                cubie.transform.localPosition = new Vector3(
                    offset + x * step,
                    offset + y * step,
                    offset + z * step);
                cubie.transform.localScale = Vector3.one * CubeSize;
                _cubies[x, y, z] = cubie;

                AttachStickers(x, y, z, cubie);
            }
        }

        /// <summary>
        /// Create sticker quads for every exposed face of the cubie at grid
        /// (x,y,z) and paint each with the color from the current logical
        /// state.  After this call, the cubie is a self-contained physical
        /// object whose visible colors match the logical model.
        /// </summary>
        private void AttachStickers(int x, int y, int z, GameObject cubie)
        {
            float half = CubeSize * 0.5f + StickerInset;
            float sz = CubeSize * 0.85f;

            if (y == 2) // U face
            {
                int pos = (2 - z) * 3 + x;
                var q = MakeQuad(cubie, new Vector3(0, half, 0),
                    Quaternion.Euler(90, 0, 0), sz);
                PaintQuad(q, _cube.GetSticker(RubikCube.U, pos));
            }
            if (y == 0) // D face
            {
                int pos = z * 3 + x;
                var q = MakeQuad(cubie, new Vector3(0, -half, 0),
                    Quaternion.Euler(-90, 0, 0), sz);
                PaintQuad(q, _cube.GetSticker(RubikCube.D, pos));
            }
            if (z == 2) // F face
            {
                int pos = (2 - y) * 3 + x;
                var q = MakeQuad(cubie, new Vector3(0, 0, half),
                    Quaternion.Euler(0, 180, 0), sz);
                PaintQuad(q, _cube.GetSticker(RubikCube.F, pos));
            }
            if (z == 0) // B face
            {
                int pos = (2 - y) * 3 + (2 - x);
                var q = MakeQuad(cubie, new Vector3(0, 0, -half),
                    Quaternion.identity, sz);
                PaintQuad(q, _cube.GetSticker(RubikCube.B, pos));
            }
            if (x == 0) // L face
            {
                int pos = (2 - y) * 3 + (2 - z);
                var q = MakeQuad(cubie, new Vector3(-half, 0, 0),
                    Quaternion.Euler(0, 90, 0), sz);
                PaintQuad(q, _cube.GetSticker(RubikCube.L, pos));
            }
            if (x == 2) // R face
            {
                int pos = (2 - y) * 3 + z;
                var q = MakeQuad(cubie, new Vector3(half, 0, 0),
                    Quaternion.Euler(0, -90, 0), sz);
                PaintQuad(q, _cube.GetSticker(RubikCube.R, pos));
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════

        private GameObject CreateCubie(string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            SetColor(go, CubieColor);
            return go;
        }

        private GameObject MakeQuad(GameObject parent,
            Vector3 localPos, Quaternion localRot, float size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Sticker";
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = Vector3.one * size;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            return go;
        }

        private void PaintQuad(GameObject quad, int colorIdx)
        {
            if (colorIdx >= 0 && colorIdx < StickerColors.Length)
                SetColor(quad, StickerColors[colorIdx]);
        }

        private static void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var mat = r.material;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 0.3f);
                }
            }
            else
            {
                mat.color = color;
            }
        }
    }
}
