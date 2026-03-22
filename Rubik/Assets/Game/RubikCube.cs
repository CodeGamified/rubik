// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using UnityEngine;

namespace Rubik.Game
{
    /// <summary>
    /// Cubie-centric Rubik's Cube.
    /// The 3D grid IS the state — no separate sticker array.
    /// Stickers never change color; cubies move between grid slots.
    /// </summary>
    public class RubikCube : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════════
        // FACE CONSTANTS
        // ═══════════════════════════════════════════════════════════════

        public const int U = 0, D = 1, F = 2, B = 3, L = 4, R = 5;
        public const int FaceCount = 6;
        public const int N = 3;

        public static readonly string[] FaceNames = { "U", "D", "F", "B", "L", "R" };

        public static readonly Vector3Int[] FaceNormals =
        {
            Vector3Int.up,                        // U  +Y
            Vector3Int.down,                      // D  -Y
            new Vector3Int( 0,  0,  1),           // F  +Z
            new Vector3Int( 0,  0, -1),           // B  -Z
            Vector3Int.left,                      // L  -X
            Vector3Int.right                      // R  +X
        };

        public static readonly Vector3[] FaceAxes =
        {
            Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.left, Vector3.right
        };

        // Standard Rubik face colors: U=white, D=yellow, F=green, B=blue, L=orange, R=red
        public static readonly Color[] FaceColors =
        {
            Color.white,
            Color.yellow,
            Color.green,
            Color.blue,
            new Color(1f, 0.5f, 0f),
            Color.red
        };

        public static readonly string[] FaceColorKeys =
        {
            "white", "yellow", "green", "blue", "orange", "red"
        };

        // ═══════════════════════════════════════════════════════════════
        // STATE
        // ═══════════════════════════════════════════════════════════════

        private RubikCubie[,,] _grid; // _grid[x, y, z]
        public int MoveCount { get; private set; }
        public bool IsSolved => SolvedFaceCount() == FaceCount;

        // Events
        public System.Action OnCubeChanged;

        /// <summary>
        /// Fired before grid permutation with (face, direction, layerCubies, axis).
        /// The renderer uses this to set up animation.
        /// </summary>
        public System.Action<int, int, RubikCubie[], Vector3> OnFaceRotating;

        // ═══════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ═══════════════════════════════════════════════════════════════

        public void Initialize()
        {
            _grid = new RubikCubie[N, N, N];
            BuildCubies();
            MoveCount = 0;
        }

        public void ResetToSolved()
        {
            // Destroy existing cubies
            if (_grid != null)
            {
                for (int x = 0; x < N; x++)
                    for (int y = 0; y < N; y++)
                        for (int z = 0; z < N; z++)
                        {
                            if (_grid[x, y, z] != null)
                                Destroy(_grid[x, y, z].gameObject);
                        }
            }
            _grid = new RubikCubie[N, N, N];
            BuildCubies();
            MoveCount = 0;
            OnCubeChanged?.Invoke();
        }

        public void ResetMoveCount() => MoveCount = 0;

        // ═══════════════════════════════════════════════════════════════
        // CUBIE CREATION
        // ═══════════════════════════════════════════════════════════════

        private void BuildCubies()
        {
            int last = N - 1;
            for (int x = 0; x < N; x++)
                for (int y = 0; y < N; y++)
                    for (int z = 0; z < N; z++)
                    {
                        // Interior cubie (hidden) — skip
                        if (x > 0 && x < last && y > 0 && y < last && z > 0 && z < last)
                            continue;

                        var stickers = new System.Collections.Generic.List<RubikCubie.Sticker>(3);

                        if (y == last) stickers.Add(new RubikCubie.Sticker { Normal = FaceNormals[U], ColorIndex = U });
                        if (y == 0)    stickers.Add(new RubikCubie.Sticker { Normal = FaceNormals[D], ColorIndex = D });
                        if (z == last) stickers.Add(new RubikCubie.Sticker { Normal = FaceNormals[F], ColorIndex = F });
                        if (z == 0)    stickers.Add(new RubikCubie.Sticker { Normal = FaceNormals[B], ColorIndex = B });
                        if (x == 0)    stickers.Add(new RubikCubie.Sticker { Normal = FaceNormals[L], ColorIndex = L });
                        if (x == last) stickers.Add(new RubikCubie.Sticker { Normal = FaceNormals[R], ColorIndex = R });

                        var go = new GameObject($"Cubie_{x}_{y}_{z}");
                        go.transform.SetParent(transform, false);
                        go.transform.localPosition = GridToLocal(x, y, z);

                        var cubie = go.AddComponent<RubikCubie>();
                        cubie.Init(stickers.ToArray());
                        _grid[x, y, z] = cubie;
                    }
        }

        // ═══════════════════════════════════════════════════════════════
        // FACE ROTATION
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Rotate a face. direction: 1 = CW (looking from outside), -1 = CCW.
        /// Returns 1 on success.
        /// </summary>
        public int RotateFace(int face, int direction)
        {
            if (face < 0 || face >= FaceCount) return 0;
            if (direction != 1 && direction != -1) direction = direction > 0 ? 1 : -1;

            // 1. Gather layer cubies BEFORE permutation (for animation)
            var layerCubies = GetLayerCubies(face);
            var axis = FaceAxes[face];

            // Notify renderer so it can set up animation
            OnFaceRotating?.Invoke(face, direction, layerCubies, axis);

            // 2. Rotate sticker normals on all cubies in the layer
            for (int i = 0; i < layerCubies.Length; i++)
            {
                if (layerCubies[i] != null)
                    layerCubies[i].RotateStickers(axis, direction);
            }

            // 3. Permute grid
            PermuteLayer(face, direction);

            // 4. Update bookkeeping
            MoveCount++;
            OnCubeChanged?.Invoke();
            return 1;
        }

