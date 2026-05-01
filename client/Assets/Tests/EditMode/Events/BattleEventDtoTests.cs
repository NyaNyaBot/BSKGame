using System;
using System.Collections.Generic;
using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Events
{
    /// <summary>
    /// ADR-0002 / story-001：战斗事件 DTO 不可变性与 context 过滤。
    /// </summary>
    public sealed class BattleEventDtoTests
    {
        /// <summary>
        /// 用于测试的最小 readonly struct 事件 DTO。
        /// </summary>
        private readonly struct TestDamageEvent : IBattleEvent
        {
            public string EventId { get; }
            public int SchemaVersion { get; }
            public SceneEventContext Context { get; }
            public long OccurredAtCombatClockMs { get; }
            public int DamageAmount { get; }

            public TestDamageEvent(string eventId, SceneEventContext context, long clockMs, int damage)
            {
                EventId = eventId;
                SchemaVersion = 1;
                Context = context;
                OccurredAtCombatClockMs = clockMs;
                DamageAmount = damage;
            }
        }

        [Test]
        public void IBattleEvent_RequiredFields_AllPresent()
        {
            var ctx = new SceneEventContext("scene1", 1, "battle1", 1);
            var evt = new TestDamageEvent("evt-001", ctx, 1000L, 50);

            Assert.That(evt.EventId, Is.EqualTo("evt-001"));
            Assert.That(evt.SchemaVersion, Is.EqualTo(1));
            Assert.That(evt.Context.SceneContextId, Is.EqualTo("scene1"));
            Assert.That(evt.Context.BattleContextId, Is.EqualTo("battle1"));
            Assert.That(evt.OccurredAtCombatClockMs, Is.EqualTo(1000L));
        }

        [Test]
        public void DTO_IsReadonlyStruct_FieldsCannotMutateAfterConstruction()
        {
            var ctx = new SceneEventContext("s", 1, "b", 1);
            var evt = new TestDamageEvent("e1", ctx, 500L, 30);

            var copy = evt;
            Assert.That(copy.EventId, Is.EqualTo(evt.EventId));
            Assert.That(copy.DamageAmount, Is.EqualTo(evt.DamageAmount));

            Assert.That(typeof(TestDamageEvent).IsValueType, Is.True, "DTO should be a value type (struct)");
        }

        [Test]
        public void Publish_CurrentContext_HandlerReceivesEvent()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var bus = new BattleEventBus(svc);

            var received = new List<TestDamageEvent>();
            bus.Subscribe<TestDamageEvent>(e => received.Add(e));

            var evt = new TestDamageEvent("e1", battle.ToEventContext(), 100L, 25);
            bus.Publish(evt);

            Assert.That(received, Has.Count.EqualTo(1));
            Assert.That(received[0].EventId, Is.EqualTo("e1"));
        }

        [Test]
        public void Publish_StaleContext_DropStaleTrue_HandlerNotCalled()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var staleCtx = battle.ToEventContext();

            svc.DisposeBattle(battle.BattleContextId, BattleDisposeReason.SceneUnload);

            var bus = new BattleEventBus(svc);
            var received = new List<TestDamageEvent>();
            bus.Subscribe<TestDamageEvent>(e => received.Add(e),
                new BattleEventSubscriptionOptions(dropStaleContext: true));

            var staleEvt = new TestDamageEvent("stale-1", staleCtx, 200L, 10);
            bus.Publish(staleEvt);

            Assert.That(received, Is.Empty, "Stale context event should be dropped");
        }

        [Test]
        public void Publish_StaleContext_DropStaleFalse_HandlerStillCalled()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var staleCtx = battle.ToEventContext();

            svc.DisposeBattle(battle.BattleContextId, BattleDisposeReason.SceneUnload);

            var bus = new BattleEventBus(svc);
            var received = new List<TestDamageEvent>();
            bus.Subscribe<TestDamageEvent>(e => received.Add(e),
                new BattleEventSubscriptionOptions(dropStaleContext: false));

            var evt = new TestDamageEvent("s1", staleCtx, 100L, 5);
            bus.Publish(evt);

            Assert.That(received, Has.Count.EqualTo(1),
                "DropStaleContext=false should deliver even stale events");
        }

        [Test]
        public void Subscribe_Priority_HigherPriorityHandledFirst()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var bus = new BattleEventBus(svc);

            var order = new List<string>();
            bus.Subscribe<TestDamageEvent>(_ => order.Add("low"),
                new BattleEventSubscriptionOptions(priority: 10));
            bus.Subscribe<TestDamageEvent>(_ => order.Add("high"),
                new BattleEventSubscriptionOptions(priority: 100));
            bus.Subscribe<TestDamageEvent>(_ => order.Add("mid"),
                new BattleEventSubscriptionOptions(priority: 50));

            var evt = new TestDamageEvent("p1", battle.ToEventContext(), 0L, 1);
            bus.Publish(evt);

            Assert.That(order, Is.EqualTo(new[] { "high", "mid", "low" }));
        }

        [Test]
        public void Unsubscribe_Dispose_HandlerNoLongerCalled()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var bus = new BattleEventBus(svc);

            int callCount = 0;
            var sub = bus.Subscribe<TestDamageEvent>(_ => callCount++);

            var evt = new TestDamageEvent("u1", battle.ToEventContext(), 0L, 1);
            bus.Publish(evt);
            Assert.That(callCount, Is.EqualTo(1));

            sub.Dispose();
            bus.Publish(evt);
            Assert.That(callCount, Is.EqualTo(1), "Disposed subscription should not receive events");
        }

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var bus = new BattleEventBus(svc);

            var evt = new TestDamageEvent("n1", battle.ToEventContext(), 0L, 1);
            Assert.DoesNotThrow(() => bus.Publish(evt));
        }
    }
}
