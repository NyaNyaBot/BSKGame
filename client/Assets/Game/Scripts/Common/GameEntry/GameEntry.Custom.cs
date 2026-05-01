using Game.Gameplay.SceneLifecycle;
using GameFramework;
using UnityEngine;

namespace Game.Client
{
    public partial class GameEntry
    {
        /// <summary>
        /// 场景与战斗上下文（ADR-0001），单一实例；由场景管理在加载/卸载时驱动。
        /// </summary>
        public static ISceneContextService SceneContextService { get; private set; } = null!;

        // public static BuiltinDataComponent BuiltinData
        // {
        //     get;
        //     private set;
        // }
        private static void InitCustomComponents()
        {
            SceneContextService = new SceneContextService();
            //BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
        }
        
        public static void QuitApplication()
        {
#if UNITY_EDITOR

            UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");

#endif
            Application.Quit();
        }
    }
}