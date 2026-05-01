using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;

namespace Game.Gameplay.TurnManager
{
    /// <summary>
    /// 战斗输入阶段变更事件（ADR-0002, ADR-0004 / TR-turn-002）。
    /// 每次合法阶段迁移由 turn manager 发布一次。
    /// </summary>
    public readonly struct BattleInputPhaseChanged : IBattleEvent
    {
        public string EventId => "battle.input_phase_changed";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }

        public BattleInputPhase PreviousPhase { get; }
        public BattleInputPhase CurrentPhase { get; }

        public BattleInputPhaseChanged(
            SceneEventContext context,
            long occurredAtCombatClockMs,
            BattleInputPhase previousPhase,
            BattleInputPhase currentPhase)
        {
            Context = context;
            OccurredAtCombatClockMs = occurredAtCombatClockMs;
            PreviousPhase = previousPhase;
            CurrentPhase = currentPhase;
        }
    }
}
