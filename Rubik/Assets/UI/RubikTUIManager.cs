// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeGamified.TUI;
using CodeGamified.Settings;
using CodeGamified.Audio;
using Rubik.Game;
using Rubik.Scripting;

namespace Rubik.UI
{
    /// <summary>
    /// TUI Manager for Rubik — left debugger panel + right status panel.
    /// Same pattern as Tetris TUI manager.
    /// </summary>
    public class RubikTUIManager : MonoBehaviour, ISettingsListener
    {
        private RubikMatchManager _match;
        private RubikProgram _playerProgram;
        private Equalizer _equalizer;

        private Canvas _canvas;
        private RectTransform _canvasRect;

        private RubikCodeDebugger _debugger;
        private RectTransform _debuggerRect;

        private RubikStatusPanel _statusPanel;
        private RectTransform _statusPanelRect;

        private TUIEdgeDragger _debuggerRightEdge;
        private TUIEdgeDragger _statusLeftEdge;

        private TMP_FontAsset _font;
        private float _fontSize;

        private RectTransform[] _allPanelRects;

        public void Initialize(RubikMatchManager match, RubikProgram program,
                               Equalizer equalizer = null)
        {
            _match = match;
            _playerProgram = program;
            _equalizer = equalizer;
            _fontSize = SettingsBridge.FontSize;

            BuildCanvas();
            BuildPanels();
        }

        private void OnEnable()  => SettingsBridge.Register(this);
        private void OnDisable() => SettingsBridge.Unregister(this);

        public void OnSettingsChanged(SettingsSnapshot settings, SettingsCategory changed)
        {
            if (changed != SettingsCategory.Display) return;
            if (Mathf.Approximately(settings.FontSize, _fontSize)) return;
            _fontSize = settings.FontSize;
            RebuildPanels();
        }

        private void RebuildPanels()
        {
            if (_allPanelRects != null)
                foreach (var rt in _allPanelRects)
                    if (rt != null) Destroy(rt.gameObject);

            _debugger = null;
            _statusPanel = null;
            BuildPanels();
        }

        private void BuildCanvas()
        {
            var canvasGO = new GameObject("RubikTUI_Canvas");
            canvasGO.transform.SetParent(transform, false);

            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;
            _canvas.sortingOrder = 100;
            _canvas.planeDistance = 1f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            _canvasRect = canvasGO.GetComponent<RectTransform>();

            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void BuildPanels()
        {
            const float dLeft  = 0f;
            const float dRight = 0.33f;
            const float sLeft  = 0.67f;
            const float sRight = 1.0f;

            _debuggerRect = CreatePanel("Debugger",
                new Vector2(dLeft, 0f), new Vector2(dRight, 1f));
            _debugger = _debuggerRect.gameObject.AddComponent<RubikCodeDebugger>();
            AddPanelBackground(_debuggerRect);
            _debugger.InitializeProgrammatic(GetFont(), _fontSize,
                _debuggerRect.GetComponent<Image>());
            _debugger.SetTitle("YOUR CODE");
            _debugger.Bind(_playerProgram);

            _statusPanelRect = CreatePanel("StatusPanel",
                new Vector2(sLeft, 0f), new Vector2(sRight, 1f));
            _statusPanel = _statusPanelRect.gameObject.AddComponent<RubikStatusPanel>();
            AddPanelBackground(_statusPanelRect);
            _statusPanel.InitializeProgrammatic(GetFont(), _fontSize - 1f,
                _statusPanelRect.GetComponent<Image>());
            _statusPanel.Bind(_match, _playerProgram);
            if (_equalizer != null)
                _statusPanel.BindEqualizer(_equalizer);

            _allPanelRects = new[] { _debuggerRect, _statusPanelRect };
            LinkEdges();
        }

        private void LinkEdges()
        {
            _debuggerRightEdge = TUIEdgeDragger.Create(
                _debuggerRect, _canvasRect, TUIEdgeDragger.Edge.Right);
            _statusLeftEdge = TUIEdgeDragger.Create(
                _statusPanelRect, _canvasRect, TUIEdgeDragger.Edge.Left);
        }

        private RectTransform CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvasRect, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private void AddPanelBackground(RectTransform panel)
        {
            var img = panel.gameObject.GetComponent<Image>();
            if (img == null)
                img = panel.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            img.raycastTarget = true;
        }

        private TMP_FontAsset GetFont()
        {
            if (_font != null) return _font;
            _font = Resources.Load<TMP_FontAsset>("Unifont SDF");
            return _font;
        }
    }
}
