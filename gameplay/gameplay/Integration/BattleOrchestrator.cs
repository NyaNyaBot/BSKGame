using System;
using Game.Gameplay.BattleAction;
using Game.Gameplay.Character;
using Game.Gameplay.Combat;
using Game.Gameplay.Damage;
using Game.Gameplay.EnemyAI;
using Game.Gameplay.Events;
using Game.Gameplay.Feedback;
using Game.Gameplay.Input;
using Game.Gameplay.Parry;
using Game.Gameplay.SceneLifecycle;
using Game.Gameplay.TurnManager;

namespace Game.Gameplay.Integration
{
    /// <summary>
    /// 战斗系统聚合根 — 创建、串联、驱动、拆解所有纯 C# 战斗子系统。
    /// Unity 侧只需持有本类引用，每帧调用 Tick(deltaMs)。
    /// </summary>
    public sealed class BattleOrchestrator : IDisposable
    {
        private bool _disposed;
        private readonly ISceneContextService _sceneContext;

        // ── Infrastructure ──
        public BattleSession Session { get; }
        public BattleEventBus Bus => Session.EventBus;
        public CombatClock Clock => Session.Clock;
        public InputHitAreaRegistry HitAreaRegistry => Session.HitAreaRegistry;

        // ── Core ──
        public InputPhaseStateMachine PhaseMachine { get; }
        public CombatTickRunner TickRunner { get; }
        public CharacterRepository CharacterRepo { get; }
        public CharacterReadModel ReadModel { get; }

        // ── Damage & Action ──
        public DamageService DamageService { get; }
        public BattleActionService ActionService { get; }
        public CounterActionService CounterService { get; }

        // ── Parry ──
        public ParryResolver ParryResolver { get; }

        // ── Enemy AI ──
        public EnemyAttackPlanner AttackPlanner { get; }
        public EnemyComboManager ComboManager { get; }

        // ── Feedback ──
        public FeedbackProfileLookup FeedbackLookup { get; }
        public HitStopController HitStopController { get; }

        // ── State ──
        public int TurnNumber { get; private set; }
        public bool IsRunning { get; private set; }

        public BattleOrchestrator(ISceneContextService sceneContext, BattleStartConfig config)
        {
            _sceneContext = sceneContext ?? throw new ArgumentNullException(nameof(sceneContext));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var bus = new BattleEventBus(sceneContext);
            var clock = new CombatClock();
            var hitAreaRegistry = new InputHitAreaRegistry();

            Session = new BattleSession(
                config.BattleContextId ?? Guid.NewGuid().ToString("N"),
                sceneContext, bus, clock, hitAreaRegistry);

            PhaseMachine = new InputPhaseStateMachine();
            TickRunner = new CombatTickRunner(clock, bus);

            CharacterRepo = new CharacterRepository();
            ReadModel = new CharacterReadModel(CharacterRepo);
            DamageService = new DamageService(CharacterRepo);
            ActionService = new BattleActionService(PhaseMachine, CharacterRepo);
            CounterService = new CounterActionService(CharacterRepo, clock);

            ParryResolver = new ParryResolver();

            AttackPlanner = new EnemyAttackPlanner(CharacterRepo);
            ComboManager = new EnemyComboManager();

            FeedbackLookup = new FeedbackProfileLookup();
            HitStopController = new HitStopController(clock);
        }

        /// <summary>
        /// 初始化角色并启动战斗阶段流转。
        /// </summary>
        public void Start(CharacterDefinition playerDef, CharacterDefinition enemyDef)
        {
            if (IsRunning) return;

            CharacterRepo.CreateInstance(playerDef);
            CharacterRepo.CreateInstance(enemyDef);

            PhaseMachine.PhaseChanged += OnPhaseChanged;
            PhaseMachine.TryTransition(BattleInputPhase.BattleStart, out _);
            PhaseMachine.TryTransition(BattleInputPhase.PlayerCommand, out _);

            TurnNumber = 1;
            IsRunning = true;
        }

