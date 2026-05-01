using System;
using System.Collections.Generic;
using Game.Gameplay.Character;
using Game.Gameplay.TurnManager;

namespace Game.Gameplay.UI
{
    /// <summary>
    /// 战斗 HUD 视图模型（ADR-0010 / TR-ui-001, TR-ui-003）。
    /// 纯数据驱动：事件到达时更新字段，UI 层订阅 Changed 回调刷新。
    /// 不持有可变 gameplay 对象的引用。
    /// </summary>
    public sealed class BattleHudViewModel
    {
        public CharacterBattleSnapshot PlayerSnapshot { get; private set; }
        public CharacterBattleSnapshot EnemySnapshot { get; private set; }
        public BattleInputPhase CurrentPhase { get; private set; }
        public bool IsCounterAvailable { get; private set; }
        public bool IsParryReady { get; private set; }
        public bool IsDebugOverlayEnabled { get; set; }
        public long DebugCombatClockMs { get; private set; }
        public string DebugAttackSegmentId { get; private set; } = string.Empty;

        public BattleResultInfo? Result { get; private set; }

        public event Action? OnChanged;

        public void UpdatePlayerSnapshot(CharacterBattleSnapshot snapshot)
        {
            PlayerSnapshot = snapshot;
            OnChanged?.Invoke();
        }

        public void UpdateEnemySnapshot(CharacterBattleSnapshot snapshot)
        {
            EnemySnapshot = snapshot;
            OnChanged?.Invoke();
        }

        public void UpdatePhase(BattleInputPhase phase)
        {
            CurrentPhase = phase;
            IsParryReady = phase == BattleInputPhase.EnemyAction;
            OnChanged?.Invoke();
        }

        public void SetCounterAvailable(bool available)
        {
            IsCounterAvailable = available;
            OnChanged?.Invoke();
        }

        public void SetBattleResult(BattleResultInfo result)
        {
            Result = result;
            OnChanged?.Invoke();
        }

        public void UpdateDebugInfo(long combatClockMs, string attackSegmentId)
        {
            DebugCombatClockMs = combatClockMs;
            DebugAttackSegmentId = attackSegmentId ?? string.Empty;
        }

        public void Reset()
        {
            PlayerSnapshot = default;
            EnemySnapshot = default;
            CurrentPhase = BattleInputPhase.None;
            IsCounterAvailable = false;
            IsParryReady = false;
            IsDebugOverlayEnabled = false;
            DebugCombatClockMs = 0;
            DebugAttackSegmentId = string.Empty;
            Result = null;
        }
    }

    public readonly struct BattleResultInfo
    {
        public BattleOutcome Outcome { get; }
        public string DefeatedInstanceId { get; }

        public BattleResultInfo(BattleOutcome outcome, string defeatedInstanceId)
        {
            Outcome = outcome;
            DefeatedInstanceId = defeatedInstanceId ?? string.Empty;
        }
    }

    public enum BattleOutcome : byte
    {
        Victory = 0,
        Defeat = 1,
    }
}
