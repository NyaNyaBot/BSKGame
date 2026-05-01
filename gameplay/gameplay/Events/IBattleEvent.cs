using Game.Gameplay.SceneLifecycle;

namespace Game.Gameplay.Events
{
    /// <summary>
    /// 所有战斗事件 DTO 的契约（ADR-0002）。
    /// 实现类型应为 readonly struct 或不可变 class，publish 后禁止修改。
    /// </summary>
    public interface IBattleEvent
    {
        /// <summary>
        /// 全局唯一事件 ID（每次 publish 唯一）。
        /// </summary>
        string EventId { get; }

        /// <summary>
        /// DTO schema 版本号；MVP 从 1 起。
        /// </summary>
        int SchemaVersion { get; }

        /// <summary>
        /// 事件发生时的场景/战斗上下文快照，用于 stale 事件过滤。
        /// </summary>
        SceneEventContext Context { get; }

        /// <summary>
        /// 事件发生时的逻辑战斗时钟（ms），由 ICombatClock 提供，非 Unity Time。
        /// </summary>
        long OccurredAtCombatClockMs { get; }
    }
}
