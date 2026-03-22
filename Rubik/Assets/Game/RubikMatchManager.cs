// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using UnityEngine;
using CodeGamified.Time;

namespace Rubik.Game
{
    /// <summary>
    /// Match manager — scramble, solve detection, scoring, auto-restart.
    /// </summary>
    public class RubikMatchManager : MonoBehaviour
    {
        private RubikCube _cube;

        // Config
        private bool _autoRestart;
        private float _restartDelay;
        private int _scrambleMoves;

        // State
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public bool GameOver { get; private set; }
        public bool MatchInProgress { get; private set; }
        public int MatchesPlayed { get; private set; }
        public int ScrambleLength { get; private set; }

        // Accessors
        public RubikCube Cube => _cube;

        // Events
        public System.Action OnMatchStarted;
        public System.Action OnGameOver;
        public System.Action<int> OnScoreChanged;
        public System.Action OnCubeChanged;
        public System.Action OnSolved;

        public void Initialize(RubikCube cube, int scrambleMoves = 20,
                               bool autoRestart = true, float restartDelay = 3f)
        {
            _cube = cube;
            _scrambleMoves = scrambleMoves;
            _autoRestart = autoRestart;
            _restartDelay = restartDelay;
        }

        public void StartMatch()
        {
            StopAllCoroutines();
            _cube.ResetToSolved();
            Score = 0;
            GameOver = false;
            MatchInProgress = false;

            _cube.OnCubeChanged -= OnCubeStateChanged;
            _cube.OnCubeChanged += OnCubeStateChanged;

            StartCoroutine(AnimatedStartSequence());
        }

        private System.Collections.IEnumerator AnimatedStartSequence()
        {
            var renderer = _cube.GetComponent<RubikRenderer>();

            // 1. Pulse sticker emissivity for 2 seconds
            if (renderer != null)
                yield return renderer.PulseEmission(2f);

            // 2. DEBUG: deterministic 3-move scramble: F CW, U CW, R CW
            //    Expected state after each move on a solved cube:
            //    F CW: U bottom row → R left col, R left col → D top row(rev), D top row → L right col, L right col → U bottom row
            //    U CW: F top row → R top row, R top row → B top row, B top row → L top row, L top row → F top row
            //    R CW: U right col → B left col(rev), F right col → U right col, D right col → F right col, B left col(rev) → D right col
            //    To solve: R CCW, U CCW, F CCW
            var debugScramble = new (int face, int dir)[]
            {
                (RubikCube.F, 1),   // F CW
                (RubikCube.U, 1),   // U CW
                (RubikCube.R, 1),   // R CW
            };
            for (int i = 0; i < debugScramble.Length; i++)
            {
                var (face, dir) = debugScramble[i];
                Debug.Log($"[SCRAMBLE] Move {i+1}/{debugScramble.Length}: {RubikCube.FaceNames[face]} {(dir == 1 ? "CW" : "CCW")}");
                _cube.RotateFace(face, dir);

                // Wait for animation to finish + minimum 500ms between moves
                float waited = 0f;
                while (waited < 0.5f || (renderer != null && renderer.IsBusy))
                {
                    waited += Time.deltaTime;
                    yield return null;
                }
            }

            // 3. Scramble complete — begin match
            _cube.ResetMoveCount();
            ScrambleLength = _scrambleMoves;
            MatchInProgress = true;
            OnMatchStarted?.Invoke();
        }

        // ═══════════════════════════════════════════════════════════════
        // COMMANDS — called by IOHandler
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Rotate a face. Returns 1=success, 0=fail.</summary>
        public int DoRotate(int face, int direction)
        {
            if (!MatchInProgress || GameOver) return 0;
            return _cube.RotateFace(face, direction);
        }

        // ═══════════════════════════════════════════════════════════════
        // STATE CHANGE HANDLER
        // ═══════════════════════════════════════════════════════════════

        private void OnCubeStateChanged()
        {
            OnCubeChanged?.Invoke();

            if (!MatchInProgress || GameOver) return;

            // Update score: solved stickers + bonus for solved faces
            int stickers = _cube.SolvedStickerCount();
            int faces = _cube.SolvedFaceCount();
            int newScore = stickers + faces * 10;
            if (newScore != Score)
            {
                Score = newScore;
                if (Score > HighScore) HighScore = Score;
                OnScoreChanged?.Invoke(Score);
            }

            // Check for full solve
            if (_cube.IsSolved)
            {
                // Bonus for efficiency — fewer moves = higher score
                int moveBonus = Mathf.Max(0, 200 - _cube.MoveCount * 2);
                Score += 100 + moveBonus;
                if (Score > HighScore) HighScore = Score;
                OnScoreChanged?.Invoke(Score);
                OnSolved?.Invoke();
                EndGame();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // GAME OVER
        // ═══════════════════════════════════════════════════════════════

        private void EndGame()
        {
            GameOver = true;
            MatchInProgress = false;
            MatchesPlayed++;
            OnGameOver?.Invoke();

            if (_autoRestart)
                StartCoroutine(RestartAfterDelay());
        }

        private System.Collections.IEnumerator RestartAfterDelay()
        {
            float waited = 0f;
            while (waited < _restartDelay)
            {
                if (SimulationTime.Instance != null && !SimulationTime.Instance.isPaused)
                    waited += Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);
                yield return null;
            }
            StartMatch();
        }

        private void OnDestroy()
        {
            if (_cube != null)
                _cube.OnCubeChanged -= OnCubeStateChanged;
        }
    }
}
