namespace Game.Gameplay.TurnManager
{
    /// <summary>
    /// MVP 战斗输入阶段枚举（ADR-0004 / TR-turn-001）。
    /// 与 GDD 回合管理器命名对齐。
    /// </summary>
    public enum BattleInputPhase : byte
    {
        None = 0,
        BattleStart = 1,
        PlayerCommand = 2,
        EnemyAction = 3,
        Resolution = 4,
        TurnEnd = 5,
        BattleEnd = 6,
    }
}
