// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using UnityEngine;
using CodeGamified.Engine;
using CodeGamified.Engine.Compiler;
using CodeGamified.Engine.Runtime;
using CodeGamified.Time;
using Rubik.Game;

namespace Rubik.Scripting
{
    /// <summary>
    /// ProgramBehaviour subclass — tick-based code execution for Rubik's Cube.
    ///
    /// EXECUTION MODEL (tick-based, deterministic):
    ///   - Each simulation tick (~20 ops/sec sim-time), the script runs from the top
    ///   - Memory (variables) persists across ticks
    ///   - PC resets to 0 each tick (on HALT)
    ///   - Results are IDENTICAL at 0.5x, 1x, 100x speed
    /// </summary>
    public class RubikProgram : ProgramBehaviour
    {
        private RubikMatchManager _match;
        private RubikCube _cube;
        private RubikIOHandler _ioHandler;
        private RubikCompilerExtension _compilerExt;
        private RubikRenderer _renderer;

        public const float OPS_PER_SECOND = 20f;
        private float _opAccumulator;

        private const string DEFAULT_CODE = @"# 🧊 RUBIK'S CUBE — Write your solver!
# Your script runs at 20 ops/sec (sim-time).
# When it finishes, it restarts from the top.
# Variables persist — use them to track state.
#
# FACES: 0=U(white) 1=D(yellow) 2=F(green) 3=B(blue) 4=L(orange) 5=R(red)
# STICKER POSITIONS (per face, row-major):
#   0 1 2
#   3 4 5
#   6 7 8
#
# BUILTINS — Queries:
#   get_solved()            → 1 if entire cube is solved
#   get_solved_faces()      → count of solved faces (0-6)
#   get_solved_stickers()   → stickers in correct position (0-54)
#   get_moves()             → moves made this solve
#   get_scramble_length()   → scramble length
#   get_sticker(face, pos)  → color at sticker (0-5)
#   get_face_solved(face)   → 1 if face is solved
#   get_score()             → current score
#   get_game_over()         → 1 if game over
#   get_input()             → keyboard input code
#
# BUILTINS — Rotations (all return 1):
#   rotate_u() / rotate_u_prime()   → U / U'
#   rotate_d() / rotate_d_prime()   → D / D'
#   rotate_f() / rotate_f_prime()   → F / F'
#   rotate_b() / rotate_b_prime()   → B / B'
#   rotate_l() / rotate_l_prime()   → L / L'
#   rotate_r() / rotate_r_prime()   → R / R'
#   rotate(face, direction)         → generic (face 0-5, dir 1=CW/-1=CCW)
#
# HOTKEYS: [1] Easy  [2] Medium  [3] Hard  [4] Keyboard  [5] Reset
#
# This starter passes keyboard input through:
inp = get_input()
if inp == 1:
    rotate_u()
if inp == 2:
    rotate_u_prime()
if inp == 3:
    rotate_d()
if inp == 4:
    rotate_d_prime()
if inp == 5:
    rotate_f()
if inp == 6:
    rotate_f_prime()
if inp == 7:
    rotate_b()
if inp == 8:
    rotate_b_prime()
if inp == 9:
    rotate_l()
if inp == 10:
    rotate_l_prime()
if inp == 11:
    rotate_r()
if inp == 12:
    rotate_r_prime()
";

        public const string USER_CONTROLLED_CODE = @"# KEYBOARD CONTROL
# U/I = U/U'   J/K = D/D'   F/G = F/F'
# V/B = B/B'   D/E = L/L'   H/Y = R/R'
inp = get_input()
if inp == 1:
    rotate_u()
if inp == 2:
    rotate_u_prime()
if inp == 3:
    rotate_d()
if inp == 4:
    rotate_d_prime()
if inp == 5:
    rotate_f()
if inp == 6:
    rotate_f_prime()
if inp == 7:
    rotate_b()
if inp == 8:
    rotate_b_prime()
if inp == 9:
    rotate_l()
if inp == 10:
    rotate_l_prime()
if inp == 11:
    rotate_r()
if inp == 12:
    rotate_r_prime()
";

        public const string EASY_AI_CODE = @"# EASY AI — Random face rotations
# Picks a random face and direction each tick.
# Brute-force: eventually solves via random walk.
face = get_moves() % 6
dir = 1
if get_moves() % 3 == 0:
    dir = -1
rotate(face, dir)
";

        public const string MEDIUM_AI_CODE = @"# MEDIUM AI — White cross solver
# Step 1: Solve the white (U=0) cross edges.
# Edge positions on U face: 1(top), 3(left), 5(right), 7(bottom)
# Target: U-face edges match U color AND adjacent face centers.
#
# Checks each U edge. If wrong, tries rotations to fix it.

u = 0
f = 2
r = 5
l = 4
d = 1

# Check U-top edge (pos 1) — should be white, B-center adjacent
s = get_sticker(u, 1)
if s != u:
    rotate_b()
    rotate_u()

# Check U-left edge (pos 3) — should be white
s = get_sticker(u, 3)
if s != u:
    rotate_l()
    rotate_u_prime()

# Check U-right edge (pos 5) — should be white
s = get_sticker(u, 5)
if s != u:
    rotate_r_prime()
    rotate_u()

# Check U-bottom edge (pos 7) — should be white
s = get_sticker(u, 7)
if s != u:
    rotate_f()
    rotate_f()

# If no U edges are wrong, try random moves on lower layers
sf = get_solved_faces()
if sf < 1:
    rotate_d()
    rotate_f()
elif sf < 6:
    # Try to solve more layers
    rotate_r()
    rotate_d()
    rotate_r_prime()
";

