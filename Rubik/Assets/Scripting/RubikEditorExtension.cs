// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System.Collections.Generic;
using CodeGamified.Editor;

namespace Rubik.Scripting
{
    /// <summary>
    /// Editor extension — provides Rubik-specific function metadata
    /// to CodeEditorWindow's option tree for tap-to-code editing.
    /// </summary>
    public class RubikEditorExtension : IEditorExtension
    {
        public List<EditorTypeInfo> GetAvailableTypes() => new();

        public List<EditorFuncInfo> GetAvailableFunctions() => new()
        {
            // Queries
            new EditorFuncInfo { Name = "get_solved",          Hint = "1 if cube solved",            ArgCount = 0 },
            new EditorFuncInfo { Name = "get_solved_faces",    Hint = "count of solved faces (0-6)", ArgCount = 0 },
            new EditorFuncInfo { Name = "get_solved_stickers", Hint = "correct stickers (0-54)",     ArgCount = 0 },
            new EditorFuncInfo { Name = "get_moves",           Hint = "moves this solve",            ArgCount = 0 },
            new EditorFuncInfo { Name = "get_scramble_length", Hint = "scramble move count",         ArgCount = 0 },
            new EditorFuncInfo { Name = "get_sticker",         Hint = "color at (face, pos)",        ArgCount = 2 },
            new EditorFuncInfo { Name = "get_face_solved",     Hint = "1 if face solved",            ArgCount = 1 },
            new EditorFuncInfo { Name = "get_score",           Hint = "current score",               ArgCount = 0 },
            new EditorFuncInfo { Name = "get_game_over",       Hint = "1 if game over",              ArgCount = 0 },
            new EditorFuncInfo { Name = "get_input",           Hint = "keyboard input code",         ArgCount = 0 },

            // Named rotations
            new EditorFuncInfo { Name = "rotate_u",       Hint = "U face CW",    ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_u_prime", Hint = "U face CCW",   ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_d",       Hint = "D face CW",    ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_d_prime", Hint = "D face CCW",   ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_f",       Hint = "F face CW",    ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_f_prime", Hint = "F face CCW",   ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_b",       Hint = "B face CW",    ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_b_prime", Hint = "B face CCW",   ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_l",       Hint = "L face CW",    ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_l_prime", Hint = "L face CCW",   ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_r",       Hint = "R face CW",    ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate_r_prime", Hint = "R face CCW",   ArgCount = 0 },
            new EditorFuncInfo { Name = "rotate",         Hint = "rotate(face, dir)", ArgCount = 2 },
        };

        public List<EditorMethodInfo> GetMethodsForType(string typeName) => new();

        public List<string> GetVariableNameSuggestions() => new()
        {
            "face", "pos", "color", "stickers", "faces", "moves", "inp"
        };

        public List<string> GetStringLiteralSuggestions() => new();
    }
}
