// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System.Collections.Generic;
using UnityEngine;
using CodeGamified.Engine;
using CodeGamified.Engine.Runtime;
using CodeGamified.TUI;
using Rubik.Scripting;
using static Rubik.Scripting.RubikOpCode;

namespace Rubik.UI
{
    /// <summary>
    /// Adapts a RubikProgram into the engine's IDebuggerDataSource contract.
    /// </summary>
    public class RubikDebuggerData : IDebuggerDataSource
    {
        private readonly RubikProgram _program;
        private readonly string _label;

        public RubikDebuggerData(RubikProgram program, string label = null)
        {
            _program = program;
            _label = label;
        }

        public string ProgramName => _label ?? _program?.ProgramName ?? "RubikAI";
        public string[] SourceLines => _program?.Program?.SourceLines;
        public bool HasLiveProgram =>
            _program != null && _program.Executor != null && _program.Program != null
            && _program.Program.Instructions != null && _program.Program.Instructions.Length > 0;
        public int PC
        {
            get
            {
                var s = _program?.State;
                if (s == null) return 0;
                return s.LastExecutedPC >= 0 ? s.LastExecutedPC : s.PC;
            }
        }
        public long CycleCount => _program?.State?.CycleCount ?? 0;

        public string StatusString
        {
            get
            {
                if (_program == null || _program.Executor == null)
                    return TUIColors.Dimmed("NO PROGRAM");
                var state = _program.State;
                if (state == null) return TUIColors.Dimmed("NO STATE");
                int instCount = _program.Program?.Instructions?.Length ?? 0;
                return TUIColors.Fg(TUIColors.BrightGreen, $"TICK {instCount} inst");
            }
        }

        public List<string> BuildSourceLines(int pc, int scrollOffset, int maxRows)
        {
            var lines = new List<string>();
            var src = SourceLines;
            if (src == null) return lines;

            int activeLine = -1;
            int activeEnd = -1;
            bool isHalt = false;
            Instruction activeInst = default;
            if (HasLiveProgram && _program.Program.Instructions.Length > 0
                && pc < _program.Program.Instructions.Length)
            {
                activeInst = _program.Program.Instructions[pc];
                activeLine = activeInst.SourceLine - 1;
                isHalt = activeInst.Op == OpCode.HALT;
                if (activeLine >= 0)
                    activeEnd = SourceHighlight.GetContinuationEnd(src, activeLine);
            }

            if (scrollOffset == 0 && lines.Count < maxRows)
            {
                string whileLine = "while True:";
                if (isHalt)
                    lines.Add(TUIColors.Fg(TUIColors.BrightGreen, $"  {TUIGlyphs.ArrowR}   {whileLine}"));
                else
                    lines.Add($"  {TUIColors.Dimmed(TUIGlyphs.ArrowR)}   {SynthwaveHighlighter.Highlight(whileLine)}");
            }

            int tokenLine = -1;
            if (activeLine >= 0)
            {
                string token = SourceHighlight.GetSourceToken(activeInst);
                if (token != null)
                {
                    for (int k = activeLine; k <= activeEnd; k++)
                    {
                        if (src[k].IndexOf(token) >= 0) { tokenLine = k; break; }
                    }
                }
                if (tokenLine < 0) tokenLine = activeLine;
            }

            int focusLine = tokenLine >= 0 ? tokenLine : activeLine;
            int autoScroll = scrollOffset;
            if (focusLine >= 0 && scrollOffset == 0)
            {
                int margin = Mathf.Max(3, maxRows / 4);
                int visibleEnd = autoScroll + maxRows - 2;
                if (focusLine < autoScroll + margin)
                    autoScroll = Mathf.Max(0, focusLine - margin);
                else if (focusLine > visibleEnd - margin)
                    autoScroll = Mathf.Max(0, focusLine - maxRows + margin + 2);
            }

            for (int i = autoScroll; i < src.Length && lines.Count < maxRows; i++)
            {
                bool isActive = (i >= activeLine && i <= activeEnd);
                bool isTokenLine = (i == tokenLine);
                string raw = src[i];
                string highlighted = SynthwaveHighlighter.Highlight(raw);

                string pointer = isTokenLine
                    ? TUIColors.Fg(TUIColors.BrightGreen, TUIGlyphs.ArrowR)
                    : " ";
                string lineNum = TUIColors.Dimmed($"{(i + 1),3}");

                if (isActive)
                    lines.Add($"  {pointer} {lineNum} {TUIColors.Fg(TUIColors.BrightGreen, raw)}");
                else
                    lines.Add($"  {pointer} {lineNum} {highlighted}");
            }

            return lines;
        }

