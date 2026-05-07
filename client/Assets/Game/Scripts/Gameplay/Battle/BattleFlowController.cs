using System;
using Game.Gameplay.BattleAction;
using Game.Gameplay.Character;
using Game.Gameplay.Damage;
using Game.Gameplay.Input;
using Game.Gameplay.Integration;
using Game.Gameplay.Parry;
using Game.Gameplay.SceneLifecycle;
using Game.Gameplay.TurnManager;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// Unity 侧战斗流程控制器（动画驱动状态机）。
    /// 回合循环：PlayerInput → PlayerAttack → PostPlayerAttack → EnemyWindUp →
    ///   EnemyParryWindow → (ParrySuccess | EnemyStrike) → PostEnemyTurn → PlayerInput
    /// 每个状态有明确的进入/退出条件，输入仅在指定状态被接受。
    /// </summary>
    public class BattleFlowController : MonoBehaviour
    {
        private enum BattleState
        {
            None,
            PlayerInput,        // 等待玩家输入（仅此阶段接受攻击输入）
            PlayerAttack,       // 玩家攻击动画播放中
            PostPlayerAttack,   // 玩家攻击后短暂喘息
            EnemyWindUp,        // 敌人前摇（点击=过早=失败弹反）
            EnemyParryWindow,   // 弹反窗口（中心=Perfect，其余=Normal）
            ParrySuccess,       // 弹反成功（Normal/Perfect），敌人受击表现
            PlayerCounter,      // 完美弹反 → 玩家原地反击
            CounterHitReaction, // 反击命中后等待敌人受击动画
            EnemyStrike,        // 失败弹反，敌人命中，玩家受击表现
            PostEnemyTurn,      // 敌人回合后短暂喘息
            BattleEnd
        }

        private BattleOrchestrator _orchestrator;
        private BattleHudTouchAreaManager _touchAreaManager;
        private BattleSceneSetup _sceneSetup;
        private bool _hudOpened;
        private IDisposable _defeatedSubscription;
        private bool _battleEnded;

        private BattleState _state = BattleState.None;
        private float _stateTimer;
        private float _stateDuration;
        private bool _hitAppliedThisState;
        private bool _parryAttempted;
        private ParryGrade _lastParryGrade;
        private float _inputCooldown;

        private const float PostPlayerAttackDuration = 0.4f;
        private const float EnemyWindUpDuration = 0.8f;
        private const float ParryWindowDuration = 0.6f;
        private const float PerfectWindowDuration = 0.3f;
        private const float ParrySuccessDuration = 1.2f;
        private const float PlayerCounterDuration = 0.8f;
        private const float CounterHitReactionDuration = 0.6f;
        private const float EnemyStrikeDuration = 1.2f;
        private const float PostEnemyTurnDuration = 0.7f;
        private const float InputCooldownSeconds = 0.25f;

        public BattleOrchestrator Orchestrator => _orchestrator;

        /// <summary>
        /// 阶段变化事件: (phaseId, phaseDuration, parryWindowDuration, perfectWindowDuration)
        /// phaseId: 0=Hide, 1=WindUp, 2=ParryWindow, 3=Strike
        /// </summary>
        public static event System.Action<int, float, float, float> OnEnemyAttackPhaseChanged;

        public void Initialize(ISceneContextService sceneContext, BattleStartConfig config,
            CharacterDefinition playerDef, CharacterDefinition enemyDef)
        {
            if (_orchestrator != null)
            {
                Log.Warning("BattleFlowController.Initialize called twice.");
                return;
            }

            try
            {
                _orchestrator = new BattleOrchestrator(sceneContext, config);
                _orchestrator.Start(playerDef, enemyDef);

                _touchAreaManager = new BattleHudTouchAreaManager(_orchestrator.HitAreaRegistry, _orchestrator.Bus);

                // 任一角色阵亡 → 立即终止状态机，避免后续阶段继续推进。
                _defeatedSubscription = _orchestrator.Bus.Subscribe<CharacterDefeated>(OnCharacterDefeated);

                InitializePresentation();
                OpenBattleHud();

                Log.Info("[Battle] Started. Player={0} Enemy={1}", _orchestrator.PlayerInstanceId, _orchestrator.EnemyInstanceId);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[BattleFlowController] Initialize failed: {ex}");
            }
        }

        private void InitializePresentation()
        {
            _sceneSetup = UnityEngine.Object.FindObjectOfType<BattleSceneSetup>();

            if (_sceneSetup != null)
                _sceneSetup.InitializePresentation(_orchestrator.Bus, _orchestrator.PlayerInstanceId, _orchestrator.EnemyInstanceId);
            else
                Log.Warning("[BattleFlowController] No BattleSceneSetup found in scene.");
        }

        private void Update()
        {
            if (_orchestrator == null) return;

            // 阵亡通知或 orchestrator 已停止 → 立即落幕，不再推进任何战斗阶段。
            if (_battleEnded || !_orchestrator.IsRunning)
            {
                if (_state != BattleState.BattleEnd)
                {
                    NotifyHudPhase(0, 0f); // 关闭弹反 UI
                    OnBattleEnd();
                    _state = BattleState.BattleEnd;
                }
                return;
            }

            if (_state == BattleState.None)
                TransitionTo(BattleState.PlayerInput);

            if (_inputCooldown > 0f)
                _inputCooldown -= Time.deltaTime;

            _stateTimer += Time.deltaTime;

            long deltaMs = (long)(Time.deltaTime * 1000f);
            _orchestrator.Tick(deltaMs);

            switch (_state)
            {
                case BattleState.PlayerInput:        TickPlayerInput();        break;
                case BattleState.PlayerAttack:       TickPlayerAttack();       break;
                case BattleState.PostPlayerAttack:   TickPostPlayerAttack();   break;
                case BattleState.EnemyWindUp:        TickEnemyWindUp();        break;
                case BattleState.EnemyParryWindow:   TickEnemyParryWindow();   break;
                case BattleState.ParrySuccess:       TickParrySuccess();       break;
                case BattleState.PlayerCounter:      TickPlayerCounter();      break;
                case BattleState.CounterHitReaction: TickCounterHitReaction(); break;
                case BattleState.EnemyStrike:        TickEnemyStrike();        break;
                case BattleState.PostEnemyTurn:      TickPostEnemyTurn();      break;
            }
        }

        private void TransitionTo(BattleState newState)
        {
            _state = newState;
            _stateTimer = 0f;
            _stateDuration = 0f;
            _hitAppliedThisState = false;
            OnEnterState(newState);
        }

        private void OnEnterState(BattleState state)
        {
            switch (state)
            {
                case BattleState.PlayerInput:
                    _parryAttempted = false;
                    NotifyHudPhase(0, 0f);
                    break;

                case BattleState.PlayerAttack:
                {
                    var p = _sceneSetup?.PlayerPresenter;
                    _stateDuration = p != null ? p.PlayAttack() : 1.0f;
                    if (_stateDuration <= 0.01f) _stateDuration = 1.0f;
                    Log.Info("[Battle] PlayerAttack started, duration={0:F2}s", _stateDuration);
                    break;
                }

                case BattleState.PostPlayerAttack:
                    _stateDuration = PostPlayerAttackDuration;
                    break;

                case BattleState.EnemyWindUp:
                {
                    _parryAttempted = false;
                    _stateDuration = EnemyWindUpDuration;
                    var ep = _sceneSetup?.EnemyPresenter;
                    if (ep != null) ep.PlayAttack();
                    NotifyHudPhase(1, EnemyWindUpDuration);
                    Log.Info("[Battle] EnemyWindUp started");
                    break;
                }

                case BattleState.EnemyParryWindow:
                    _stateDuration = ParryWindowDuration;
                    NotifyHudPhase(2, ParryWindowDuration);
                    break;

                case BattleState.ParrySuccess:
                {
                    _stateDuration = ParrySuccessDuration;
                    NotifyHudPhase(4, ParrySuccessDuration);
                    ResolveParrySuccess();
                    break;
                }

                case BattleState.PlayerCounter:
                {
                    var p = _sceneSetup?.PlayerPresenter;
                    float animLen = p != null ? p.PlayAttack() : 1.0f;
                    if (animLen <= 0.01f) animLen = 1.0f;
                    // 用动画长度做状态时长，确保 0.55 命中帧与视觉一致。
                    _stateDuration = Mathf.Max(animLen, PlayerCounterDuration);
                    NotifyHudPhase(0, 0f);
                    Log.Info("[Battle] PlayerCounter started, duration={0:F2}s", _stateDuration);
                    break;
                }

                case BattleState.CounterHitReaction:
                {
                    _stateDuration = CounterHitReactionDuration;
                    break;
                }

                case BattleState.EnemyStrike:
                {
                    _stateDuration = EnemyStrikeDuration;
                    NotifyHudPhase(3, EnemyStrikeDuration);
                    Log.Info("[Battle] EnemyStrike (failed parry)");
                    break;
                }

                case BattleState.PostEnemyTurn:
                {
                    _stateDuration = PostEnemyTurnDuration;
                    NotifyHudPhase(0, 0f);
                    var pl = _sceneSetup?.PlayerPresenter;
                    var en = _sceneSetup?.EnemyPresenter;
                    if (pl != null) pl.PlayIdle();
                    if (en != null) en.PlayIdle();
                    break;
                }
            }
        }

        // ───────────── State tick handlers ─────────────

        private void TickPlayerInput()
        {
            if (_inputCooldown > 0f) return;
            if (!IsAttackInputDetected()) return;

            _inputCooldown = InputCooldownSeconds;
            TransitionTo(BattleState.PlayerAttack);
        }

        private void TickPlayerAttack()
        {
            // 命中帧：攻击动画约 55% 处剑刃挥下，与屏幕震动同步。
            if (!_hitAppliedThisState && _stateTimer >= _stateDuration * 0.55f)
            {
                _hitAppliedThisState = true;
                ApplyPlayerDamage();
            }

            if (_stateTimer >= _stateDuration)
            {
                if (!_orchestrator.IsRunning) { OnBattleEnd(); _state = BattleState.BattleEnd; return; }
                TransitionTo(BattleState.PostPlayerAttack);
            }
        }

        private void TickPostPlayerAttack()
        {
            if (_stateTimer >= _stateDuration)
                TransitionTo(BattleState.EnemyWindUp);
        }

        private void TickEnemyWindUp()
        {
            bool inputThisFrame = IsAttackInputDetected();
            if (inputThisFrame)
                Log.Info("[Battle] WindUp input detected. cooldown={0:F3}, t={1:F3}",
                    _inputCooldown, _stateTimer);

            if (!_parryAttempted && _inputCooldown <= 0f && inputThisFrame)
            {
                _parryAttempted = true;
                _inputCooldown = InputCooldownSeconds;
                _lastParryGrade = ParryGrade.FailedParry;
                _orchestrator.PendingParryDamageModifierBp = 10000;
                Log.Info("[Battle] Parry too early! (during WindUp)");
                PublishParryResult();
                TransitionTo(BattleState.EnemyStrike);
                return;
            }

            if (_stateTimer >= _stateDuration)
                TransitionTo(BattleState.EnemyParryWindow);
        }

        private void TickEnemyParryWindow()
        {
            bool inputThisFrame = IsAttackInputDetected();
            if (inputThisFrame)
                Log.Info("[Battle] ParryWindow input detected. _parryAttempted={0}, _inputCooldown={1:F3}, _stateTimer={2:F3}",
                    _parryAttempted, _inputCooldown, _stateTimer);

            if (!_parryAttempted && _inputCooldown <= 0f && inputThisFrame)
            {
                _parryAttempted = true;
                _inputCooldown = InputCooldownSeconds;

                float windowCenter = ParryWindowDuration * 0.5f;
                float offsetFromCenter = Mathf.Abs(_stateTimer - windowCenter);

                if (offsetFromCenter <= PerfectWindowDuration * 0.5f)
                {
                    _lastParryGrade = ParryGrade.PerfectParry;
                    _orchestrator.PendingParryDamageModifierBp = 0; // 完美弹反 → 0 伤害
                }
                else
                {
                    _lastParryGrade = ParryGrade.NormalParry;
                    _orchestrator.PendingParryDamageModifierBp = 5000; // 普通弹反 → 50% 伤害
                }

                var p = _sceneSetup?.PlayerPresenter;
                if (p != null) p.PlayBlock();

                Log.Info("[Battle] Parry success! Grade={0}, offset={1:F3}s", _lastParryGrade, _stateTimer - windowCenter);
                PublishParryResult();
                TransitionTo(BattleState.ParrySuccess);
                return;
            }

            if (_stateTimer >= _stateDuration)
            {
                _lastParryGrade = ParryGrade.FailedParry;
                _orchestrator.PendingParryDamageModifierBp = 10000;
                PublishParryResult();
                TransitionTo(BattleState.EnemyStrike);
            }
        }

        private void TickParrySuccess()
        {
            if (_stateTimer >= _stateDuration)
            {
                var p = _sceneSetup?.PlayerPresenter;
                if (p != null) p.StopBlock();

                if (!_orchestrator.IsRunning) { OnBattleEnd(); _state = BattleState.BattleEnd; return; }

                // 完美弹反 → 玩家反击；普通弹反 → 直接进入回合喘息
                if (_lastParryGrade == ParryGrade.PerfectParry)
                    TransitionTo(BattleState.PlayerCounter);
                else
                    TransitionTo(BattleState.PostEnemyTurn);
            }
        }

        private void TickPlayerCounter()
        {
            // 命中帧：攻击动画约 55% 处剑刃挥下，与屏幕震动同步。
            if (!_hitAppliedThisState && _stateTimer >= _stateDuration * 0.55f)
            {
                _hitAppliedThisState = true;
                ApplyPlayerDamage();
            }

            if (_stateTimer >= _stateDuration)
            {
                if (!_orchestrator.IsRunning) { OnBattleEnd(); _state = BattleState.BattleEnd; return; }
                TransitionTo(BattleState.CounterHitReaction);
            }
        }

        private void TickCounterHitReaction()
        {
            if (_stateTimer >= _stateDuration)
            {
                if (!_orchestrator.IsRunning) { OnBattleEnd(); _state = BattleState.BattleEnd; return; }

                // 反击之后 orchestrator 处于 EnemyAction 阶段（来自 SubmitPlayerAction）。
                // 强制 0 伤害推进到下一回合，避免重复触发敌人攻击。
                _orchestrator.PendingParryDamageModifierBp = 0;
                _orchestrator.AdvanceToResolution();

                TransitionTo(BattleState.PostEnemyTurn);
            }
        }

        private void TickEnemyStrike()
        {
            if (!_hitAppliedThisState)
            {
                _hitAppliedThisState = true;
                ApplyEnemyDamage();
            }

            if (_stateTimer >= _stateDuration)
            {
                if (!_orchestrator.IsRunning) { OnBattleEnd(); _state = BattleState.BattleEnd; return; }
                TransitionTo(BattleState.PostEnemyTurn);
            }
        }

        private void TickPostEnemyTurn()
        {
            if (_stateTimer >= _stateDuration)
                TransitionTo(BattleState.PlayerInput);
        }

        // ───────────── Damage / Parry application ─────────────

        private void ApplyPlayerDamage()
        {
            if (_sceneSetup?.PresenterManager != null)
            {
                _sceneSetup.PresenterManager.SetLastAttacker(_orchestrator.PlayerInstanceId);
                _sceneSetup.PresenterManager.SetSkipNextAttackAnim(true);
            }

            var result = _orchestrator.SubmitPlayerAction(
                _orchestrator.PlayerInstanceId,
                _orchestrator.EnemyInstanceId);

            if (result.Outcome != BattleActionOutcome.Accepted)
                Log.Warning("[Battle] Player action rejected: {0}", result.Outcome);
        }

        private void ApplyEnemyDamage()
        {
            if (_sceneSetup?.PresenterManager != null)
            {
                _sceneSetup.PresenterManager.SetLastAttacker(_orchestrator.EnemyInstanceId);
                _sceneSetup.PresenterManager.SetSkipNextAttackAnim(true);
            }

            _orchestrator.AdvanceToResolution();
        }

        private void ResolveParrySuccess()
        {
            if (_sceneSetup?.PresenterManager != null)
            {
                _sceneSetup.PresenterManager.SetLastAttacker(_orchestrator.EnemyInstanceId);
                _sceneSetup.PresenterManager.SetSkipNextAttackAnim(true);
                // 弹反成功（无论 Perfect/Normal）：玩家保持格挡，不打断为受击动画
                _sceneSetup.PresenterManager.SetSkipNextHitReaction(true);
            }

            _orchestrator.AdvanceToResolution();

            // 完美弹反：敌人被反弹播放受击动画；普通弹反不反弹（敌人姿态保持）
            if (_lastParryGrade == ParryGrade.PerfectParry)
            {
                var ep = _sceneSetup?.EnemyPresenter;
                if (ep != null) ep.PlayHitReaction();
            }
        }

        // ───────────── Helpers ─────────────

        private void PublishParryResult()
        {
            var ctx = new SceneEventContext("battle", 1, _orchestrator.Session.BattleContextId, 1);
            _orchestrator.Bus.Publish(new ParryResolved(
                ctx, _orchestrator.Clock.NowMs,
                $"parry_t{_orchestrator.TurnNumber}",
                $"enemy_seg_t{_orchestrator.TurnNumber}",
                1,
                _lastParryGrade,
                0,
                _lastParryGrade == ParryGrade.PerfectParry ? 0 :
                    _lastParryGrade == ParryGrade.NormalParry ? 5000 : 10000,
                _lastParryGrade == ParryGrade.PerfectParry ? EchoIntent.High : EchoIntent.None));
        }

        private void NotifyHudPhase(int phaseId, float duration)
        {
            OnEnemyAttackPhaseChanged?.Invoke(phaseId, duration, ParryWindowDuration, PerfectWindowDuration);
        }

        private static bool IsAttackInputDetected()
        {
#if UNITY_EDITOR || (!UNITY_IOS && !UNITY_ANDROID)
            return UnityEngine.Input.GetMouseButtonDown(0);
#else
            return UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began;
#endif
        }

        private void OnCharacterDefeated(CharacterDefeated evt)
        {
            if (_battleEnded) return;
            _battleEnded = true;
            Log.Info("[Battle] Defeated detected: {0}. Stopping state machine.", evt.InstanceId);
        }

        private void OnBattleEnd()
        {
            var player = _orchestrator.CharacterRepo.GetInstance(_orchestrator.PlayerInstanceId);
            bool playerWon = player != null && player.Status == CharacterStatus.Active;
            Log.Info("[Battle] Ended. Result={0}, Turns={1}", playerWon ? "Victory" : "Defeat", _orchestrator.TurnNumber);
        }

        private void OnDestroy()
        {
            _defeatedSubscription?.Dispose();
            _defeatedSubscription = null;

            _touchAreaManager?.Dispose();
            _touchAreaManager = null;

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

            UnityEngine.Debug.Log($"[BattleFlowController] OpenBattleHud: calling GameEntry.UI.OpenUIForm(BattleHudForm=202)");
            var serialId = GameEntry.UI.OpenUIForm(UIFormId.BattleHudForm, openData);
            UnityEngine.Debug.Log($"[BattleFlowController] OpenUIForm returned serialId={serialId}");
        }
    }
}
