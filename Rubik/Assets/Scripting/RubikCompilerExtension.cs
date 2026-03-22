// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System.Collections.Generic;
using CodeGamified.Engine;
using CodeGamified.Engine.Compiler;

namespace Rubik.Scripting
{
    /// <summary>
    /// Opcode enum — CUSTOM_0..CUSTOM_N for Rubik's Cube.
    ///
    /// Convention:
    ///   - Queries first (read cube state → R0)
    ///   - Commands last (act on cube, result → R0: 1=success, 0=fail)
    /// </summary>
    public enum RubikOpCode
    {
        // ── Queries (no args, result in R0) ──
        GET_SOLVED          = 0,   // 1 if whole cube solved, 0 otherwise
        GET_SOLVED_FACES    = 1,   // count of fully solved faces (0-6)
        GET_SOLVED_STICKERS = 2,   // count of stickers in correct position (0-54)
        GET_MOVES           = 3,   // total moves made this solve
        GET_SCRAMBLE_LEN    = 4,   // number of scramble moves
        GET_SCORE           = 5,   // current score
        GET_GAME_OVER       = 6,   // 1 if game over
        GET_INPUT           = 7,   // keyboard input code

        // ── Queries with args ──
        GET_STICKER         = 8,   // get_sticker(face, pos) → color (0-5)
        GET_FACE_SOLVED     = 9,   // get_face_solved(face) → 0/1

        // ── Commands (args in R0[,R1], result in R0) ──
        ROTATE_U            = 10,  // rotate U face CW
        ROTATE_U_PRIME      = 11,  // rotate U face CCW
        ROTATE_D            = 12,  // rotate D face CW
        ROTATE_D_PRIME      = 13,  // rotate D face CCW
        ROTATE_F            = 14,  // rotate F face CW
        ROTATE_F_PRIME      = 15,  // rotate F face CCW
        ROTATE_B            = 16,  // rotate B face CW
        ROTATE_B_PRIME      = 17,  // rotate B face CCW
        ROTATE_L            = 18,  // rotate L face CW
        ROTATE_L_PRIME      = 19,  // rotate L face CCW
        ROTATE_R            = 20,  // rotate R face CW
        ROTATE_R_PRIME      = 21,  // rotate R face CCW
        ROTATE              = 22,  // rotate(face, dir) — generic
    }

    /// <summary>
    /// Compiler extension — one case per builtin function name.
    /// </summary>
    public class RubikCompilerExtension : ICompilerExtension
    {
        public void RegisterBuiltins(CompilerContext ctx) { }

