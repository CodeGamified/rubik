// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System.Text;
using UnityEngine;

namespace Rubik.Game
{
    /// <summary>
    /// 3×3 Rubik's Cube state — 6 faces × 9 stickers.
    ///
    /// Face indices: 0=U(White), 1=D(Yellow), 2=F(Green), 3=B(Blue), 4=L(Orange), 5=R(Red).
    /// Sticker indices per face (row-major looking at the face):
    ///   0 1 2
    ///   3 4 5
    ///   6 7 8
    ///
    /// Center sticker (index 4) never moves — it defines the face color.
    /// </summary>
    public class RubikCube : MonoBehaviour
    {
        public const int FaceCount = 6;
        public const int StickersPerFace = 9;

        // Face IDs
        public const int U = 0; // Up    (White)
        public const int D = 1; // Down  (Yellow)
        public const int F = 2; // Front (Green)
        public const int B = 3; // Back  (Blue)
        public const int L = 4; // Left  (Orange)
        public const int R = 5; // Right (Red)

        public static readonly string[] FaceNames = { "U", "D", "F", "B", "L", "R" };
        public static readonly Color[] FaceColors =
        {
            Color.white,                            // U
            Color.yellow,                           // D
            new Color(0f, 0.8f, 0f),                // F green
            new Color(0.2f, 0.4f, 1f),              // B blue
            new Color(1f, 0.5f, 0f),                // L orange
            new Color(1f, 0f, 0f),                  // R red
        };

        /// <summary>Sticker state: _stickers[face, position] = color (0-5).</summary>
        private int[,] _stickers;

        public int MoveCount { get; private set; }
        public bool IsSolved => CheckSolved();

        public System.Action OnCubeChanged;
        /// <summary>Fired BEFORE state changes with (face, direction). Used by renderer for animation.</summary>
        public System.Action<int, int> OnFaceRotating;

        public void Initialize()
        {
            _stickers = new int[FaceCount, StickersPerFace];
            ResetToSolved();
        }

        /// <summary>Reset cube to the solved state (each face is its own color).</summary>
        public void ResetToSolved()
        {
            for (int f = 0; f < FaceCount; f++)
                for (int s = 0; s < StickersPerFace; s++)
                    _stickers[f, s] = f;
            MoveCount = 0;
            OnCubeChanged?.Invoke();
        }

        // ═══════════════════════════════════════════════════════════════
        // QUERIES
        // ═══════════════════════════════════════════════════════════════

        public int GetSticker(int face, int pos)
        {
            if (face < 0 || face >= FaceCount || pos < 0 || pos >= StickersPerFace) return -1;
            return _stickers[face, pos];
        }

        /// <summary>Returns true if all 9 stickers on the given face are the same color.</summary>
        public bool IsFaceSolved(int face)
        {
            if (face < 0 || face >= FaceCount) return false;
            int c = _stickers[face, 0];
            for (int i = 1; i < StickersPerFace; i++)
                if (_stickers[face, i] != c) return false;
            return true;
        }

        /// <summary>Count how many stickers are in their solved position.</summary>
        public int SolvedStickerCount()
        {
            int count = 0;
            for (int f = 0; f < FaceCount; f++)
                for (int s = 0; s < StickersPerFace; s++)
                    if (_stickers[f, s] == f) count++;
            return count;
        }

        /// <summary>Count how many of the 6 faces are fully solved.</summary>
        public int SolvedFaceCount()
        {
            int count = 0;
            for (int f = 0; f < FaceCount; f++)
                if (IsFaceSolved(f)) count++;
            return count;
        }

