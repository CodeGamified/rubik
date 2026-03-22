// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using CodeGamified.Audio;
using CodeGamified.TUI;
using CodeGamified.Time;
using CodeGamified.Settings;
using CodeGamified.Quality;
using UnityEngine.SceneManagement;
using Rubik.Game;
using Rubik.Scripting;

namespace Rubik.UI
{
    /// <summary>
    /// Right-side status panel — 6 vertical sections with draggable row dividers.
    /// Mirrors Tetris status panel layout.
    /// </summary>
    public class RubikStatusPanel : TerminalWindow
    {
        private RubikMatchManager _match;
        private RubikProgram _playerProgram;
        private Equalizer _equalizer;

        private string _scriptLabel;

        private const int SECTION_COUNT = 6;
        private const int SEC_TITLE    = 0;
        private const int SEC_MATCH    = 1;
        private const int SEC_SCRIPT   = 2;
        private const int SEC_SETTINGS = 3;
        private const int SEC_CONTROLS = 4;
        private const int SEC_AUDIO    = 5;

        private int[] _sectionRows;
        private float[] _sectionRatios = { 0f, 0.12f, 0.28f, 0.48f, 0.65f, 0.82f };
        private TUIRowDragger[] _rowDraggers;
        private bool _sectionsReady;

        private TUIOverlayBinding _overlays;

        // ASCII art animation
        private float _asciiTimer;
        private int _asciiPhase;
        private float[] _revealThresholds;
        private const float AsciiHold = 5f;
        private const float AsciiAnim = 1f;
        private const int AsciiWordCount = 2;
        private static readonly char[] GlitchGlyphs =
            "░▒▓█▀▄▌▐╬╫╪╩╦╠╣─│┌┐└┘├┤┬┴┼".ToCharArray();

        private static readonly string[][] AsciiWords =
        {
            new[] // RUBIK
            {
                " █████████  ██    ██  █████████  ██  ██    ██ ",
                " ██      ██ ██    ██  ██      ██ ██  ██  ██   ",
                " █████████  ██    ██  █████████  ██  ████     ",
                " ██    ██   ██    ██  ██      ██ ██  ██  ██   ",
                " ██     ██  ████████  █████████  ██  ██    ██ ",
            },
            new[] // SOLVE
            {
                " █████████  ██████████ ██       ██    ██ █████████ ",
                " ██         ██      ██ ██       ██    ██ ██        ",
                " █████████  ██      ██ ██       ██    ██ ██████    ",
                "        ██  ██      ██ ██        ██  ██  ██        ",
                " █████████  ██████████ ████████   ████   █████████ ",
            },
        };

        protected override void Awake()
        {
            base.Awake();
            windowTitle = "RUBIK";
            totalRows = 40;
        }

        public void Bind(RubikMatchManager match, RubikProgram playerProgram)
        {
            _match = match;
            _playerProgram = playerProgram;
        }

        public void BindEqualizer(Equalizer equalizer) => _equalizer = equalizer;

        protected override void OnLayoutReady()
        {
            SetupSections();
        }

        protected override void Update()
        {
            base.Update();
            if (!rowsReady) return;
            _equalizer?.Update(UnityEngine.Time.deltaTime);
            AdvanceAsciiTimer();
            HandleInput();
        }

        // ═══════════════════════════════════════════════════════════════
        // SECTION LAYOUT
        // ═══════════════════════════════════════════════════════════════

