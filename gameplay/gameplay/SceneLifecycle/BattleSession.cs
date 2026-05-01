using System;
using System.Collections.Generic;
using Game.Gameplay.Combat;
using Game.Gameplay.Events;
using Game.Gameplay.Input;

namespace Game.Gameplay.SceneLifecycle
{
    /// <summary>
    /// 战斗临时运行态聚合根（ADR-0001 / story-003）。
    /// 拥有一场战斗生命周期内所有临时资源（时钟、事件总线、触区注册表、订阅令牌）。
    /// Dispose 时按确定顺序 teardown：冻结输入 → 清挂起事件 → 释放订阅 → 重置时钟 → 清触区。
    /// </summary>
    public sealed class BattleSession : IDisposable
    {
        private readonly ISceneContextService _sceneContext;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _isDisposed;

        public string BattleContextId { get; }
        public BattleEventBus EventBus { get; }
        public CombatClock Clock { get; }
        public InputHitAreaRegistry HitAreaRegistry { get; }

        public bool IsDisposed => _isDisposed;

        public BattleSession(
            string battleContextId,
            ISceneContextService sceneContext,
            BattleEventBus eventBus,
            CombatClock clock,
            InputHitAreaRegistry hitAreaRegistry)
        {
            BattleContextId = battleContextId ?? throw new ArgumentNullException(nameof(battleContextId));
            _sceneContext = sceneContext ?? throw new ArgumentNullException(nameof(sceneContext));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            HitAreaRegistry = hitAreaRegistry ?? throw new ArgumentNullException(nameof(hitAreaRegistry));
        }

        /// <summary>
        /// 注册一个随 session 生命周期管理的订阅令牌。
        /// </summary>
        public void TrackSubscription(IDisposable subscription)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(BattleSession));
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));
            _subscriptions.Add(subscription);
        }

        public int TrackedSubscriptionCount => _subscriptions.Count;

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            EventBus.ClearPending();

            for (int i = _subscriptions.Count - 1; i >= 0; i--)
            {
                _subscriptions[i].Dispose();
            }
            _subscriptions.Clear();

            Clock.Reset();
            HitAreaRegistry.Clear();

            _sceneContext.DisposeBattle(BattleContextId, BattleDisposeReason.NormalEnd);
        }
    }
}
