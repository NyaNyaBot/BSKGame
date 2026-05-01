using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.Editor
{
    public static class BattleHudPrefabBuilder
    {
        // Art Bible §4.4 UI 色板
        static readonly Color PanelDeep = HexColor("#222831");
        static readonly Color PanelSecondary = HexColor("#2E3540");
        static readonly Color TextPrimary = HexColor("#D8E4EC");
        static readonly Color TextSecondary = HexColor("#8FA3B0");
        static readonly Color FocusBlue = HexColor("#6BA3B8");
        static readonly Color DangerOrange = HexColor("#E85D2C");
        static readonly Color RewardGold = HexColor("#C9A227");

        const string PrefabFolder = "Assets/GameRes/UI/UIForms/BattleHud";
        const string PrefabPath = PrefabFolder + "/BattleHudForm.prefab";

        [MenuItem("BSKGame/Build BattleHud Prefab", false, 200)]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/GameRes/UI/UIForms", "BattleHud");
            }

            var root = new GameObject("BattleHudForm", typeof(RectTransform));
            var rootRT = root.GetComponent<RectTransform>();
            StretchFull(rootRT);

            // UGuiForm 依赖的组件 (OnInit 会 GetOrAdd，预制体中预置更稳)
            root.AddComponent<Canvas>();
            root.AddComponent<CanvasGroup>();
            root.AddComponent<GraphicRaycaster>();

            // BattleHudForm 脚本
            var form = root.AddComponent<BattleHudForm>();

            // ── Player Status Panel (左上) ──
            var playerPanel = CreatePanel("PlayerStatusPanel", root.transform, TextAnchor.UpperLeft,
                new Vector2(0, 1), new Vector2(0.45f, 1), new Vector2(10, -10), new Vector2(-5, -10));

            CreatePanelBg(playerPanel, PanelDeep, 0.85f);

            var playerHpBar = CreateSlider("PlayerHpBar", playerPanel,
                new Vector2(0.15f, 0.4f), new Vector2(0.95f, 0.7f));
            form.PlayerHpBar = playerHpBar;

            var playerHpText = CreateText("PlayerHpText", playerPanel.transform, "100/100", 14,
                new Vector2(0.15f, 0.4f), new Vector2(0.95f, 0.7f));
            form.PlayerHpText = playerHpText;

            // ── Enemy Status Panel (右上) ──
            var enemyPanel = CreatePanel("EnemyStatusPanel", root.transform, TextAnchor.UpperRight,
                new Vector2(0.55f, 1), new Vector2(1, 1), new Vector2(5, -10), new Vector2(-10, -10));

            CreatePanelBg(enemyPanel, PanelDeep, 0.85f);

            var enemyHpBar = CreateSlider("EnemyHpBar", enemyPanel,
                new Vector2(0.05f, 0.4f), new Vector2(0.85f, 0.7f));
            form.EnemyHpBar = enemyHpBar;

            var enemyHpText = CreateText("EnemyHpText", enemyPanel.transform, "80/80", 14,
                new Vector2(0.05f, 0.4f), new Vector2(0.85f, 0.7f));
            form.EnemyHpText = enemyHpText;

            // ── Phase Text (左下) ──
            var phaseText = CreateText("PhaseText", root.transform, "指令阶段", 14,
                new Vector2(0, 0), new Vector2(0.3f, 0.08f));
            phaseText.alignment = TextAnchor.MiddleLeft;
            var phaseRT = phaseText.GetComponent<RectTransform>();
            phaseRT.offsetMin = new Vector2(16, 8);
            phaseRT.offsetMax = new Vector2(0, 0);
            form.PhaseText = phaseText;

            // ── Parry Ready Indicator (右下) ──
            var parryGO = CreateCanvasGroupPanel("ParryReadyIndicator", root.transform,
                new Vector2(0.65f, 0), new Vector2(1, 0.15f));
            var parryBg = CreatePanelBg(parryGO, PanelSecondary, 0.7f);
            var parryLabel = CreateText("ParryLabel", parryGO.transform, "弹反", 18,
                new Vector2(0, 0), new Vector2(1, 1));
            parryLabel.alignment = TextAnchor.MiddleCenter;
            form.ParryReadyIndicator = parryGO.GetComponent<CanvasGroup>();
            form.ParryReadyIndicator.alpha = 0f;

            // ── Counter Ready Indicator (右下偏上) ──
            var counterGO = CreateCanvasGroupPanel("CounterReadyIndicator", root.transform,
                new Vector2(0.65f, 0.15f), new Vector2(1, 0.25f));
            var counterBg = CreatePanelBg(counterGO, PanelSecondary, 0.7f);
            SetImageColor(counterBg, RewardGold, 0.3f);
            var counterLabel = CreateText("CounterLabel", counterGO.transform, "反击", 16,
                new Vector2(0, 0), new Vector2(1, 1));
            counterLabel.alignment = TextAnchor.MiddleCenter;
            counterLabel.color = RewardGold;
            form.CounterReadyIndicator = counterGO.GetComponent<CanvasGroup>();
            form.CounterReadyIndicator.alpha = 0f;

            // ── Result Panel (全屏覆盖) ──
            var resultGO = CreateCanvasGroupPanel("ResultPanel", root.transform,
                new Vector2(0, 0), new Vector2(1, 1));
            var resultBg = CreatePanelBg(resultGO, Color.black, 0.75f);
            var resultText = CreateText("ResultText", resultGO.transform, "胜利", 28,
                new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.65f));
            resultText.alignment = TextAnchor.MiddleCenter;
            form.ResultPanel = resultGO.GetComponent<CanvasGroup>();
            form.ResultPanel.alpha = 0f;
            form.ResultPanel.blocksRaycasts = false;
            form.ResultText = resultText;

            // ── Debug Overlay (右侧纵向) ──
            var debugGO = CreateCanvasGroupPanel("DebugOverlay", root.transform,
                new Vector2(0.6f, 0.7f), new Vector2(1, 1));
            var debugBg = CreatePanelBg(debugGO, Color.black, 0.5f);

            var debugClockText = CreateText("DebugClockText", debugGO.transform, "Clock: 0ms", 11,
                new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.95f));
            debugClockText.color = Color.green;
            form.DebugClockText = debugClockText;

            var debugPhaseText = CreateText("DebugPhaseText", debugGO.transform, "Phase: None", 11,
                new Vector2(0.05f, 0.33f), new Vector2(0.95f, 0.62f));
            debugPhaseText.color = Color.green;
            form.DebugPhaseText = debugPhaseText;

            var debugSegText = CreateText("DebugSegmentIdText", debugGO.transform, "Seg: ", 11,
                new Vector2(0.05f, 0.0f), new Vector2(0.95f, 0.29f));
            debugSegText.color = Color.green;
            form.DebugSegmentIdText = debugSegText;

            form.DebugOverlay = debugGO.GetComponent<CanvasGroup>();
            form.DebugOverlay.alpha = 0f;

            // ── 保存预制体 ──
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.Refresh();

            Debug.Log($"[BattleHudPrefabBuilder] Prefab saved → {PrefabPath}");
            EditorUtility.DisplayDialog("BattleHud Prefab", $"预制体已生成：\n{PrefabPath}", "OK");
        }

        #region Helpers

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static GameObject CreatePanel(string name, Transform parent, TextAnchor pivot,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            float h = 80f;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            return go;
        }

        static GameObject CreateCanvasGroupPanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        static Image CreatePanelBg(GameObject parent, Color color, float alpha)
        {
            var bgGO = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(parent.transform, false);
            StretchFull(bgGO.GetComponent<RectTransform>());
            var img = bgGO.GetComponent<Image>();
            color.a = alpha;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static void SetImageColor(Image img, Color color, float alpha)
        {
            color.a = alpha;
            img.color = color;
        }

        static Slider CreateSlider(string name, GameObject parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var slider = go.GetComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            // Background
            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(go.transform, false);
            StretchFull(bgGO.GetComponent<RectTransform>());
            bgGO.GetComponent<Image>().color = PanelSecondary;

            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            StretchFull(fillArea.GetComponent<RectTransform>());

            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(fillArea.transform, false);
            StretchFull(fillGO.GetComponent<RectTransform>());
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.color = DangerOrange;

            slider.fillRect = fillGO.GetComponent<RectTransform>();
            slider.value = 1f;
            slider.maxValue = 1f;

            return slider;
        }

        static Text CreateText(string name, Transform parent, string defaultText, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var text = go.GetComponent<Text>();
            text.text = defaultText;
            text.fontSize = fontSize;
            text.color = TextPrimary;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        #endregion
    }
}
