// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rubik.Scripting
{
    /// <summary>
    /// Captures keyboard input for Rubik's Cube face rotations.
    /// Encodes as a single float:
    ///   0=none, 1=U, 2=U', 3=D, 4=D', 5=F, 6=F',
    ///   7=B, 8=B', 9=L, 10=L', 11=R, 12=R'
    ///
    /// Key bindings:
    ///   U/I = U/U'   J/K = D/D'   F/G = F/F'
    ///   V/B = B/B'   D/E = L/L'   H/Y = R/R'
    /// </summary>
    public class RubikInputProvider : MonoBehaviour
    {
        public static RubikInputProvider Instance { get; private set; }

        public float CurrentInput { get; private set; }

        private InputAction[] _actions;
        private float[] _inputValues;

        private static readonly (string name, string binding, float value)[] Bindings =
        {
            ("U_CW",   "<Keyboard>/u", 1f),
            ("U_CCW",  "<Keyboard>/i", 2f),
            ("D_CW",   "<Keyboard>/j", 3f),
            ("D_CCW",  "<Keyboard>/k", 4f),
            ("F_CW",   "<Keyboard>/f", 5f),
            ("F_CCW",  "<Keyboard>/g", 6f),
            ("B_CW",   "<Keyboard>/v", 7f),
            ("B_CCW",  "<Keyboard>/b", 8f),
            ("L_CW",   "<Keyboard>/d", 9f),
            ("L_CCW",  "<Keyboard>/e", 10f),
            ("R_CW",   "<Keyboard>/h", 11f),
            ("R_CCW",  "<Keyboard>/y", 12f),
        };

        private void Awake()
        {
            Instance = this;

            _actions = new InputAction[Bindings.Length];
            _inputValues = new float[Bindings.Length];

            for (int i = 0; i < Bindings.Length; i++)
            {
                _actions[i] = new InputAction(Bindings[i].name, InputActionType.Button);
                _actions[i].AddBinding(Bindings[i].binding);
                _actions[i].Enable();
                _inputValues[i] = Bindings[i].value;
            }
        }

        private void Update()
        {
            CurrentInput = 0f;
            for (int i = 0; i < _actions.Length; i++)
            {
                if (_actions[i].WasPressedThisFrame())
                {
                    CurrentInput = _inputValues[i];
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            if (_actions != null)
            {
                for (int i = 0; i < _actions.Length; i++)
                {
                    _actions[i]?.Disable();
                    _actions[i]?.Dispose();
                }
            }
            if (Instance == this) Instance = null;
        }
    }
}