        private void SetupSections()
        {
            ComputeSectionRows();
            _sectionsReady = true;

            if (_rowDraggers == null)
            {
                _rowDraggers = new TUIRowDragger[SECTION_COUNT - 1];
                for (int i = 0; i < SECTION_COUNT - 1; i++)
                {
                    int idx = i;
                    int minRow = (i > 0 ? _sectionRows[i] : 1) + 1;
                    int maxRow = (i + 2 < SECTION_COUNT ? _sectionRows[i + 2] : totalRows) - 1;
                    _rowDraggers[i] = AddRowDragger(
                        _sectionRows[i + 1], minRow, maxRow, pos => OnRowDragged(idx, pos));
                }
            }
            else
            {
                float rh = rows.Count > 0 ? rows[0].RowHeight : 18f;
                for (int i = 0; i < SECTION_COUNT - 1; i++)
                {
                    _rowDraggers[i].UpdateRowHeight(rh);
                    _rowDraggers[i].UpdatePosition(_sectionRows[i + 1]);
                    UpdateDraggerLimits(i);
                }
            }

            BuildAndApplyOverlays();
        }

        private void ComputeSectionRows()
        {
            _sectionRows = new int[SECTION_COUNT];
            _sectionRows[0] = 0;
            for (int i = 1; i < SECTION_COUNT; i++)
            {
                int minRow = _sectionRows[i - 1] + 1;
                int maxRow = totalRows - (SECTION_COUNT - i);
                _sectionRows[i] = Mathf.Clamp(
                    Mathf.RoundToInt(totalRows * _sectionRatios[i]), minRow, maxRow);
            }
        }

        private void OnRowDragged(int draggerIndex, int newRow)
        {
            int secIdx = draggerIndex + 1;
            _sectionRows[secIdx] = newRow;
            _sectionRatios[secIdx] = (float)newRow / totalRows;
            if (draggerIndex > 0) UpdateDraggerLimits(draggerIndex - 1);
            if (draggerIndex < SECTION_COUNT - 2) UpdateDraggerLimits(draggerIndex + 1);
            if (_overlays != null)
                _overlays.Apply(rows, null, totalChars);
        }

        private void UpdateDraggerLimits(int draggerIdx)
        {
            int minRow = _sectionRows[draggerIdx] + 1;
            int maxRow = (draggerIdx + 2 < SECTION_COUNT ? _sectionRows[draggerIdx + 2] : totalRows) - 1;
            _rowDraggers[draggerIdx].UpdateLimits(minRow, maxRow);
        }

        private int SectionStart(int sec) => _sectionRows[sec];
        private int SectionEnd(int sec) => sec + 1 < SECTION_COUNT ? _sectionRows[sec + 1] : totalRows;

        // ═══════════════════════════════════════════════════════════════
        // INPUT
        // ═══════════════════════════════════════════════════════════════

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                LoadScript(RubikProgram.EASY_AI_CODE, "Easy");
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                LoadScript(RubikProgram.MEDIUM_AI_CODE, "Medium");
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                LoadScript(RubikProgram.HARD_AI_CODE, "Hard");
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                LoadScript(RubikProgram.USER_CONTROLLED_CODE, "Keyboard");
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            { _playerProgram?.UploadCode(null); _scriptLabel = null; }

            if (Input.GetKeyDown(KeyCode.P))
                SimulationTime.Instance?.TogglePause();
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (Input.GetKeyDown(KeyCode.F5))
                SettingsBridge.SetMasterVolume(SettingsBridge.MasterVolume + (shift ? -0.1f : 0.1f));
            if (Input.GetKeyDown(KeyCode.F6))
                SettingsBridge.SetMusicVolume(SettingsBridge.MusicVolume + (shift ? -0.1f : 0.1f));
            if (Input.GetKeyDown(KeyCode.F7))
                SettingsBridge.SetSfxVolume(SettingsBridge.SfxVolume + (shift ? -0.1f : 0.1f));
        }

        private void LoadScript(string code, string label)
        {
            if (_playerProgram == null) return;
            _playerProgram.UploadCode(code);
            _scriptLabel = label;
        }

        // ═══════════════════════════════════════════════════════════════
        // OVERLAY BINDINGS
        // ═══════════════════════════════════════════════════════════════

