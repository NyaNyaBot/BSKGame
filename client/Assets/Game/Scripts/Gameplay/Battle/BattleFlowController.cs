using System;
using Game.Gameplay.Character;
using Game.Gameplay.Integration;
using Game.Gameplay.SceneLifecycle;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// Unity 侧战斗流程控制器。
    /// 挂在战斗场景根节点上，负责：
    ///   1) 创建 BattleOrchestrator 并初始化角色
    ///   2) 每帧驱动 Tick
    ///   3) 打开 BattleHudForm
    ///   4) 场景卸载时 Dispose
    /// </summary>
    public class BattleFlowController : MonoBehaviour
    {
        private BattleOrchestrator _orchestrator;
        private bool _hudOpened;

        /// <summary>
        /// 外部可读取 orchestrator（如 HUD 需要 ReadModel）。
        /// </summary>
        public BattleOrchestrator Orchestrator => _orchestrator;

        public void Initialize(ISceneContextService sceneContext, BattleStartConfig config,
            CharacterDefinition playerDef, CharacterDefinition enemyDef)
        {
            if (_orchestrator != null)
            {
                Log.Warning("BattleFlowController.Initialize called twice.");
                return;
            }

            _orchestrator = new BattleOrchestrator(sceneContext, config);
            _orchestrator.Start(playerDef, enemyDef);

            OpenBattleHud();
        }

        private void Update()
        {
            if (_orchestrator == null || !_orchestrator.IsRunning) return;

            long deltaMs = (long)(Time.deltaTime * 1000f);
            _orchestrator.Tick(deltaMs);
        }

        private void OnDestroy()
        {
            if (_orchestrator != null)
            {
                _orchestrator.Dispose();
                _orchestrator = null;
            }
        }

        private void OpenBattleHud()
        {
            if (_hudOpened) return;
            _hudOpened = true;

            var openData = new BattleHudOpenData
            {
                Bus = _orchestrator.Bus,
                ReadModel = _orchestrator.ReadModel,
                Clock = _orchestrator.Clock,
            };
            GameEntry.UI.OpenUIForm(UIFormId.BattleHudForm, openData);
        }
    }
}
