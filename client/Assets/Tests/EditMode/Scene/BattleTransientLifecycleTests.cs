using System;
using Game.Gameplay.Combat;
using Game.Gameplay.Events;
using Game.Gameplay.Input;
using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Scene
{
    /// <summary>
    /// ADR-0001 / story-003：战斗临时运行态创建与销毁。
    /// 验证 BattleSession.Dispose 释放所有订阅、清除挂起事件、
    /// 重置时钟、清空触区，且不泄漏。
    /// </summary>
    public sealed class BattleTransientLifecycleTests
    {
        private SceneContextService _ctx;
        private BattleEventBus _bus;
        private CombatClock _clock;
        private InputHitAreaRegistry _hitAreas;

        [SetUp]
        public void SetUp()
        {
            _ctx = new SceneContextService();
            _ctx.EnterScene(SceneKind.Battle);
            _bus = new BattleEventBus(_ctx);
            _clock = new CombatClock();
            _hitAreas = new InputHitAreaRegistry();
        }

        private BattleSession CreateSession()
        {
            var battle = _ctx.CreateBattleContext(default);
            return new BattleSession(battle.BattleContextId, _ctx, _bus, _clock, _hitAreas);
        }

        [Test]
        public void Dispose_ClearsAllSubscriptions()
        {
            var session = CreateSession();

            int callCount = 0;
            var sub = _bus.Subscribe<TestLifecycleEvent>(e => callCount++);
            session.TrackSubscription(sub);

            Assert.That(session.TrackedSubscriptionCount, Is.EqualTo(1));

            session.Dispose();

            Assert.That(session.TrackedSubscriptionCount, Is.EqualTo(0));

            var battle = _ctx.CreateBattleContext(default);
            var evt = new TestLifecycleEvent(battle.ToEventContext());
            _bus.Publish(evt);

            Assert.That(callCount, Is.EqualTo(0), "Disposed subscription must not receive events");
        }

        [Test]
        public void Dispose_ClearsPendingEvents()
        {
            var session = CreateSession();

            _bus.Subscribe<TestLifecycleEvent>(e =>
            {
                _bus.Publish(new TestLifecycleEvent(e.Context));
            });

            var battle = _ctx.CreateBattleContext(default);
            _bus.Publish(new TestLifecycleEvent(battle.ToEventContext()));
            Assert.That(_bus.PendingCount, Is.GreaterThan(0));

            session.Dispose();

            Assert.That(_bus.PendingCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_ResetsClock()
        {
            var session = CreateSession();

            _clock.Advance(200);
            Assert.That(_clock.NowMs, Is.EqualTo(200));

            session.Dispose();

            Assert.That(_clock.NowMs, Is.EqualTo(0));
            Assert.That(_clock.IsPaused, Is.False);
        }

        [Test]
        public void Dispose_ClearsHitAreaRegistry()
        {
            var session = CreateSession();

            _hitAreas.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry));
            _hitAreas.Register(new HitAreaRegistration("counter", InputHitAreaRegistry.PriorityCounter));
            Assert.That(_hitAreas.RegisteredCount, Is.EqualTo(2));

            session.Dispose();

            Assert.That(_hitAreas.RegisteredCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_DisposeBattleContext_MakesOldContextStale()
        {
            var session = CreateSession();

            Assert.That(_ctx.TryGetActiveBattle(out var battle), Is.True);
            var eventCtx = battle.ToEventContext();
            Assert.That(_ctx.IsCurrent(eventCtx), Is.True);

            session.Dispose();

            Assert.That(_ctx.IsCurrent(eventCtx), Is.False, "After dispose, old battle context must be stale");
        }

        [Test]
        public void Dispose_Twice_IsIdempotent()
        {
            var session = CreateSession();
            session.Dispose();
            Assert.DoesNotThrow(() => session.Dispose());
        }

        [Test]
        public void TrackSubscription_AfterDispose_Throws()
        {
            var session = CreateSession();
            session.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
                session.TrackSubscription(new DummyDisposable()));
        }

        [Test]
        public void TwoConsecutiveSessions_NoLeakage()
        {
            var session1 = CreateSession();
            int call1 = 0;
            var sub1 = _bus.Subscribe<TestLifecycleEvent>(e => call1++);
            session1.TrackSubscription(sub1);
            _hitAreas.Register(new HitAreaRegistration("area1", 100));
            _clock.Advance(100);

            session1.Dispose();

            var session2 = CreateSession();
            int call2 = 0;
            var sub2 = _bus.Subscribe<TestLifecycleEvent>(e => call2++);
            session2.TrackSubscription(sub2);

            var battle2 = _ctx.CreateBattleContext(default);
            _bus.Publish(new TestLifecycleEvent(battle2.ToEventContext()));

            Assert.That(call1, Is.EqualTo(0), "Session 1 handler must not fire after dispose");
            Assert.That(call2, Is.EqualTo(1), "Session 2 handler should fire");
            Assert.That(_clock.NowMs, Is.EqualTo(0), "Clock reset after session 1 dispose");
            Assert.That(_hitAreas.RegisteredCount, Is.EqualTo(0), "Hit areas cleared after session 1 dispose");

            session2.Dispose();
        }

        #region Test helpers

        private readonly struct TestLifecycleEvent : IBattleEvent
        {
            public string EventId => "test.lifecycle";
            public int SchemaVersion => 1;
            public SceneEventContext Context { get; }
            public long OccurredAtCombatClockMs => 0;

            public TestLifecycleEvent(SceneEventContext ctx)
            {
                Context = ctx;
            }
        }

        private sealed class DummyDisposable : IDisposable
        {
            public void Dispose() { }
        }

        #endregion
    }
}
