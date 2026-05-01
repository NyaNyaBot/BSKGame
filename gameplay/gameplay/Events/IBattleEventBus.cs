using System;

namespace Game.Gameplay.Events
{
    /// <summary>
    /// 战斗事件总线契约（ADR-0002）。
    /// 提供类型化 publish/subscribe，可选 context 过滤。
    /// drain 顺序由 ADR-0004 turn manager 控制，不由总线自行决定。
    /// </summary>
    public interface IBattleEventBus
    {
        void Publish<TEvent>(TEvent evt) where TEvent : IBattleEvent;

        IDisposable Subscribe<TEvent>(
            BattleEventHandler<TEvent> handler,
            BattleEventSubscriptionOptions options = default)
            where TEvent : IBattleEvent;
    }

    public delegate void BattleEventHandler<in TEvent>(TEvent evt)
        where TEvent : IBattleEvent;

    public readonly struct BattleEventSubscriptionOptions
    {
        /// <summary>
        /// 订阅优先级；数值越大越先收到事件。同优先级按订阅顺序。
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// 若为 true，总线在调用 handler 前用 ISceneContextService.IsCurrent 校验，
        /// context 不匹配则静默跳过（ADR-0001 / ADR-0002 stale 事件防护）。
        /// </summary>
        public readonly bool DropStaleContext;

        public BattleEventSubscriptionOptions(int priority = 0, bool dropStaleContext = true)
        {
            Priority = priority;
            DropStaleContext = dropStaleContext;
        }
    }
}