        private void BuildAndApplyOverlays()
        {
            if (_overlays == null)
            {
                _overlays = new TUIOverlayBinding();

                int settingsBase = SectionStart(SEC_SETTINGS);
                _overlays.Slider(settingsBase + 1, -1,
                    () => SettingsBridge.QualityLevel / 3f,
                    v => { int lv = Mathf.RoundToInt(v * 3f); SettingsBridge.SetQualityLevel(lv); QualityBridge.SetTier((QualityTier)lv); },
                    step: 1f / 3f);
                _overlays.Slider(settingsBase + 2, -1,
                    () => FontToSlider(SettingsBridge.FontSize),
                    v => SettingsBridge.SetFontSize(SliderToFont(v)),
                    step: 1f / 40f);

                int controlsBase = SectionStart(SEC_CONTROLS);
                _overlays.Slider(controlsBase + 1, -1,
                    () => SpeedToSlider(SimulationTime.Instance != null ? SimulationTime.Instance.timeScale : 1f),
                    v => SimulationTime.Instance?.SetTimeScale(SliderToSpeed(v)));

                int audioBase = SectionStart(SEC_AUDIO);
                _overlays.Slider(audioBase + 1, -1,
                    () => SettingsBridge.MasterVolume,
                    v => SettingsBridge.SetMasterVolume(v));
                _overlays.Slider(audioBase + 2, -1,
                    () => SettingsBridge.MusicVolume,
                    v => SettingsBridge.SetMusicVolume(v));
                _overlays.Slider(audioBase + 3, -1,
                    () => SettingsBridge.SfxVolume,
                    v => SettingsBridge.SetSfxVolume(v));

                // Script selection buttons
                int scriptBase = SectionStart(SEC_SCRIPT);
                Func<int, int, (int, int)> fullBtnLayout =
                    (cs, cw) => (cs + 2, Mathf.Max(4, cw - 2));
                _overlays.Button(scriptBase + 4, -1, fullBtnLayout,
                    _ => LoadScript(RubikProgram.EASY_AI_CODE, "Easy"));
                _overlays.Button(scriptBase + 5, -1, fullBtnLayout,
                    _ => LoadScript(RubikProgram.MEDIUM_AI_CODE, "Medium"));
                _overlays.Button(scriptBase + 6, -1, fullBtnLayout,
                    _ => LoadScript(RubikProgram.HARD_AI_CODE, "Hard"));
                _overlays.Button(scriptBase + 7, -1, fullBtnLayout,
                    _ => LoadScript(RubikProgram.USER_CONTROLLED_CODE, "Keyboard"));
                _overlays.Button(scriptBase + 8, -1, fullBtnLayout,
                    _ => { _playerProgram?.UploadCode(null); _scriptLabel = null; });
            }

            _overlays.Apply(rows, null, totalChars);
        }

        private static float SpeedToSlider(float speed)
        {
            speed = Mathf.Clamp(speed, 0.1f, 100f);
            return Mathf.Log10(speed * 10f) / 3f;
        }
        private static float SliderToSpeed(float slider) =>
            0.1f * Mathf.Pow(1000f, Mathf.Clamp01(slider));
        private static float FontToSlider(float fontSize) =>
            Mathf.Clamp01((fontSize - 8f) / 40f);
        private static float SliderToFont(float slider) =>
            8f + Mathf.Clamp01(slider) * 40f;

        // ═══════════════════════════════════════════════════════════════
        // RENDER
        // ═══════════════════════════════════════════════════════════════

        protected override void Render()
        {
            ClearAllRows();

            if (!_sectionsReady)
            {
                SetRow(0, $" {TUIColors.Bold("RUBIK")}");
                return;
            }

            _overlays?.Sync();

            RenderSection(SEC_TITLE,    BuildTitleSection);
            RenderSection(SEC_MATCH,    BuildMatchSection);
            RenderSection(SEC_SCRIPT,   BuildScriptSection);
            RenderSection(SEC_SETTINGS, BuildSettingsSection);
            RenderSection(SEC_CONTROLS, BuildControlsSection);
            RenderSection(SEC_AUDIO,    BuildAudioSection);

            for (int i = 1; i < SECTION_COUNT; i++)
            {
                int r = _sectionRows[i];
                if (r > 0 && r < totalRows)
                    SetRow(r, Separator());
            }
        }

