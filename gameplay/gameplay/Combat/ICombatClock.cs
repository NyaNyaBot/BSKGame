namespace Game.Gameplay.Combat
{
    /// <summary>
    /// 只读战斗时钟（ADR-0004）。
    /// 输入、弹反、伤害、UI/反馈等系统读取逻辑时间；
    /// 仅 turn manager 通过 <see cref="ICombatClockController"/> 可写。
    /// </summary>
    public interface ICombatClock
    {
        /// <summary>
        /// 当前逻辑战斗时间（毫秒）。
        /// hit stop 期间此值冻结不变。
        /// </summary>
        long NowMs { get; }

        /// <summary>
        /// 时钟是否因 hit stop 暂停。
        /// </summary>
        bool IsPaused { get; }
    }

    /// <summary>
    /// 可写战斗时钟控制器（ADR-0004），仅 turn manager 持有。
    /// </summary>
    public interface ICombatClockController : ICombatClock
    {
        void Advance(long deltaMs);
        void Pause();
        void Resume();
    }
}
