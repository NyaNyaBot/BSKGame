using System;
using System.Collections.Generic;
using Game.Gameplay.SceneLifecycle;

namespace Game.Gameplay.Events
{
    /// <summary>
    /// 轻量同步进程内战斗事件总线（ADR-0002）。
    /// 支持 typed publish/subscribe、context filtering、入队与确定性 drain（ADR-0004）。
    /// </summary>
    public sealed class BattleEventBus : IBattleEventBus
    {
        public const int DefaultMaxRecursionDepth = 8;
        public const int DefaultMaxEventsPerDrain = 256;

        private readonly ISceneContextService _sceneContext;
        private readonly Dictionary<Type, object> _subscriptions = new Dictionary<Type, object>();

        private readonly Queue<QueuedEvent> _pendingQueue = new Queue<QueuedEvent>();
        private bool _isDispatching;
        private int _currentDrainDepth;

        public int MaxRecursionDepth { get; set; } = DefaultMaxRecursionDepth;
        public int MaxEventsPerDrain { get; set; } = DefaultMaxEventsPerDrain;

        /// <summary>
        /// 当前 drain 循环内已处理的事件计数（Debug 诊断用）。
        /// </summary>
        public int LastDrainEventCount { get; private set; }

        public BattleEventBus(ISceneContextService sceneContext)
        {
            _sceneContext = sceneContext ?? throw new ArgumentNullException(nameof(sceneContext));
        }

        /// <summary>
        /// 发布事件。若当前正在 dispatch 中，事件入队等待 drain；否则直接分发。
        /// </summary>
        public void Publish<TEvent>(TEvent evt) where TEvent : IBattleEvent
        {
            if (_isDispatching)
            {
                _pendingQueue.Enqueue(QueuedEvent.Create(evt));
                return;
            }

            DispatchImmediate(evt);
        }

        public IDisposable Subscribe<TEvent>(
            BattleEventHandler<TEvent> handler,
            BattleEventSubscriptionOptions options = default)
            where TEvent : IBattleEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var type = typeof(TEvent);
            if (!_subscriptions.TryGetValue(type, out var raw))
            {
                raw = new SubscriptionList<TEvent>();
                _subscriptions[type] = raw;
            }

            var list = (SubscriptionList<TEvent>)raw;
            return list.Add(handler, options);
        }

        /// <summary>
        /// drain 入口：由 turn manager / combat tick runner 调用（ADR-0004）。
        /// 按入队顺序处理挂起事件，直到队列为空或达到上限。
        /// </summary>
        /// <returns>本次 drain 处理的事件数。</returns>
        public int DrainQueue()
        {
            int processed = 0;
            _currentDrainDepth++;

            if (_currentDrainDepth > MaxRecursionDepth)
            {
                _currentDrainDepth--;
                throw new InvalidOperationException(
                    $"BattleEventBus: drain recursion depth exceeded {MaxRecursionDepth}. " +
                    "Possible infinite publish loop detected.");
            }

            try
            {
                while (_pendingQueue.Count > 0)
                {
                    if (processed >= MaxEventsPerDrain)
                    {
                        throw new InvalidOperationException(
                            $"BattleEventBus: per-drain event limit exceeded ({MaxEventsPerDrain}). " +
                            "Possible infinite event production loop.");
                    }

                    var queued = _pendingQueue.Dequeue();
                    queued.DispatchTo(this);
                    processed++;
                }
            }
            finally
            {
                _currentDrainDepth--;
                LastDrainEventCount = processed;
            }

            return processed;
        }

        /// <summary>
        /// 挂起队列中的事件数（诊断用）。
        /// </summary>
        public int PendingCount => _pendingQueue.Count;

        /// <summary>
        /// 清除所有挂起事件（战斗结束/场景卸载时调用）。
        /// </summary>
        public void ClearPending()
        {
            _pendingQueue.Clear();
        }

        private void DispatchImmediate<TEvent>(TEvent evt) where TEvent : IBattleEvent
        {
            var type = typeof(TEvent);
            if (!_subscriptions.TryGetValue(type, out var raw))
                return;

            var list = (SubscriptionList<TEvent>)raw;
            _isDispatching = true;
            try
            {
                list.Dispatch(evt, _sceneContext);
            }
            finally
            {
                _isDispatching = false;
            }
        }

        #region Inner types

        private readonly struct QueuedEvent
        {
            private readonly Action<BattleEventBus> _dispatch;

            public static QueuedEvent Create<TEvent>(TEvent evt) where TEvent : IBattleEvent
            {
                return new QueuedEvent(bus => bus.DispatchImmediate(evt));
            }

            private QueuedEvent(Action<BattleEventBus> dispatch)
            {
                _dispatch = dispatch;
            }

            public void DispatchTo(BattleEventBus bus) => _dispatch(bus);
        }

        private sealed class SubscriptionList<TEvent> where TEvent : IBattleEvent
        {
            private readonly List<Entry> _entries = new List<Entry>();
            private bool _dirty;

            public IDisposable Add(BattleEventHandler<TEvent> handler, BattleEventSubscriptionOptions options)
            {
                var entry = new Entry(handler, options);
                _entries.Add(entry);
                _dirty = true;
                return new Unsubscriber(this, entry);
            }

            public void Dispatch(TEvent evt, ISceneContextService sceneContext)
            {
                if (_dirty)
                {
                    _entries.Sort((a, b) => b.Options.Priority.CompareTo(a.Options.Priority));
                    _dirty = false;
                }

                var ctx = evt.Context;
                for (int i = 0; i < _entries.Count; i++)
                {
                    var e = _entries[i];
                    if (e.Removed) continue;

                    if (e.Options.DropStaleContext && !sceneContext.IsCurrent(in ctx))
                        continue;

                    e.Handler(evt);
                }
            }

            private void Remove(Entry entry)
            {
                entry.Removed = true;
            }

            private sealed class Entry
            {
                public readonly BattleEventHandler<TEvent> Handler;
                public readonly BattleEventSubscriptionOptions Options;
                public bool Removed;

                public Entry(BattleEventHandler<TEvent> handler, BattleEventSubscriptionOptions options)
                {
                    Handler = handler;
                    Options = options;
                }
            }

            private sealed class Unsubscriber : IDisposable
            {
                private SubscriptionList<TEvent>? _list;
                private Entry? _entry;

                public Unsubscriber(SubscriptionList<TEvent> list, Entry entry)
                {
                    _list = list;
                    _entry = entry;
                }

                public void Dispose()
                {
                    _list?.Remove(_entry!);
                    _list = null;
                    _entry = null;
                }
            }
        }

        #endregion
    }
}
