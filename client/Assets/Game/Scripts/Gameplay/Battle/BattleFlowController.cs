using Game.Gameplay.BattleAction;
using Game.Gameplay.Character;
using Game.Gameplay.Integration;
using Game.Gameplay.SceneLifecycle;
using Game.Gameplay.TurnManager;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// Unity 侧战斗流程控制器。
    /// 职责：创建 Orchestrator、驱动 Tick、处理玩家输入、自动执行敌人回合、管理战斗结束。
    /// </summary>
    public class BattleFlowController : MonoBehaviour
    {
        private BattleOrchestrator _orchestrator;
        private bool _hudOpened;
        private float _enemyActionDelay;
        private bool _waitingForEnemyResolve;

        private const float EnemyTurnDelaySeconds = 0.6f;

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
            Log.Info("[Battle] Started. Player={0} Enemy={1}", _orchestrator.PlayerInstanceId, _orchestrator.EnemyInstanceId);
        }

        private void Update()
        {
            if (_orchestrator == null || !_orchestrator.IsRunning) return;

            long deltaMs = (long)(Time.deltaTime * 1000f);
            _orchestrator.Tick(deltaMs);

            if (_waitingForEnemyResolve)
            {
                _enemyActionDelay -= Time.deltaTime;
                if (_enemyActionDelay <= 0f)
                {
                    _waitingForEnemyResolve = false;
                    _orchestrator.AdvanceToResolution();

                    if (!_orchestrator.IsRunning)
                        OnBattleEnd();
                }
                return;
            }

            if (_orchestrator.PhaseMachine.CurrentPhase == BattleInputPhase.PlayerCommand)
                HandlePlayerInput();
        }

        private void HandlePlayerInput()
        {
            bool inputDetected = false;

#if UNITY_EDITOR || (!UNITY_IOS && !UNITY_ANDROID)
            inputDetected = UnityEngine.Input.GetMouseButtonDown(0);
#else
            inputDetected = UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began;
#endif

            if (!inputDetected) return;

            var result = _orchestrator.SubmitPlayerAction(
                _orchestrator.PlayerInstanceId,
                _orchestrator.EnemyInstanceId);

            if (result.Outcome == BattleActionOutcome.Accepted)
            {
                Log.Info("[Battle] Player attacked. Phase → EnemyAction");

                if (!_orchestrator.IsRunning)
                {
                    OnBattleEnd();
                    return;
                }

                _waitingForEnemyResolve = true;
                _enemyActionDelay = EnemyTurnDelaySeconds;
            }
        }

        private void OnBattleEnd()
        {
            var player = _orchestrator.CharacterRepo.GetInstance(_orchestrator.PlayerInstanceId);
            bool playerWon = player != null && player.Status == CharacterStatus.Active;
            Log.Info("[Battle] Ended. Result={0}, Turns={1}", playerWon ? "Victory" : "Defeat", _orchestrator.TurnNumber);
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
