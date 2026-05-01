using System;
using Game.Core;
using Game.Gameplay;
using Game.Gameplay.SceneLifecycle;
using GameFramework.DataTable;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.SceneManagement;
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
                int buildIndex = ResolveBattleSceneBuildIndex(m_SelectBattle.BattleScenePath);
                IDataTable<DRScene> dtScene = GameEntry.DataTable.GetDataTable<DRScene>();
                DRScene drScene = dtScene != null ? dtScene.GetDataRow(buildIndex) : null;
                if (buildIndex < 0 || drScene == null)
                {
                    Log.Warning(
                        "Cannot start battle: scene path '{0}' resolved to build index {1}, but DRScene row is missing. Use paths like 'Assets/GameRes/Scenes/Main.unity' in Battle table and ensure scenes are listed in Build Settings.",
                        m_SelectBattle.BattleScenePath,
                        buildIndex);
                    m_StartBattle = false;
                }
                else
                {
                    LeaveMenu();
                    ChangeScene(buildIndex);
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
        /// 将战役表中的场景路径转为 Editor Build Settings 中的路径，再解析 build index。
        /// 历史表数据常写成 GameRes/... 且无 .unity 后缀，会导致 GetBuildIndexByScenePath 恒为 -1。
        /// </summary>
        private static int ResolveBattleSceneBuildIndex(string battleScenePath)
        {
            if (string.IsNullOrWhiteSpace(battleScenePath))
            {
                return -1;
            }

            var p = battleScenePath.Trim().Replace('\\', '/');
            if (!p.StartsWith("Assets/", StringComparison.Ordinal))
            {
                p = "Assets/" + p.TrimStart('/');
            }

            if (!p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                p += ".unity";
            }

            return SceneUtility.GetBuildIndexByScenePath(p);
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

            m_IsChangeSceneComplete = true;
            m_IsLoadingScene = false;
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