        private void RenderSection(int sec, Func<int, string[]> builder)
        {
            int start = SectionStart(sec);
            int end = SectionEnd(sec);
            int contentStart = sec > 0 ? start + 1 : start;
            int contentHeight = end - contentStart;
            if (contentHeight <= 0) return;
            var lines = builder(contentHeight);
            for (int i = 0; i < lines.Length && i < contentHeight; i++)
                SetRow(contentStart + i, lines[i]);
        }

        // ── Section 0: TITLE ────────────────────────────────────

        private string[] BuildTitleSection(int maxRows)
        {
            var art = BuildAsciiArt(totalChars);
            int artWidth = art.Length > 0 ? VisibleLen(art[0]) : 0;
            int pad = Mathf.Max(0, (totalChars - artWidth) / 2);
            if (pad > 0)
            {
                string spaces = new string(' ', pad);
                for (int i = 0; i < art.Length; i++)
                    if (!string.IsNullOrEmpty(art[i]))
                        art[i] = spaces + art[i];
            }
            if (art.Length > maxRows)
            {
                var trimmed = new string[maxRows];
                System.Array.Copy(art, trimmed, maxRows);
                return trimmed;
            }
            return art;
        }

        // ── Section 1: MATCH / CUBE STATE ───────────────────────

        private string[] BuildMatchSection(int maxRows)
        {
            var lines = new List<string>();

            Color32 accent = TUIGradient.Sample(0.3f);
            lines.Add($" {TUIColors.Fg(accent, TUIGlyphs.DiamondFilled)} {TUIColors.Bold("CUBE")}");

            if (_match != null)
            {
                string emdash = "\u2014";
                var cube = _match.Cube;

                int stickers = cube != null ? cube.SolvedStickerCount() : 0;
                int faces = cube != null ? cube.SolvedFaceCount() : 0;
                int moves = cube != null ? cube.MoveCount : 0;
                bool solved = cube != null && cube.IsSolved;

                lines.Add($"  {TUIColors.Fg(TUIColors.BrightCyan, $"{stickers}/54")} {emdash} STICKERS");
                lines.Add($"  {TUIColors.Fg(TUIColors.BrightGreen, $"{faces}/6")} {emdash} FACES");
                lines.Add($"  {TUIColors.Fg(TUIColors.BrightYellow, $"{moves}")} {emdash} MOVES");
                lines.Add($"  {TUIColors.Fg(TUIColors.BrightMagenta, $"{_match.Score}")} {emdash} SCORE");

                // Face status mini-display
                if (cube != null)
                {
                    var sb = new StringBuilder("  ");
                    for (int f = 0; f < RubikCube.FaceCount; f++)
                    {
                        string name = RubikCube.FaceNames[f];
                        if (cube.IsFaceSolved(f))
                            sb.Append(TUIColors.Fg(TUIColors.BrightGreen, $"{name}{TUIGlyphs.Check} "));
                        else
                            sb.Append(TUIColors.Dimmed($"{name}· "));
                    }
                    lines.Add(sb.ToString());
                }

                if (solved)
                    lines.Add($"  {TUIColors.Fg(TUIColors.BrightGreen, "SOLVED!")}");
                else if (_match.GameOver)
                    lines.Add($"  {TUIColors.Fg(TUIColors.Red, "GAME OVER")}");
            }
            else
            {
                lines.Add(TUIColors.Dimmed("  No match"));
            }

            return Trim(lines, maxRows);
        }

        // ── Section 2: SCRIPT CONTROLS ──────────────────────────

