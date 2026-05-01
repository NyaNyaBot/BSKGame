using System;
using System.Collections.Generic;

namespace Game.Gameplay.TurnManager
{
    /// <summary>
    /// 战斗输入阶段状态机（ADR-0004 / TR-turn-001）。
    /// 仅 turn manager 可调用 TryTransition；对外只读暴露 CurrentPhase。
    /// </summary>
    public sealed class InputPhaseStateMachine
    {
        private static readonly Dictionary<BattleInputPhase, BattleInputPhase[]> LegalTransitions =
            new Dictionary<BattleInputPhase, BattleInputPhase[]>
            {
                { BattleInputPhase.None,          new[] { BattleInputPhase.BattleStart } },
                { BattleInputPhase.BattleStart,    new[] { BattleInputPhase.PlayerCommand } },
                { BattleInputPhase.PlayerCommand,  new[] { BattleInputPhase.EnemyAction, BattleInputPhase.Resolution } },
                { BattleInputPhase.EnemyAction,    new[] { BattleInputPhase.Resolution } },
                { BattleInputPhase.Resolution,     new[] { BattleInputPhase.TurnEnd } },
                { BattleInputPhase.TurnEnd,        new[] { BattleInputPhase.PlayerCommand, BattleInputPhase.BattleEnd } },
                { BattleInputPhase.BattleEnd,      Array.Empty<BattleInputPhase>() },
            };

        private BattleInputPhase _current = BattleInputPhase.None;

        public BattleInputPhase CurrentPhase => _current;

        public event Action<BattleInputPhase, BattleInputPhase>? PhaseChanged;

        /// <summary>
        /// 尝试迁移到目标阶段。合法则迁移并触发 PhaseChanged，返回 true；
        /// 非法则保持原阶段，填充拒绝原因，返回false。
        /// 重入同阶段视为 no-op 返回 true 但不触发事件。
        /// </summary>
        public bool TryTransition(BattleInputPhase target, out string? rejectionReason)
        {
            rejectionReason = null;

            if (target == _current)
                return true;

            if (!LegalTransitions.TryGetValue(_current, out var allowed))
            {
                rejectionReason = $"Unknown source phase: {_current}";
                return false;
            }

            bool found = false;
            for (int i = 0; i < allowed.Length; i++)
            {
                if (allowed[i] == target)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                rejectionReason = $"Illegal transition: {_current} → {target}";
                return false;
            }

            var previous = _current;
            _current = target;
            PhaseChanged?.Invoke(previous, target);
            return true;
        }

        /// <summary>
        /// 强制重置到 None（战斗结束/新战斗开始时使用）。
        /// </summary>
        public void Reset()
        {
            _current = BattleInputPhase.None;
        }

        /// <summary>
        /// 检查从当前阶段到目标阶段是否合法（不执行迁移）。
        /// </summary>
        public bool IsTransitionLegal(BattleInputPhase target)
        {
            if (target == _current) return true;
            if (!LegalTransitions.TryGetValue(_current, out var allowed)) return false;
            for (int i = 0; i < allowed.Length; i++)
            {
                if (allowed[i] == target) return true;
            }
            return false;
        }
    }
}
