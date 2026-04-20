// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can real-time parry feel precise and satisfying on WebGL touch screens?
// Date: 2026-04-12

using UnityEngine;
using System.Linq;

namespace Prototype.ParryCombat
{
    public class ParryDebugPanel : MonoBehaviour
    {
        ParryPrototypeController _controller;
        ParryFeedback _feedback;
        bool _showPanel = true;
        Vector2 _scrollPos;

        float _fps;
        float _fpsTimer;
        int _fpsFrames;

        readonly GUIStyle _headerStyle = new GUIStyle();
        readonly GUIStyle _labelStyle = new GUIStyle();
        readonly GUIStyle _resultStyle = new GUIStyle();
        bool _stylesInitialized;

        void Awake()
        {
            _controller = GetComponent<ParryPrototypeController>();
            _feedback = GetComponent<ParryFeedback>();
        }

        void Update()
        {
            _fpsFrames++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer >= 0.5f)
            {
                _fps = _fpsFrames / _fpsTimer;
                _fpsFrames = 0;
                _fpsTimer = 0;
            }
        }

        void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _headerStyle.fontSize = 16;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = Color.white;

            _labelStyle.fontSize = 13;
            _labelStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            _resultStyle.fontSize = 20;
            _resultStyle.fontStyle = FontStyle.Bold;
            _resultStyle.alignment = TextAnchor.MiddleCenter;
        }

        void OnGUI()
        {
            InitStyles();

            DrawPhaseIndicator();
            DrawResultFlash();

            if (_showPanel)
                DrawDebugPanel();

            if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 35), _showPanel ? "Hide Debug" : "Show Debug"))
                _showPanel = !_showPanel;
        }

        void DrawPhaseIndicator()
        {
            if (_controller == null) return;

            float barWidth = Screen.width * 0.6f;
            float barHeight = 30;
            float barX = (Screen.width - barWidth) * 0.5f;
            float barY = Screen.height - 80;

            GUI.Box(new Rect(barX - 5, barY - 5, barWidth + 10, barHeight + 10), "");

            Color phaseColor = _controller.CurrentPhase switch
            {
                ParryPhase.Idle => new Color(0.3f, 0.3f, 0.3f),
                ParryPhase.WindUp => new Color(0.9f, 0.3f, 0.1f),
                ParryPhase.ParryWindow => new Color(1f, 0.85f, 0.1f),
                ParryPhase.Strike => new Color(1f, 0.1f, 0.1f),
                ParryPhase.Cooldown => new Color(0.2f, 0.4f, 0.2f),
                _ => Color.gray
            };

            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, phaseColor);
            tex.Apply();

            float fillWidth = barWidth * _controller.PhaseProgress;
            GUI.DrawTexture(new Rect(barX, barY, fillWidth, barHeight), tex);
            Destroy(tex);

            var centerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            centerStyle.normal.textColor = Color.white;

            string phaseText = _controller.CurrentPhase switch
            {
                ParryPhase.Idle => "IDLE — 准备...",
                ParryPhase.WindUp => "⚠ WIND UP — 注意前摇!",
                ParryPhase.ParryWindow => "⚡ PARRY NOW! 点击弹反!",
                ParryPhase.Strike => "STRIKE!",
                ParryPhase.Cooldown => "COOLDOWN",
                _ => ""
            };
            GUI.Label(new Rect(barX, barY, barWidth, barHeight), phaseText, centerStyle);
        }

        void DrawResultFlash()
        {
            if (_controller == null || _controller.LastResult == ParryResult.None) return;

            _resultStyle.normal.textColor = _controller.LastResult switch
            {
                ParryResult.Perfect => new Color(1f, 0.85f, 0.1f),
                ParryResult.Good => Color.white,
                ParryResult.Miss => new Color(1f, 0.3f, 0.3f),
                _ => Color.white
            };

            string text = _controller.LastResult switch
            {
                ParryResult.Perfect => "PERFECT!",
                ParryResult.Good => "GOOD",
                ParryResult.Miss => "MISS",
                _ => ""
            };

            float centerX = Screen.width * 0.5f - 100;
            float centerY = Screen.height * 0.3f;
            GUI.Label(new Rect(centerX, centerY, 200, 40), text, _resultStyle);

            string offsetText = $"Offset: {_controller.LastParryOffset:+0.0;-0.0}ms";
            var offsetStyle = new GUIStyle(_labelStyle) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(centerX, centerY + 35, 200, 25), offsetText, offsetStyle);
        }

        void DrawDebugPanel()
        {
            if (_controller == null) return;

            float panelWidth = 320;
            float panelHeight = 520;
            var panelRect = new Rect(10, 10, panelWidth, panelHeight);

            GUI.Box(panelRect, "");
            GUI.Box(panelRect, "");

            GUILayout.BeginArea(new Rect(panelRect.x + 10, panelRect.y + 10,
                panelRect.width - 20, panelRect.height - 20));

            GUILayout.Label("弹反原型 Debug Panel", _headerStyle);
            GUILayout.Space(5);
            GUILayout.Label($"FPS: {_fps:F0}", _labelStyle);
            GUILayout.Label($"TimeScale: {Time.timeScale:F2}", _labelStyle);

            GUILayout.Space(10);
            GUILayout.Label("— 统计 —", _headerStyle);
            GUILayout.Label($"总攻击: {_controller.TotalAttacks}", _labelStyle);
            GUILayout.Label($"Perfect: {_controller.PerfectCount}  |  Good: {_controller.GoodCount}  |  Miss: {_controller.MissCount}", _labelStyle);

            if (_controller.TotalAttacks > 0)
            {
                float perfectRate = (float)_controller.PerfectCount / _controller.TotalAttacks * 100;
                GUILayout.Label($"Perfect Rate: {perfectRate:F1}%", _labelStyle);
            }

            GUILayout.Space(5);
            GUILayout.Label($"上次帧延迟: {_controller.LastInputLatency:F1}ms", _labelStyle);
            GUILayout.Label($"上次弹反偏移: {_controller.LastParryOffset:+0.0;-0.0}ms", _labelStyle);

            if (_controller.InputLatencies.Count > 0)
            {
                float avg = _controller.InputLatencies.Average();
                float max = _controller.InputLatencies.Max();
                float min = _controller.InputLatencies.Min();
                GUILayout.Label($"延迟 avg/min/max: {avg:F1} / {min:F1} / {max:F1} ms", _labelStyle);
            }

            GUILayout.Space(10);
            GUILayout.Label("— 参数调整 —", _headerStyle);

            GUILayout.Label($"弹反窗口: {_controller.parryWindowDuration * 1000:F0}ms", _labelStyle);
            _controller.parryWindowDuration = GUILayout.HorizontalSlider(
                _controller.parryWindowDuration, 0.1f, 0.8f);

            GUILayout.Label($"完美窗口: {_controller.perfectWindowDuration * 1000:F0}ms", _labelStyle);
            _controller.perfectWindowDuration = GUILayout.HorizontalSlider(
                _controller.perfectWindowDuration, 0.03f, 0.3f);

            GUILayout.Label($"前摇时长: {_controller.windUpDuration * 1000:F0}ms", _labelStyle);
            _controller.windUpDuration = GUILayout.HorizontalSlider(
                _controller.windUpDuration, 0.3f, 2.0f);

            GUILayout.Label($"空闲间隔: {_controller.idleDuration * 1000:F0}ms", _labelStyle);
            _controller.idleDuration = GUILayout.HorizontalSlider(
                _controller.idleDuration, 0.5f, 3.0f);

            if (_feedback != null)
            {
                GUILayout.Space(5);
                GUILayout.Label($"顿帧(Perfect): {_feedback.perfectHitStopDuration * 1000:F0}ms", _labelStyle);
                _feedback.perfectHitStopDuration = GUILayout.HorizontalSlider(
                    _feedback.perfectHitStopDuration, 0f, 0.3f);

                GUILayout.Label($"震屏强度(Perfect): {_feedback.perfectShakeIntensity:F2}", _labelStyle);
                _feedback.perfectShakeIntensity = GUILayout.HorizontalSlider(
                    _feedback.perfectShakeIntensity, 0f, 0.5f);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("重置统计"))
                _controller.ResetStats();

            GUILayout.EndArea();
        }
    }
}
