// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using CodeGamified.TUI;
using Rubik.Scripting;

namespace Rubik.UI
{
    /// <summary>
    /// Thin adapter — wires a RubikProgram into the engine's CodeDebuggerWindow
    /// via RubikDebuggerData (IDebuggerDataSource).
    /// </summary>
    public class RubikCodeDebugger : CodeDebuggerWindow
    {
        protected override void Awake()
        {
            base.Awake();
            windowTitle = "CODE";
        }

        public void Bind(RubikProgram program)
        {
            SetDataSource(new RubikDebuggerData(program));
        }
    }
}