        /// <summary>
        /// 驱动一帧战斗逻辑。deltaMs 为实际经过的毫秒数。
        /// </summary>
        public void Tick(long deltaMs)
        {
            if (!IsRunning || _disposed) return;

            HitStopController.TickRealtime((int)deltaMs);
            TickRunner.Tick(deltaMs);
        }

        /// <summary>
        /// 玩家提交基础攻击。返回行动结果和可选的伤害结果。
        /// </summary>
        public BattleActionResult SubmitPlayerAction(string actorInstanceId, string targetInstanceId)
        {
            var requestId = $"action_t{TurnNumber}_{Clock.NowMs}";
            var request = new BattleActionRequest(requestId, actorInstanceId, targetInstanceId);
            var result = ActionService.Submit(request);

            if (result.Outcome == BattleActionOutcome.Accepted && result.GeneratedDamageRequest.HasValue)
            {
                var dmgResult = DamageService.ApplyDamage(result.GeneratedDamageRequest.Value);
                PublishDamageEvents(dmgResult);

                PhaseMachine.TryTransition(BattleInputPhase.EnemyAction, out _);
            }

            return result;
        }

        /// <summary>
        /// 推进到敌人行动结算，然后回到玩家回合。
        /// </summary>
        public void AdvanceToResolution()
        {
            if (PhaseMachine.CurrentPhase != BattleInputPhase.EnemyAction) return;

            PhaseMachine.TryTransition(BattleInputPhase.Resolution, out _);
            PhaseMachine.TryTransition(BattleInputPhase.TurnEnd, out _);

            AttackPlanner.TickCooldowns();
            ActionService.ClearCache();
            TurnNumber++;

            if (ShouldEndBattle())
            {
                PhaseMachine.TryTransition(BattleInputPhase.BattleEnd, out _);
                IsRunning = false;
            }
            else
            {
                PhaseMachine.TryTransition(BattleInputPhase.PlayerCommand, out _);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            IsRunning = false;

            PhaseMachine.PhaseChanged -= OnPhaseChanged;

            ComboManager.Clear();
            AttackPlanner.ClearCooldowns();
            CounterService.Clear();
            ParryResolver.Clear();
            FeedbackLookup.Clear();
            HitStopController.Reset();
            ActionService.ClearCache();
            DamageService.ClearCache();
            CharacterRepo.Clear();
            PhaseMachine.Reset();

            Session.Dispose();
        }

        #region Internal Wiring

        private SceneEventContext GetEventContext()
        {
            if (_sceneContext.TryGetActiveBattle(out var battle))
                return battle.ToEventContext();
            return SceneEventContext.ForSceneOnly(_sceneContext.CurrentScene);
        }

        private void PublishDamageEvents(DamageResult result)
        {
            if (result.Outcome != DamageOutcome.Applied) return;

            var ctx = GetEventContext();
            Bus.Publish(new DamageApplied(
                ctx, Clock.NowMs,
                result.RequestId, result.TargetInstanceId,
                result.ShieldAbsorbed, result.HpDamage,
                result.FinalHp, result.FinalShield));

            if (result.IsDefeated)
            {
                Bus.Publish(new CharacterDefeated(
                    ctx, Clock.NowMs,
                    result.TargetInstanceId, result.RequestId));
            }
        }

        private void OnPhaseChanged(BattleInputPhase previous, BattleInputPhase current)
        {
            var ctx = GetEventContext();
            Bus.Publish(new BattleInputPhaseChanged(ctx, Clock.NowMs, previous, current));
        }

        private bool ShouldEndBattle()
        {
            var instances = CharacterRepo.GetAllInstances();
            int activeCount = 0;
            for (int i = 0; i < instances.Count; i++)
            {
                if (instances[i].Status == CharacterStatus.Active)
                    activeCount++;
            }
            return activeCount < 2;
        }

        #endregion
    }

    /// <summary>
    /// 战斗启动配置。
    /// </summary>
    public class BattleStartConfig
    {
        public string BattleContextId { get; set; }
    }
}