        // ═══════════════════════════════════════════════════════════════
        // GRID PERMUTATION
        // ═══════════════════════════════════════════════════════════════

        private void PermuteLayer(int face, int direction)
        {
            int last = N - 1;
            var slice = new RubikCubie[N, N];

            // Read current slice
            for (int a = 0; a < N; a++)
                for (int b = 0; b < N; b++)
                    slice[a, b] = GetGridSlot(face, a, b);

            // Write back rotated: CW(a,b) = (b, last-a)
            for (int a = 0; a < N; a++)
                for (int b = 0; b < N; b++)
                {
                    RubikCubie source = direction == 1
                        ? slice[last - b, a]    // CW
                        : slice[b, last - a];   // CCW
                    SetGridSlot(face, a, b, source);
                }
        }

        // ═══════════════════════════════════════════════════════════════
        // FACE ↔ GRID MAPPING (the 6-case mapping)
        //
        // face(a, b) → grid(x, y, z)
        // Derived so that CW(a,b)=(b, 2-a) matches physical CW rotation.
        //
        // U: (a, 2, b)       D: (a, 0, 2-b)
        // F: (a, 2-b, 2)     B: (a, b, 0)
        // L: (0, 2-b, a)     R: (2, 2-b, a)
        // ═══════════════════════════════════════════════════════════════

        private RubikCubie GetGridSlot(int face, int a, int b)
        {
            int last = N - 1;
            switch (face)
            {
                case U: return _grid[a, last, b];
                case D: return _grid[a, 0, last - b];
                case F: return _grid[a, last - b, last];
                case B: return _grid[a, b, 0];
                case L: return _grid[0, last - b, a];
                case R: return _grid[last, last - b, a];
                default: return null;
            }
        }

        private void SetGridSlot(int face, int a, int b, RubikCubie cubie)
        {
            int last = N - 1;
            switch (face)
            {
                case U: _grid[a, last, b] = cubie; break;
                case D: _grid[a, 0, last - b] = cubie; break;
                case F: _grid[a, last - b, last] = cubie; break;
                case B: _grid[a, b, 0] = cubie; break;
                case L: _grid[0, last - b, a] = cubie; break;
                case R: _grid[last, last - b, a] = cubie; break;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // STATE QUERIES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Get the color index of the sticker at (face, pos) where pos = a*3+b (row-major).
        /// </summary>
        public int GetSticker(int face, int pos)
        {
            if (face < 0 || face >= FaceCount || pos < 0 || pos >= N * N) return -1;
            int a = pos / N, b = pos % N;
            var cubie = GetGridSlot(face, a, b);
            if (cubie == null) return -1;
            return cubie.GetColorAt(FaceNormals[face]);
        }

        public bool IsFaceSolved(int face)
        {
            if (face < 0 || face >= FaceCount) return false;
            int expected = face; // solved → each face shows its own color
            for (int a = 0; a < N; a++)
                for (int b = 0; b < N; b++)
                {
                    var cubie = GetGridSlot(face, a, b);
                    if (cubie == null) return false;
                    if (cubie.GetColorAt(FaceNormals[face]) != expected)
                        return false;
                }
            return true;
        }

        public int SolvedFaceCount()
        {
            int count = 0;
            for (int f = 0; f < FaceCount; f++)
                if (IsFaceSolved(f)) count++;
            return count;
        }

        public int SolvedStickerCount()
        {
            int count = 0;
            for (int f = 0; f < FaceCount; f++)
            {
                var normal = FaceNormals[f];
                for (int a = 0; a < N; a++)
                    for (int b = 0; b < N; b++)
                    {
                        var cubie = GetGridSlot(f, a, b);
                        if (cubie != null && cubie.GetColorAt(normal) == f)
                            count++;
                    }
            }
            return count;
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Returns all cubies in the given face's layer (may include nulls for interior).</summary>
        public RubikCubie[] GetLayerCubies(int face)
        {
            var result = new RubikCubie[N * N];
            for (int a = 0; a < N; a++)
                for (int b = 0; b < N; b++)
                    result[a * N + b] = GetGridSlot(face, a, b);
            return result;
        }

        /// <summary>Get the cubie at grid position (x, y, z).</summary>
        public RubikCubie GetCubieAt(int x, int y, int z) => _grid[x, y, z];

        /// <summary>Convert grid indices to local position (centered at origin).</summary>
        public static Vector3 GridToLocal(int x, int y, int z)
        {
            const float offset = (N - 1) / 2f; // 1.0 for N=3
            return new Vector3(x - offset, y - offset, z - offset);
        }

        /// <summary>Get the target local position for a cubie after permutation.</summary>
        public Vector3 GetCubieTargetPosition(RubikCubie cubie)
        {
            for (int x = 0; x < N; x++)
                for (int y = 0; y < N; y++)
                    for (int z = 0; z < N; z++)
                        if (_grid[x, y, z] == cubie)
                            return GridToLocal(x, y, z);
            return Vector3.zero;
        }
    }
}