        private bool CheckSolved()
        {
            for (int f = 0; f < FaceCount; f++)
                for (int s = 0; s < StickersPerFace; s++)
                    if (_stickers[f, s] != f) return false;
            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // FACE ROTATION — the core permutation
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Rotate a face. direction=1 for CW, direction=-1 for CCW.
        /// Returns 1 on success, 0 on invalid input.
        /// </summary>
        public int RotateFace(int face, int direction)
        {
            if (face < 0 || face >= FaceCount) return 0;
            if (direction != 1 && direction != -1) return 0;

            // Notify renderer BEFORE state change so it can snapshot cubie positions
            OnFaceRotating?.Invoke(face, direction);

            if (direction == 1)
                RotateFaceCW(face);
            else
                RotateFaceCCW(face);

            MoveCount++;
            DumpState(FaceNames[face], direction == 1 ? "CW" : "CCW");
            OnCubeChanged?.Invoke();
            return 1;
        }

        private void RotateFaceCW(int face)
        {
            // Rotate the 9 stickers on this face 90° CW
            RotateFaceStickers(face, true);
            // Cycle the 4 adjacent edge strips
            CycleEdgesCW(face);
        }

        private void RotateFaceCCW(int face)
        {
            RotateFaceStickers(face, false);
            CycleEdgesCCW(face);
        }

        /// <summary>Rotate the 9 stickers on the face itself (not adjacent edges).</summary>
        private void RotateFaceStickers(int face, bool cw)
        {
            // Corner cycle: 0→2→8→6 (CW) or 0→6→8→2 (CCW)
            // Edge cycle: 1→5→7→3 (CW) or 1→3→7→5 (CCW)
            if (cw)
            {
                Cycle4(face, 0, 2, 8, 6);
                Cycle4(face, 1, 5, 7, 3);
            }
            else
            {
                Cycle4(face, 0, 6, 8, 2);
                Cycle4(face, 1, 3, 7, 5);
            }
        }

        private void Cycle4(int face, int a, int b, int c, int d)
        {
            int tmp = _stickers[face, a];
            _stickers[face, a] = _stickers[face, d];
            _stickers[face, d] = _stickers[face, c];
            _stickers[face, c] = _stickers[face, b];
            _stickers[face, b] = tmp;
        }

        /// <summary>Cycle the 4 adjacent edge strips clockwise for the given face.</summary>
        private void CycleEdgesCW(int face)
        {
            // Each face rotation affects 12 stickers on 4 adjacent faces
            switch (face)
            {
                case U: CycleStrip(F, 0, 1, 2, R, 0, 1, 2, B, 0, 1, 2, L, 0, 1, 2, false); break;
                case D: CycleStrip(F, 6, 7, 8, L, 6, 7, 8, B, 6, 7, 8, R, 6, 7, 8, false); break;
                case F: CycleStripF_CW(); break;
                case B: CycleStripB_CW(); break;
                case L: CycleStripL_CW(); break;
                case R: CycleStripR_CW(); break;
            }
        }

        private void CycleEdgesCCW(int face)
        {
            // CCW = 3 CW rotations of the edge strips (simplest correct approach)
            switch (face)
            {
                case U: CycleStrip(F, 0, 1, 2, L, 0, 1, 2, B, 0, 1, 2, R, 0, 1, 2, true); break;
                case D: CycleStrip(F, 6, 7, 8, R, 6, 7, 8, B, 6, 7, 8, L, 6, 7, 8, true); break;
                case F: CycleStripF_CCW(); break;
                case B: CycleStripB_CCW(); break;
                case L: CycleStripL_CCW(); break;
                case R: CycleStripR_CCW(); break;
            }
        }

        /// <summary>Generic 4-face strip cycle (for U and D faces where strips are simple rows).</summary>
        private void CycleStrip(int f1, int a1, int b1, int c1,
                                int f2, int a2, int b2, int c2,
                                int f3, int a3, int b3, int c3,
                                int f4, int a4, int b4, int c4,
                                bool reverse)
        {
            if (!reverse)
            {
                // f1 ← f4, f4 ← f3, f3 ← f2, f2 ← f1
                int t0 = _stickers[f1, a1], t1 = _stickers[f1, b1], t2 = _stickers[f1, c1];
                _stickers[f1, a1] = _stickers[f4, a4]; _stickers[f1, b1] = _stickers[f4, b4]; _stickers[f1, c1] = _stickers[f4, c4];
                _stickers[f4, a4] = _stickers[f3, a3]; _stickers[f4, b4] = _stickers[f3, b3]; _stickers[f4, c4] = _stickers[f3, c3];
                _stickers[f3, a3] = _stickers[f2, a2]; _stickers[f3, b3] = _stickers[f2, b2]; _stickers[f3, c3] = _stickers[f2, c2];
                _stickers[f2, a2] = t0; _stickers[f2, b2] = t1; _stickers[f2, c2] = t2;
            }
            else
            {
                // f1 ← f2, f2 ← f3, f3 ← f4, f4 ← f1
                int t0 = _stickers[f1, a1], t1 = _stickers[f1, b1], t2 = _stickers[f1, c1];
                _stickers[f1, a1] = _stickers[f2, a2]; _stickers[f1, b1] = _stickers[f2, b2]; _stickers[f1, c1] = _stickers[f2, c2];
                _stickers[f2, a2] = _stickers[f3, a3]; _stickers[f2, b2] = _stickers[f3, b3]; _stickers[f2, c2] = _stickers[f3, c3];
                _stickers[f3, a3] = _stickers[f4, a4]; _stickers[f3, b3] = _stickers[f4, b4]; _stickers[f3, c3] = _stickers[f4, c4];
                _stickers[f4, a4] = t0; _stickers[f4, b4] = t1; _stickers[f4, c4] = t2;
            }
        }

        // ── Front face CW: U-bottom → R-left → D-top → L-right ──
        private void CycleStripF_CW()
        {
            int t0 = _stickers[U, 6], t1 = _stickers[U, 7], t2 = _stickers[U, 8];
            _stickers[U, 6] = _stickers[L, 8]; _stickers[U, 7] = _stickers[L, 5]; _stickers[U, 8] = _stickers[L, 2];
            _stickers[L, 2] = _stickers[D, 0]; _stickers[L, 5] = _stickers[D, 1]; _stickers[L, 8] = _stickers[D, 2];
            _stickers[D, 0] = _stickers[R, 6]; _stickers[D, 1] = _stickers[R, 3]; _stickers[D, 2] = _stickers[R, 0];
            _stickers[R, 0] = t0; _stickers[R, 3] = t1; _stickers[R, 6] = t2;
        }

        private void CycleStripF_CCW()
        {
            int t0 = _stickers[U, 6], t1 = _stickers[U, 7], t2 = _stickers[U, 8];
            _stickers[U, 6] = _stickers[R, 0]; _stickers[U, 7] = _stickers[R, 3]; _stickers[U, 8] = _stickers[R, 6];
            _stickers[R, 0] = _stickers[D, 2]; _stickers[R, 3] = _stickers[D, 1]; _stickers[R, 6] = _stickers[D, 0];
            _stickers[D, 0] = _stickers[L, 2]; _stickers[D, 1] = _stickers[L, 5]; _stickers[D, 2] = _stickers[L, 8];
            _stickers[L, 2] = t2; _stickers[L, 5] = t1; _stickers[L, 8] = t0;
        }

        // ── Back face CW: U-top → L-left → D-bottom → R-right ──
        private void CycleStripB_CW()
        {
            int t0 = _stickers[U, 0], t1 = _stickers[U, 1], t2 = _stickers[U, 2];
            _stickers[U, 0] = _stickers[R, 2]; _stickers[U, 1] = _stickers[R, 5]; _stickers[U, 2] = _stickers[R, 8];
            _stickers[R, 2] = _stickers[D, 8]; _stickers[R, 5] = _stickers[D, 7]; _stickers[R, 8] = _stickers[D, 6];
            _stickers[D, 6] = _stickers[L, 0]; _stickers[D, 7] = _stickers[L, 3]; _stickers[D, 8] = _stickers[L, 6];
            _stickers[L, 0] = t2; _stickers[L, 3] = t1; _stickers[L, 6] = t0;
        }

        private void CycleStripB_CCW()
        {
            int t0 = _stickers[U, 0], t1 = _stickers[U, 1], t2 = _stickers[U, 2];
            _stickers[U, 0] = _stickers[L, 6]; _stickers[U, 1] = _stickers[L, 3]; _stickers[U, 2] = _stickers[L, 0];
            _stickers[L, 0] = _stickers[D, 6]; _stickers[L, 3] = _stickers[D, 7]; _stickers[L, 6] = _stickers[D, 8];
            _stickers[D, 6] = _stickers[R, 8]; _stickers[D, 7] = _stickers[R, 5]; _stickers[D, 8] = _stickers[R, 2];
            _stickers[R, 2] = t0; _stickers[R, 5] = t1; _stickers[R, 8] = t2;
        }

        // ── Left face CW: U-left → F-left → D-left → B-right(reversed) ──
        private void CycleStripL_CW()
        {
            int t0 = _stickers[U, 0], t1 = _stickers[U, 3], t2 = _stickers[U, 6];
            _stickers[U, 0] = _stickers[B, 8]; _stickers[U, 3] = _stickers[B, 5]; _stickers[U, 6] = _stickers[B, 2];
            _stickers[B, 2] = _stickers[D, 6]; _stickers[B, 5] = _stickers[D, 3]; _stickers[B, 8] = _stickers[D, 0];
            _stickers[D, 0] = _stickers[F, 0]; _stickers[D, 3] = _stickers[F, 3]; _stickers[D, 6] = _stickers[F, 6];
            _stickers[F, 0] = t0; _stickers[F, 3] = t1; _stickers[F, 6] = t2;
        }

        private void CycleStripL_CCW()
        {
            int t0 = _stickers[U, 0], t1 = _stickers[U, 3], t2 = _stickers[U, 6];
            _stickers[U, 0] = _stickers[F, 0]; _stickers[U, 3] = _stickers[F, 3]; _stickers[U, 6] = _stickers[F, 6];
            _stickers[F, 0] = _stickers[D, 0]; _stickers[F, 3] = _stickers[D, 3]; _stickers[F, 6] = _stickers[D, 6];
            _stickers[D, 0] = _stickers[B, 8]; _stickers[D, 3] = _stickers[B, 5]; _stickers[D, 6] = _stickers[B, 2];
            _stickers[B, 2] = t2; _stickers[B, 5] = t1; _stickers[B, 8] = t0;
        }

        // ── Right face CW: U-right → B-left(reversed) → D-right → F-right ──
        private void CycleStripR_CW()
        {
            int t0 = _stickers[U, 2], t1 = _stickers[U, 5], t2 = _stickers[U, 8];
            _stickers[U, 2] = _stickers[F, 2]; _stickers[U, 5] = _stickers[F, 5]; _stickers[U, 8] = _stickers[F, 8];
            _stickers[F, 2] = _stickers[D, 2]; _stickers[F, 5] = _stickers[D, 5]; _stickers[F, 8] = _stickers[D, 8];
            _stickers[D, 2] = _stickers[B, 6]; _stickers[D, 5] = _stickers[B, 3]; _stickers[D, 8] = _stickers[B, 0];
            _stickers[B, 0] = t2; _stickers[B, 3] = t1; _stickers[B, 6] = t0;
        }

        private void CycleStripR_CCW()
        {
            int t0 = _stickers[U, 2], t1 = _stickers[U, 5], t2 = _stickers[U, 8];
            _stickers[U, 2] = _stickers[B, 6]; _stickers[U, 5] = _stickers[B, 3]; _stickers[U, 8] = _stickers[B, 0];
            _stickers[B, 0] = _stickers[D, 8]; _stickers[B, 3] = _stickers[D, 5]; _stickers[B, 6] = _stickers[D, 2];
            _stickers[D, 2] = _stickers[F, 2]; _stickers[D, 5] = _stickers[F, 5]; _stickers[D, 8] = _stickers[F, 8];
            _stickers[F, 2] = t0; _stickers[F, 5] = t1; _stickers[F, 8] = t2;
        }

        // ═══════════════════════════════════════════════════════════════
        // DEBUG
        // ═══════════════════════════════════════════════════════════════

        private static readonly string[] ColorLetters = { "W", "Y", "G", "B", "O", "R" };

        /// <summary>Log full cube state to Unity console after each move.</summary>
        public void DumpState(string move, string dir)
        {
            var sb = new StringBuilder();
            sb.Append($"[RUBIK] After {move} {dir} (move #{MoveCount})\n");
            for (int f = 0; f < FaceCount; f++)
            {
                sb.Append($"  {FaceNames[f]}: ");
                for (int s = 0; s < StickersPerFace; s++)
                {
                    sb.Append(ColorLetters[_stickers[f, s]]);
                    if (s == 2 || s == 5) sb.Append('|');
                }
                sb.Append('\n');
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>Dump current state without a move label (for scramble etc).</summary>
        public void DumpState(string label = "STATE")
        {
            var sb = new StringBuilder();
            sb.Append($"[RUBIK] {label}\n");
            for (int f = 0; f < FaceCount; f++)
            {
                sb.Append($"  {FaceNames[f]}: ");
                for (int s = 0; s < StickersPerFace; s++)
                {
                    sb.Append(ColorLetters[_stickers[f, s]]);
                    if (s == 2 || s == 5) sb.Append('|');
                }
                sb.Append('\n');
            }
            Debug.Log(sb.ToString());
        }

        // ═══════════════════════════════════════════════════════════════
        // SCRAMBLE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Scramble with N random moves. Returns the actual move count used.</summary>
        public int Scramble(int moves)
        {
            int lastFace = -1;
            for (int i = 0; i < moves; i++)
            {
                int face;
                do { face = Random.Range(0, FaceCount); } while (face == lastFace);
                int dir = Random.value < 0.5f ? 1 : -1;
                RotateFaceCW_Internal(face, dir);
                lastFace = face;
            }
            MoveCount = 0; // Reset move count after scramble
            DumpState($"SCRAMBLE ({moves} moves)");
            OnCubeChanged?.Invoke();
            return moves;
        }

        private void RotateFaceCW_Internal(int face, int dir)
        {
            if (dir == 1) { RotateFaceStickers(face, true); CycleEdgesCW(face); }
            else { RotateFaceStickers(face, false); CycleEdgesCCW(face); }
        }
    }
}