        private string[] BuildScriptSection(int maxRows)
        {
            var lines = new List<string>();

            Color32 accent = TUIGradient.Sample(0.5f);
            lines.Add($" {TUIColors.Fg(accent, TUIGlyphs.DiamondFilled)} {TUIColors.Bold("SCRIPT")}");

            if (_playerProgram != null)
            {
                int inst = _playerProgram.Program?.Instructions?.Length ?? 0;
                string status = _playerProgram.IsRunning
                    ? TUIColors.Fg(TUIColors.BrightGreen, "RUN")
                    : TUIColors.Dimmed("STP");
                string label = _scriptLabel != null
                    ? TUIColors.Fg(TUIColors.BrightMagenta, $"({_scriptLabel})")
                    : TUIColors.Dimmed("(custom)");
                lines.Add($"  {status} {TUIColors.Dimmed($"{inst}i")} {label}");
            }
            else
            {
                lines.Add(TUIColors.Dimmed("  No program"));
            }

            lines.Add("");
            lines.Add(TUIColors.Dimmed("  LOAD"));

            string[] aiLabels = { "Easy", "Medium", "Hard" };
            for (int i = 0; i < aiLabels.Length; i++)
            {
                bool active = _scriptLabel == aiLabels[i];
                string key = TUIColors.Fg(TUIColors.BrightCyan, $"[{i + 1}]");
                string lbl = active
                    ? TUIColors.Fg(TUIColors.BrightGreen, $"{aiLabels[i]}{TUIGlyphs.ArrowL}")
                    : TUIColors.Dimmed(aiLabels[i]);
                lines.Add($"  {key} {lbl}");
            }
            {
                bool active = _scriptLabel == "Keyboard";
                string key = TUIColors.Fg(TUIColors.BrightCyan, "[4]");
                string lbl = active
                    ? TUIColors.Fg(TUIColors.BrightGreen, $"Keyboard{TUIGlyphs.ArrowL}")
                    : TUIColors.Dimmed("Keyboard");
                lines.Add($"  {key} {lbl}");
            }
            {
                string key = TUIColors.Fg(TUIColors.BrightCyan, "[5]");
                lines.Add($"  {key} {TUIColors.Dimmed("Reset")}");
            }

            lines.Add("");
            lines.Add($"  {TUIColors.Fg(TUIColors.BrightCyan, "[R]")} RELOAD SCENE");

            return Trim(lines, maxRows);
        }

        // ── Section 3: SETTINGS ─────────────────────────────────

        private string[] BuildSettingsSection(int maxRows)
        {
            var lines = new List<string>();
            int w = totalChars;

            Color32 accent = TUIGradient.Sample(0.6f);
            lines.Add($" {TUIColors.Fg(accent, TUIGlyphs.DiamondFilled)} {TUIColors.Bold("SETTINGS")}");

            float qualNorm = SettingsBridge.QualityLevel / 3f;
            string qualName = ((QualityTier)SettingsBridge.QualityLevel).ToString();
            if (qualName.Length > 4) qualName = qualName.Substring(0, 4);
            else qualName = qualName.PadRight(4);
            lines.Add(TUIWidgets.AdaptiveSliderRow(w, "QTY", qualNorm, qualName));

            float fontNorm = FontToSlider(SettingsBridge.FontSize);
            string fontStr = $"{SettingsBridge.FontSize,2:F0}pt";
            lines.Add(TUIWidgets.AdaptiveSliderRow(w, "FNT", fontNorm, fontStr));

            return Trim(lines, maxRows);
        }

        // ── Section 4: CONTROLS ─────────────────────────────────

        private string[] BuildControlsSection(int maxRows)
        {
            var lines = new List<string>();
            int w = totalChars;

            Color32 accent = TUIGradient.Sample(0.75f);
            lines.Add($" {TUIColors.Fg(accent, TUIGlyphs.DiamondFilled)} {TUIColors.Bold("CONTROLS")}");

            var sim = SimulationTime.Instance;
            float speed = sim != null ? sim.timeScale : 1f;
            float speedNorm = SpeedToSlider(speed);
            string speedFmt = speed < 10f ? $"{speed:F1}" : $"{speed:F0}";
            string speedStr = $"{speedFmt,3}x";
            string paused = (sim != null && sim.isPaused)
                ? TUIColors.Fg(TUIColors.BrightYellow, " PAUSED") : "";

            lines.Add(TUIWidgets.AdaptiveSliderRow(w, "SPD", speedNorm, speedStr) + paused);

            string pauseLabel = (sim != null && sim.isPaused) ? "PLAY" : "PAUSE";
            lines.Add($"  {TUIColors.Fg(TUIColors.BrightCyan, "[P]")} {pauseLabel}");

            return Trim(lines, maxRows);
        }