        public const string HARD_AI_CODE = @"# HARD AI — Layer-by-layer approach
# Uses simple heuristics to solve progressively:
# 1. White cross  2. White corners  3. Middle edges  4. Last layer
#
# Encodes common move sequences as function-like blocks.

u = 0
d = 1
f = 2
b = 3
l = 4
r = 5
stickers = get_solved_stickers()
faces = get_solved_faces()

# Phase 1: If less than 12 stickers correct, work on white cross
if stickers < 12:
    # Find white edges not on U face and rotate them up
    s1 = get_sticker(f, 1)
    if s1 == u:
        rotate_f()
        rotate_f()
    s3 = get_sticker(l, 1)
    if s3 == u:
        rotate_l()
        rotate_l()
    s5 = get_sticker(r, 1)
    if s5 == u:
        rotate_r()
        rotate_r()
    s7 = get_sticker(b, 1)
    if s7 == u:
        rotate_b()
        rotate_b()
    # Fix orientation with D layer
    rotate_d()

# Phase 2: White face corners
elif stickers < 20:
    # R U R' U' (sexy move) to cycle corners
    rotate_r()
    rotate_u()
    rotate_r_prime()
    rotate_u_prime()

# Phase 3: Middle layer edges
elif stickers < 34:
    # U R U' R' U' F' U F
    rotate_u()
    rotate_r()
    rotate_u_prime()
    rotate_r_prime()
    rotate_u_prime()
    rotate_f_prime()
    rotate_u()
    rotate_f()

# Phase 4: Last layer (OLL/PLL simplified)
elif stickers < 54:
    # Sune: R U R' U R U2 R'
    rotate_r()
    rotate_u()
    rotate_r_prime()
    rotate_u()
    rotate_r()
    rotate_u()
    rotate_u()
    rotate_r_prime()
    # Then adjust with U turns
    rotate_u()
";

        /// <summary>True when the loaded script is the user-controlled keyboard script.</summary>
        public bool IsUserMode { get; private set; }

        public string CurrentSourceCode => _sourceCode;
        public System.Action OnCodeChanged;

        public void Initialize(RubikMatchManager match, RubikCube cube,
                               string initialCode = null, string programName = "RubikAI")
        {
            _match = match;
            _cube = cube;
            _compilerExt = new RubikCompilerExtension();

            _renderer = cube.GetComponent<RubikRenderer>();

            _programName = programName;
            _sourceCode = initialCode ?? DEFAULT_CODE;
            _autoRun = true;

            LoadAndRun(_sourceCode);
        }

        protected override void Update()
        {
            if (_executor == null || _program == null || _isPaused) return;
            if (_match == null || !_match.MatchInProgress || _match.GameOver) return;

            // Wait for the renderer to finish all pending animations
            // before executing the next instruction.  Without this, moves
            // arrive faster than animations complete, causing visual jumps.
            if (_renderer != null && _renderer.IsBusy)
            {
                _opAccumulator = 0f;
                return;
            }

            float timeScale = SimulationTime.Instance?.timeScale ?? 1f;
            if (SimulationTime.Instance != null && SimulationTime.Instance.isPaused) return;

            float simDelta = UnityEngine.Time.deltaTime * timeScale;
            _opAccumulator += simDelta * OPS_PER_SECOND;

            int opsToRun = (int)_opAccumulator;
            _opAccumulator -= opsToRun;

            for (int i = 0; i < opsToRun; i++)
            {
                if (_executor.State.IsHalted)
                {
                    _executor.State.PC = 0;
                    _executor.State.IsHalted = false;
                }
                _executor.ExecuteOne();
            }

            if (opsToRun > 0)
                ProcessEvents();
        }

        protected override IGameIOHandler CreateIOHandler()
        {
            _ioHandler = new RubikIOHandler(_match, _cube);
            return _ioHandler;
        }

        protected override CompiledProgram CompileSource(string source, string name)
        {
            return PythonCompiler.Compile(source, name, _compilerExt);
        }

        protected override void ProcessEvents()
        {
            if (_executor?.State == null) return;
            while (_executor.State.OutputEvents.Count > 0)
                _executor.State.OutputEvents.Dequeue();
        }

        public void UploadCode(string newSource)
        {
            _sourceCode = newSource ?? DEFAULT_CODE;
            _opAccumulator = 0;
            IsUserMode = (newSource == USER_CONTROLLED_CODE);
            LoadAndRun(_sourceCode);
            Debug.Log($"[RubikAI] Uploaded new code ({_program?.Instructions?.Length ?? 0} instructions)");
            OnCodeChanged?.Invoke();
        }

        public void ResetExecution()
        {
            if (_executor?.State == null) return;
            _executor.State.Reset();
            _opAccumulator = 0f;
        }
    }
}