        public bool TryCompileCall(string functionName, List<AstNodes.ExprNode> args,
                                   CompilerContext ctx, int sourceLine)
        {
            switch (functionName)
            {
                // ── Queries: no args ──
                case "get_solved":
                    Emit(ctx, RubikOpCode.GET_SOLVED, sourceLine, "get_solved → R0");
                    return true;
                case "get_solved_faces":
                    Emit(ctx, RubikOpCode.GET_SOLVED_FACES, sourceLine, "get_solved_faces → R0");
                    return true;
                case "get_solved_stickers":
                    Emit(ctx, RubikOpCode.GET_SOLVED_STICKERS, sourceLine, "get_solved_stickers → R0");
                    return true;
                case "get_moves":
                    Emit(ctx, RubikOpCode.GET_MOVES, sourceLine, "get_moves → R0");
                    return true;
                case "get_scramble_length":
                    Emit(ctx, RubikOpCode.GET_SCRAMBLE_LEN, sourceLine, "get_scramble_length → R0");
                    return true;
                case "get_score":
                    Emit(ctx, RubikOpCode.GET_SCORE, sourceLine, "get_score → R0");
                    return true;
                case "get_game_over":
                    Emit(ctx, RubikOpCode.GET_GAME_OVER, sourceLine, "get_game_over → R0");
                    return true;
                case "get_input":
                    Emit(ctx, RubikOpCode.GET_INPUT, sourceLine, "get_input → R0");
                    return true;

                // ── Two-arg query: get_sticker(face, pos) ──
                case "get_sticker":
                    if (args != null && args.Count >= 2)
                    {
                        args[0].Compile(ctx);
                        ctx.Emit(OpCode.PUSH, 0);
                        args[1].Compile(ctx);
                        ctx.Emit(OpCode.MOV, 1, 0);
                        ctx.Emit(OpCode.POP, 0);
                    }
                    ctx.Emit(OpCode.CUSTOM_0 + (int)RubikOpCode.GET_STICKER, 0, 0, 0, sourceLine,
                        "get_sticker(R0=face, R1=pos) → R0");
                    return true;

                // ── One-arg query: get_face_solved(face) ──
                case "get_face_solved":
                    if (args != null && args.Count >= 1)
                        args[0].Compile(ctx);
                    Emit(ctx, RubikOpCode.GET_FACE_SOLVED, sourceLine, "get_face_solved(R0=face) → R0");
                    return true;

                // ── Named rotation commands (no args) ──
                case "rotate_u":       Emit(ctx, RubikOpCode.ROTATE_U, sourceLine, "U");  return true;
                case "rotate_u_prime": Emit(ctx, RubikOpCode.ROTATE_U_PRIME, sourceLine, "U'"); return true;
                case "rotate_d":       Emit(ctx, RubikOpCode.ROTATE_D, sourceLine, "D");  return true;
                case "rotate_d_prime": Emit(ctx, RubikOpCode.ROTATE_D_PRIME, sourceLine, "D'"); return true;
                case "rotate_f":       Emit(ctx, RubikOpCode.ROTATE_F, sourceLine, "F");  return true;
                case "rotate_f_prime": Emit(ctx, RubikOpCode.ROTATE_F_PRIME, sourceLine, "F'"); return true;
                case "rotate_b":       Emit(ctx, RubikOpCode.ROTATE_B, sourceLine, "B");  return true;
                case "rotate_b_prime": Emit(ctx, RubikOpCode.ROTATE_B_PRIME, sourceLine, "B'"); return true;
                case "rotate_l":       Emit(ctx, RubikOpCode.ROTATE_L, sourceLine, "L");  return true;
                case "rotate_l_prime": Emit(ctx, RubikOpCode.ROTATE_L_PRIME, sourceLine, "L'"); return true;
                case "rotate_r":       Emit(ctx, RubikOpCode.ROTATE_R, sourceLine, "R");  return true;
                case "rotate_r_prime": Emit(ctx, RubikOpCode.ROTATE_R_PRIME, sourceLine, "R'"); return true;

                // ── Generic rotate(face, direction) ──
                case "rotate":
                    if (args != null && args.Count >= 2)
                    {
                        args[0].Compile(ctx);
                        ctx.Emit(OpCode.PUSH, 0);
                        args[1].Compile(ctx);
                        ctx.Emit(OpCode.MOV, 1, 0);
                        ctx.Emit(OpCode.POP, 0);
                    }
                    ctx.Emit(OpCode.CUSTOM_0 + (int)RubikOpCode.ROTATE, 0, 0, 0, sourceLine,
                        "rotate(R0=face, R1=dir) → R0");
                    return true;

                default:
                    return false;
            }
        }

        private static void Emit(CompilerContext ctx, RubikOpCode op, int line, string comment)
        {
            ctx.Emit(OpCode.CUSTOM_0 + (int)op, 0, 0, 0, line, comment);
        }

        public bool TryCompileMethodCall(string objectName, string methodName,
                                         List<AstNodes.ExprNode> args,
                                         CompilerContext ctx, int sourceLine) => false;

        public bool TryCompileObjectDecl(string typeName, string varName,
                                         List<AstNodes.ExprNode> constructorArgs,
                                         CompilerContext ctx, int sourceLine) => false;
    }
}