        // ── Section 5: AUDIO ────────────────────────────────────

        private string[] BuildAudioSection(int maxRows)
        {
            var lines = new List<string>();
            int w = totalChars;

            Color32 accent = TUIGradient.Sample(0.9f);
            lines.Add($" {TUIColors.Fg(accent, TUIGlyphs.DiamondFilled)} {TUIColors.Bold("AUDIO")}");

            lines.Add(TUIWidgets.AdaptiveSliderRow(w, "VOL", SettingsBridge.MasterVolume, $"{SettingsBridge.MasterVolume * 100:F0}%"));
            lines.Add(TUIWidgets.AdaptiveSliderRow(w, "MSC", SettingsBridge.MusicVolume, $"{SettingsBridge.MusicVolume * 100:F0}%"));
            lines.Add(TUIWidgets.AdaptiveSliderRow(w, "SFX", SettingsBridge.SfxVolume, $"{SettingsBridge.SfxVolume * 100:F0}%"));

            if (_equalizer != null)
            {
                int eqH = Mathf.Min(6, maxRows - lines.Count);
                if (eqH >= 1)
                {
                    var eqLines = TUIEqualizer.Render(
                        _equalizer.SmoothedBands,
                        _equalizer.PeakBands,
                        new TUIEqualizer.Config
                        {
                            Width      = w,
                            Height     = eqH,
                            Style      = TUIEqualizer.Style.Bars,
                            ShowBorder = false,
                            ShowPeaks  = true,
                            ShowLabels = false,
                        });
                    foreach (var line in eqLines)
                        lines.Add(line);
                }
            }

            return Trim(lines, maxRows);
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════

        private static string[] Trim(List<string> lines, int maxRows)
        {
            if (lines.Count <= maxRows) return lines.ToArray();
            var trimmed = new string[maxRows];
            for (int i = 0; i < maxRows; i++) trimmed[i] = lines[i];
            return trimmed;
        }

        private static int VisibleLen(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int count = 0;
            bool inTag = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '<') { inTag = true; continue; }
                if (text[i] == '>') { inTag = false; continue; }
                if (!inTag) count++;
            }
            return count;
        }

        // ═══════════════════════════════════════════════════════════════
        // ASCII ART ENGINE
        // ═══════════════════════════════════════════════════════════════

        private int AsciiPhaseCount => AsciiWordCount * 2;

        private void AdvanceAsciiTimer()
        {
            _asciiTimer += Time.deltaTime;
            bool isHold = (_asciiPhase % 2) == 0;
            float threshold = isHold ? AsciiHold : AsciiAnim;
            if (_asciiTimer >= threshold)
            {
                _asciiTimer = 0f;
                _asciiPhase = (_asciiPhase + 1) % AsciiPhaseCount;
                if ((_asciiPhase % 2) == 1) InitRevealThresholds();
            }
        }

        private void InitRevealThresholds()
        {
            int innerW = AsciiWords[0][0].Length;
            int total = innerW * 5;
            _revealThresholds = new float[total];
            for (int i = 0; i < total; i++) _revealThresholds[i] = UnityEngine.Random.value;
        }

        private string[] BuildAsciiArt(int maxWidth)
        {
            int wordIdx = (_asciiPhase / 2) % AsciiWordCount;
            int innerW = AsciiWords[wordIdx][0].Length;
            int clampedInner = Mathf.Min(innerW, Mathf.Max(0, maxWidth - 2));
            if ((_asciiPhase % 2) == 0)
                return ColorizeWord(AsciiWords[wordIdx], clampedInner);
            else
            {
                int nextIdx = (wordIdx + 1) % AsciiWordCount;
                return DecipherWord(AsciiWords[wordIdx], AsciiWords[nextIdx], clampedInner);
            }
        }

        private string GradientBorderH(char left, char fill, char right, int innerWidth)
        {
            int total = innerWidth + 2;
            var sb = new StringBuilder(total * 32);
            sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(0f), left.ToString()));
            for (int i = 0; i < innerWidth; i++)
            {
                float t = total > 1 ? (float)(i + 1) / (total - 1) : 0f;
                sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(t), fill.ToString()));
            }
            sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(1f), right.ToString()));
            return sb.ToString();
        }

        private string GradientBorderV(string rawContent)
        {
            var sb = new StringBuilder(rawContent.Length + 128);
            sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(0f), "║"));
            sb.Append(rawContent);
            sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(1f), "║"));
            return sb.ToString();
        }

        private string GradientRowRaw(string row, int totalBorderedWidth)
        {
            int len = row.Length;
            if (len == 0) return "";
            var sb = new StringBuilder(len * 32);
            for (int i = 0; i < len; i++)
            {
                float t = totalBorderedWidth > 1 ? (float)(i + 1) / (totalBorderedWidth - 1) : 0f;
                sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(t), row[i].ToString()));
            }
            return sb.ToString();
        }

        private string[] ColorizeWord(string[] word, int innerW)
        {
            int totalW = innerW + 2;
            var lines = new string[7];
            lines[0] = GradientBorderH('╔', '═', '╗', innerW);
            lines[1] = GradientBorderV(new string(' ', innerW));
            for (int i = 0; i < 5; i++)
            {
                string row = word[i].Length > innerW ? word[i].Substring(0, innerW) : word[i].PadRight(innerW);
                lines[2 + i] = GradientBorderV(GradientRowRaw(row, totalW));
            }
            return lines;
        }

        private string[] DecipherWord(string[] src, string[] tgt, int innerW)
        {
            float progress = Mathf.Clamp01(_asciiTimer / AsciiAnim);
            int totalW = innerW + 2;
            var lines = new string[7];
            lines[0] = GradientBorderH('╔', '═', '╗', innerW);
            lines[1] = GradientBorderV(new string(' ', innerW));
            for (int r = 0; r < 5; r++)
            {
                string s = src[r].Length > innerW ? src[r].Substring(0, innerW) : src[r].PadRight(innerW);
                string t = tgt[r].Length > innerW ? tgt[r].Substring(0, innerW) : tgt[r].PadRight(innerW);
                lines[2 + r] = GradientBorderV(
                    DecipherRowRaw(s, t, progress, r * innerW, totalW));
            }
            return lines;
        }

        private string DecipherRowRaw(string src, string tgt, float progress,
                                      int threshOffset, int totalBorderedWidth)
        {
            int len = tgt.Length;
            var sb = new StringBuilder(len * 32);
            for (int i = 0; i < len; i++)
            {
                float t = totalBorderedWidth > 1 ? (float)(i + 1) / (totalBorderedWidth - 1) : 0f;
                char srcCh = i < src.Length ? src[i] : ' ';
                char tgtCh = tgt[i];
                if (srcCh == tgtCh) { sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(t), tgtCh.ToString())); continue; }
                int idx = threshOffset + i;
                bool isSettled = _revealThresholds != null && idx < _revealThresholds.Length
                    && progress >= _revealThresholds[idx];
                char ch;
                if (isSettled) ch = tgtCh;
                else
                {
                    bool hasContent = srcCh != ' ' || tgtCh != ' ';
                    ch = hasContent ? GlitchGlyphs[UnityEngine.Random.Range(0, GlitchGlyphs.Length)] : ' ';
                }
                sb.Append(TUIColors.Fg(TUIGradient.CyanMagenta(t), ch.ToString()));
            }
            return sb.ToString();
        }
    }
}
