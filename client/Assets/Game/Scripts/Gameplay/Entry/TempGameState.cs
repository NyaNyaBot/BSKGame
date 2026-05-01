using System;
using Game.Core;
using Game.Gameplay;
using Game.Gameplay.Character;
using Game.Gameplay.Integration;
using Game.Gameplay.SceneLifecycle;
using GameFramework.DataTable;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    public class TempGameState
    {
        private bool m_StartBattle = false;
        private UGuiForm m_MenuForm = null;
        private DRBattle m_SelectBattle = null;
        private bool m_IsChangeSceneComplete = false;
        private bool m_IsLoadingScene = false;
        public TempGameState()
        {
            GameEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GameEntry.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            GameEntry.Event.Subscribe(LoadSceneDependencyAssetEventArgs.EventId, OnLoadSceneDependencyAsset);
        }

        public void EnterMenu()
        {
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            m_SelectBattle = null;
            m_StartBattle = false;
            GameEntry.SceneContextService.EnterScene(SceneKind.Menu);
            GameEntry.UI.OpenUIForm(UIFormId.MenuForm, this);
        }

        public void LeaveMenu()
        {
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            if (m_MenuForm != null)
            {
                m_MenuForm.Close();
                m_MenuForm = null;
            }
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_MenuForm = (UGuiForm)ne.UIForm.Logic;
        }
        

        public void OnUpdate(float deltaTime)
        {
            if (m_StartBattle && !m_IsLoadingScene)
            {
                int sceneId = ResolveBattleSceneId(m_SelectBattle.BattleScenePath);
                if (sceneId < 0)
                {
                    Log.Warning(
                        "Cannot start battle: scene path '{0}' has no matching DRScene row.",
                        m_SelectBattle.BattleScenePath);
                    m_StartBattle = false;
                }
                else
                {
                    LeaveMenu();
                    ChangeScene(sceneId);
                    m_StartBattle = false;
                }
            }
            
            if (m_IsChangeSceneComplete)
            {
                Debug.Log("Load scene complete, enter main");
            }
        }

        public void ChangeScene(int sceneId)
        {
            m_IsChangeSceneComplete = false;

            IDataTable<DRScene> dtScene = GameEntry.DataTable.GetDataTable<DRScene>();
            DRScene drScene = dtScene != null ? dtScene.GetDataRow(sceneId) : null;

            if (sceneId < 0 || drScene == null)
            {
                Log.Warning("Can not load scene '{0}' from data table.", sceneId.ToString());
                return;
            }

            m_IsLoadingScene = true;
            var routing = GameEntry.SceneContextService;
            if (!string.IsNullOrEmpty(routing.CurrentScene.SceneContextId))
            {
                routing.BeginSceneTransition(SceneTransitionReason.UnloadStarting);
            }

            // 卸载所有场景
            string[] loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
            for (int i = 0; i < loadedSceneAssetNames.Length; i++)
            {
                GameEntry.Scene.UnloadScene(loadedSceneAssetNames[i]);
            }

            // 还原游戏速度
            GameEntry.Base.ResetNormalGameSpeed();

            GameEntry.Scene.LoadScene(AssetUtility.GetSceneAsset(drScene.AssetName), Constant.AssetPriority.SceneAsset, this);
        }

        /// <summary>
        /// 从 DRBattle.BattleScenePath 匹配 DRScene 表中的行。
        /// 例: "Assets/GameRes/Scenes/Main.unity" → 匹配 DRScene.AssetName="Main" → 返回 DRScene.Id。
        /// </summary>
        private static int ResolveBattleSceneId(string battleScenePath)
        {
            if (string.IsNullOrWhiteSpace(battleScenePath))
                return -1;

            IDataTable<DRScene> dtScene = GameEntry.DataTable.GetDataTable<DRScene>();
            if (dtScene == null)
                return -1;

            var p = battleScenePath.Trim().Replace('\\', '/');
            if (p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                p = p.Substring(0, p.Length - 6);

            int lastSlash = p.LastIndexOf('/');
            string sceneDirAndName = lastSlash >= 0 ? p.Substring(lastSlash + 1) : p;

            int scenesPrefix = p.IndexOf("/Scenes/", StringComparison.OrdinalIgnoreCase);
            string relativeFromScenes = scenesPrefix >= 0 ? p.Substring(scenesPrefix + 8) : sceneDirAndName;

            foreach (var row in dtScene.GetAllDataRows())
            {
                if (string.Equals(row.AssetName, relativeFromScenes, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(row.AssetName, sceneDirAndName, StringComparison.OrdinalIgnoreCase))
                {
                    return row.Id;
                }
            }

            return -1;
        }
        
        
        public void StartBattle(DRBattle battle)
        {
            m_StartBattle = true;
            m_SelectBattle = battle;
        }
        
        
        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' OK.", ne.SceneAssetName);

            var routing = GameEntry.SceneContextService;
            routing.CompleteSceneTransition();
            routing.EnterScene(SceneKind.Battle);
            routing.CreateBattleContext(default);

            SpawnBattleFlowController(routing);

            m_IsChangeSceneComplete = true;
            m_IsLoadingScene = false;
        }

        private void SpawnBattleFlowController(ISceneContextService sceneContext)
        {
            var go = new GameObject("[BattleFlowController]");
            var controller = go.AddComponent<BattleFlowController>();

            var config = new BattleStartConfig();
            if (sceneContext.TryGetActiveBattle(out var bc))
                config.BattleContextId = bc.BattleContextId;

            var playerDef = BuildCharacterDef(m_SelectBattle.Role1, "Player");
            var enemyDef = BuildCharacterDef(m_SelectBattle.Role2, "Enemy");

            controller.Initialize(sceneContext, config, playerDef, enemyDef);
        }

        /// <summary>
        /// 从数据表构建角色定义。ATK/DEF 目前未在 DRProperty 中定义，使用占位默认值。
        /// </summary>
        private CharacterDefinition BuildCharacterDef(int roleId, string fallbackName)
        {
            IDataTable<DRRole> dtRole = GameEntry.DataTable.GetDataTable<DRRole>();
            DRRole role = dtRole?.GetDataRow(roleId);
            string name = role?.Name ?? fallbackName;
            int maxHp = 100;
            int maxMp = 0;
            int atk = 20;
            int def = 5;

            if (role != null)
            {
                IDataTable<DRProperty> dtProp = GameEntry.DataTable.GetDataTable<DRProperty>();
                DRProperty prop = dtProp?.GetDataRow(role.PropertyId);
                if (prop != null)
                {
                    maxHp = System.Math.Max(1, (int)prop.MaxHealth);
                    maxMp = (int)prop.MaxMana;
                }
            }

            return new CharacterDefinition($"role_{roleId}", name, maxHp, maxMp, atk, def);
        }

        private void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Load scene '{0}' failure, error message '{1}'.", ne.SceneAssetName, ne.ErrorMessage);
            GameEntry.SceneContextService.CompleteSceneTransition();
            m_IsLoadingScene = false;
        }

        private void OnLoadSceneUpdate(object sender, GameEventArgs e)
        {
            LoadSceneUpdateEventArgs ne = (LoadSceneUpdateEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' update, progress '{1}'.", ne.SceneAssetName, ne.Progress.ToString("P2"));
        }

        private void OnLoadSceneDependencyAsset(object sender, GameEventArgs e)
        {
            LoadSceneDependencyAssetEventArgs ne = (LoadSceneDependencyAssetEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' dependency asset '{1}', count '{2}/{3}'.", ne.SceneAssetName, ne.DependencyAssetName, ne.LoadedCount.ToString(), ne.TotalCount.ToString());
        }
    }
}