        public List<string> BuildMachineLines(int pc, int maxRows)
        {
            var lines = new List<string>();
            if (!HasLiveProgram) return lines;

            var instructions = _program.Program.Instructions;
            int total = instructions.Length;

            int offset = 0;
            if (total > maxRows)
                offset = Mathf.Clamp(pc - maxRows / 3, 0, total - maxRows);
            int visibleCount = Mathf.Min(maxRows, total);

            for (int j = 0; j < visibleCount; j++)
            {
                int i = offset + j;
                var inst = instructions[i];
                bool isPC = (i == pc);
                string asm = inst.ToAssembly(FormatRubikOp);
                if (isPC)
                {
                    lines.Add(TUIColors.Fg(TUIColors.BrightGreen, $" {i:X3}  {asm}"));
                }
                else
                {
                    string addr = TUIColors.Dimmed($"{i:X3}");
                    lines.Add($" {addr}  {SynthwaveHighlighter.HighlightAsm(asm)}");
                }
            }
            return lines;
        }

        public List<string> BuildStateLines()
        {
            if (!HasLiveProgram) return new List<string>();
            var s = _program.State;
            int displayPC = s.LastExecutedPC >= 0 ? s.LastExecutedPC : s.PC;
            return TUIWidgets.BuildStateLines(
                s.Registers, s.LastRegisterModified,
                s.Flags, displayPC, s.Stack.Count,
                s.NameToAddress, s.Memory);
        }

        static string FormatRubikOp(Instruction inst)
        {
            int id = (int)inst.Op - (int)OpCode.CUSTOM_0;
            return (RubikOpCode)id switch
            {
                GET_SOLVED          => "INP R0, SOLVD",
                GET_SOLVED_FACES    => "INP R0, FACES",
                GET_SOLVED_STICKERS => "INP R0, STCKR",
                GET_MOVES           => "INP R0, MOVES",
                GET_SCRAMBLE_LEN    => "INP R0, SCRML",
                GET_SCORE           => "INP R0, SCORE",
                GET_GAME_OVER       => "INP R0, G.OVR",
                GET_INPUT           => "INP R0, INPUT",
                GET_STICKER         => "INP R0, STK",
                GET_FACE_SOLVED     => "INP R0, F.SLV",
                ROTATE_U            => "OUT ROT.U",
                ROTATE_U_PRIME      => "OUT ROT.U'",
                ROTATE_D            => "OUT ROT.D",
                ROTATE_D_PRIME      => "OUT ROT.D'",
                ROTATE_F            => "OUT ROT.F",
                ROTATE_F_PRIME      => "OUT ROT.F'",
                ROTATE_B            => "OUT ROT.B",
                ROTATE_B_PRIME      => "OUT ROT.B'",
                ROTATE_L            => "OUT ROT.L",
                ROTATE_L_PRIME      => "OUT ROT.L'",
                ROTATE_R            => "OUT ROT.R",
                ROTATE_R_PRIME      => "OUT ROT.R'",
                ROTATE              => "OUT ROTATE",
                _                   => $"IO.{id,2} {inst.Arg0}, {inst.Arg1}"
            };
        }
    }
}
