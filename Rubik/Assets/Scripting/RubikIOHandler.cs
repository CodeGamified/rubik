// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using CodeGamified.Engine;
using CodeGamified.Time;
using Rubik.Game;

namespace Rubik.Scripting
{
    /// <summary>
    /// Game I/O handler — bridges CUSTOM opcodes to Rubik's Cube state.
    /// </summary>
    public class RubikIOHandler : IGameIOHandler
    {
        private readonly RubikMatchManager _match;
        private readonly RubikCube _cube;

        public RubikIOHandler(RubikMatchManager match, RubikCube cube)
        {
            _match = match;
            _cube = cube;
        }

        public bool PreExecute(Instruction inst, MachineState state) => true;

        public void ExecuteIO(Instruction inst, MachineState state)
        {
            int op = (int)inst.Op - (int)OpCode.CUSTOM_0;

            switch ((RubikOpCode)op)
            {
                // ── Queries → R0 ──
                case RubikOpCode.GET_SOLVED:
                    state.SetRegister(0, _cube.IsSolved ? 1f : 0f);
                    break;
                case RubikOpCode.GET_SOLVED_FACES:
                    state.SetRegister(0, _cube.SolvedFaceCount());
                    break;
                case RubikOpCode.GET_SOLVED_STICKERS:
                    state.SetRegister(0, _cube.SolvedStickerCount());
                    break;
                case RubikOpCode.GET_MOVES:
                    state.SetRegister(0, _cube.MoveCount);
                    break;
                case RubikOpCode.GET_SCRAMBLE_LEN:
                    state.SetRegister(0, _match.ScrambleLength);
                    break;
                case RubikOpCode.GET_SCORE:
                    state.SetRegister(0, _match.Score);
                    break;
                case RubikOpCode.GET_GAME_OVER:
                    state.SetRegister(0, _match.GameOver ? 1f : 0f);
                    break;
                case RubikOpCode.GET_INPUT:
                    state.SetRegister(0, RubikInputProvider.Instance != null
                        ? RubikInputProvider.Instance.CurrentInput : 0f);
                    break;

                // ── Two-arg query: R0=face, R1=pos → R0 ──
                case RubikOpCode.GET_STICKER:
                {
                    int face = (int)state.GetRegister(0);
                    int pos = (int)state.GetRegister(1);
                    state.SetRegister(0, _cube.GetSticker(face, pos));
                    break;
                }

                // ── One-arg query: R0=face → R0 ──
                case RubikOpCode.GET_FACE_SOLVED:
                {
                    int face = (int)state.GetRegister(0);
                    state.SetRegister(0, _cube.IsFaceSolved(face) ? 1f : 0f);
                    break;
                }

                // ── Named rotation commands → R0 (1=success, 0=fail) ──
                case RubikOpCode.ROTATE_U:       state.SetRegister(0, _match.DoRotate(RubikCube.U,  1)); break;
                case RubikOpCode.ROTATE_U_PRIME: state.SetRegister(0, _match.DoRotate(RubikCube.U, -1)); break;
                case RubikOpCode.ROTATE_D:       state.SetRegister(0, _match.DoRotate(RubikCube.D,  1)); break;
                case RubikOpCode.ROTATE_D_PRIME: state.SetRegister(0, _match.DoRotate(RubikCube.D, -1)); break;
                case RubikOpCode.ROTATE_F:       state.SetRegister(0, _match.DoRotate(RubikCube.F,  1)); break;
                case RubikOpCode.ROTATE_F_PRIME: state.SetRegister(0, _match.DoRotate(RubikCube.F, -1)); break;
                case RubikOpCode.ROTATE_B:       state.SetRegister(0, _match.DoRotate(RubikCube.B,  1)); break;
                case RubikOpCode.ROTATE_B_PRIME: state.SetRegister(0, _match.DoRotate(RubikCube.B, -1)); break;
                case RubikOpCode.ROTATE_L:       state.SetRegister(0, _match.DoRotate(RubikCube.L,  1)); break;
                case RubikOpCode.ROTATE_L_PRIME: state.SetRegister(0, _match.DoRotate(RubikCube.L, -1)); break;
                case RubikOpCode.ROTATE_R:       state.SetRegister(0, _match.DoRotate(RubikCube.R,  1)); break;
                case RubikOpCode.ROTATE_R_PRIME: state.SetRegister(0, _match.DoRotate(RubikCube.R, -1)); break;

                // ── Generic rotate(face, dir) ──
                case RubikOpCode.ROTATE:
                {
                    int face = (int)state.GetRegister(0);
                    int dir = (int)state.GetRegister(1);
                    state.SetRegister(0, _match.DoRotate(face, dir));
                    break;
                }
            }
        }

        public float GetTimeScale() => SimulationTime.Instance?.timeScale ?? 1f;
        public double GetSimulationTime() => SimulationTime.Instance?.simulationTime ?? 0.0;
    }
}
