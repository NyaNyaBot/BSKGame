using System;
using System.Collections.Generic;
using Game.Gameplay.Combat;
using Game.Gameplay.Events;

namespace Game.Gameplay.TurnManager
{
    /// <summary>
    /// 同 tick 事件编排器（ADR-0004 / TR-turn-004）。
    /// 单一 Tick 入口按 ADR-0004 顺序调用子系统回调，
    /// 然后 drain 总线队列。
    /// </summary>
    public sealed class CombatTickRunner
    {
        /// <summary>
        /// ADR-0004 规定的子步骤顺序：
        /// 0=Dispose/Context, 1=Pause/HitStop, 2=InputPhase,
        /// 3=InputMapping, 4=Parry, 5=Damage, 6=Death,
        /// 7=CounterWindow, 8=UIFeedback, 9=Debug
        /// </summary>
        public const int StepCount = 10;

        private readonly ICombatClockController _clock;
        private readonly BattleEventBus _bus;
        private readonly Action?[] _steps = new Action?[StepCount];

        public int LastTickStepsExecuted { get; private set; }

        public CombatTickRunner(ICombatClockController clock, BattleEventBus bus)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>
        /// 注册子步骤回调。stepIndex 必须在 [0, StepCount) 范围内。
        /// </summary>
        public void RegisterStep(int stepIndex, Action callback)
        {
            if (stepIndex < 0 || stepIndex >= StepCount)
                throw new System.ArgumentOutOfRangeException(nameof(stepIndex));
            _steps[stepIndex] = callback;
        }

        /// <summary>
        /// 执行一个 tick：推进时钟 → 按序执行子步骤 → drain 总线队列。
        /// </summary>
        public void Tick(long deltaMs)
        {
            _clock.Advance(deltaMs);

            int executed = 0;
            for (int i = 0; i < StepCount; i++)
            {
                _steps[i]?.Invoke();
                executed++;
            }

            _bus.DrainQueue();
            LastTickStepsExecuted = executed;
        }
    }
}
