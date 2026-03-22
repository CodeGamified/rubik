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
            _cube.ResetToSolved();
            ScrambleLength = _cube.Scramble(_scrambleMoves);
            Score = 0;
            GameOver = false;
            MatchInProgress = true;

            _cube.OnCubeChanged -= OnCubeStateChanged;
            _cube.OnCubeChanged += OnCubeStateChanged;

            OnMatchStarted?.Invoke();
            OnCubeChanged?.Invoke();
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
