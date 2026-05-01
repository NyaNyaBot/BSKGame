#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Client.Editor
{
    /// <summary>
    /// Pre-production：从 Launch 跑通热更与 Procedure 后，可一键在进菜单后自动加载 Main 场景做流程验证，避免每次手动点战役进 BattleStorm 老场景。
    /// </summary>
    public static class PreProductionFlowValidateMenu
    {
        public const string EditorPrefsAutoLoadMainScene = "BSK.PreProduction.AutoLoadMainSceneAfterMenu";

        private const string MenuPath = "Echo of Blades/Pre-Production/Auto-Load Main Scene After Menu";

        [MenuItem(MenuPath, false, 1)]
        private static void ToggleAutoLoadMain()
        {
            bool next = !EditorPrefs.GetBool(EditorPrefsAutoLoadMainScene, false);
            EditorPrefs.SetBool(EditorPrefsAutoLoadMainScene, next);
            Debug.Log($"[BSK] {MenuPath}: {(next ? "ON" : "OFF")} (Editor Play only; uses DRBattle path containing \"Main\".)");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleAutoLoadMainValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(EditorPrefsAutoLoadMainScene, false));
            return true;
        }
    }
}
#endif
