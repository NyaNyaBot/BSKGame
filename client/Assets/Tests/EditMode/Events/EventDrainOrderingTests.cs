using System.Collections.Generic;
using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Events
{
    /// <summary>
    /// ADR-0002 + ADR-0004 / story-002：入队与同 tick 确定性 drain。
    /// </summary>
    public sealed class EventDrainOrderingTests
    {
        private readonly struct TestEvent : IBattleEvent
        {
            public string EventId { get; }
            public int SchemaVersion { get; }
            public SceneEventContext Context { get; }
            public long OccurredAtCombatClockMs { get; }
            public string Label { get; }

            public TestEvent(string eventId, SceneEventContext ctx, long clockMs, string label)
            {
                EventId = eventId;
                SchemaVersion = 1;
                Context = ctx;
                OccurredAtCombatClockMs = clockMs;
                Label = label;
            }
        }

        [Test]
        public void Publish_DuringHandler_QueuesInstead_OfImmediateDispatch()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var ctx = battle.ToEventContext();
            var bus = new BattleEventBus(svc);

            var order = new List<string>();

            bus.Subscribe<TestEvent>(e =>
            {
                order.Add(e.Label);
                if (e.Label == "A")
                {
                    bus.Publish(new TestEvent("e2", ctx, 100L, "B"));
                    bus.Publish(new TestEvent("e3", ctx, 100L, "C"));
                }
            });

            bus.Publish(new TestEvent("e1", ctx, 100L, "A"));

            Assert.That(order, Is.EqualTo(new[] { "A" }),
                "Events published during handler should be queued, not immediately dispatched");
            Assert.That(bus.PendingCount, Is.EqualTo(2));
        }

        [Test]
        public void DrainQueue_ProcessesQueuedEvents_InEnqueueOrder()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var ctx = battle.ToEventContext();
            var bus = new BattleEventBus(svc);

            var order = new List<string>();

            bus.Subscribe<TestEvent>(e =>
            {
                order.Add(e.Label);
                if (e.Label == "A")
                {
                    bus.Publish(new TestEvent("e2", ctx, 100L, "B"));
                    bus.Publish(new TestEvent("e3", ctx, 100L, "C"));
                }
            });

            bus.Publish(new TestEvent("e1", ctx, 100L, "A"));
            bus.DrainQueue();

            Assert.That(order, Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(bus.PendingCount, Is.EqualTo(0));
        }

        [Test]
        public void DrainQueue_NestedPublish_DrainsAllLevels()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var ctx = battle.ToEventContext();
            var bus = new BattleEventBus(svc);

            var order = new List<string>();

            bus.Subscribe<TestEvent>(e =>
            {
                order.Add(e.Label);
                if (e.Label == "A")
                    bus.Publish(new TestEvent("e2", ctx, 100L, "B"));
                else if (e.Label == "B")
                    bus.Publish(new TestEvent("e3", ctx, 100L, "C"));
            });

            bus.Publish(new TestEvent("e1", ctx, 100L, "A"));
            bus.DrainQueue();

            Assert.That(order, Is.EqualTo(new[] { "A", "B", "C" }));
        }

        [Test]
        public void DrainQueue_EmptyQueue_ReturnsZero()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var bus = new BattleEventBus(svc);

            var count = bus.DrainQueue();
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void DrainQueue_ExceedsMaxEventsPerDrain_Throws()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var ctx = battle.ToEventContext();
            var bus = new BattleEventBus(svc) { MaxEventsPerDrain = 3 };

            int counter = 0;
            bus.Subscribe<TestEvent>(e =>
            {
                counter++;
                if (counter < 10)
                    bus.Publish(new TestEvent($"loop-{counter}", ctx, 100L, $"L{counter}"));
            });

            bus.Publish(new TestEvent("e1", ctx, 100L, "Start"));

            Assert.Throws<System.InvalidOperationException>(() => bus.DrainQueue());
        }

        [Test]
        public void ClearPending_RemovesAllQueuedEvents()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var ctx = battle.ToEventContext();
            var bus = new BattleEventBus(svc);

            bus.Subscribe<TestEvent>(e =>
            {
                bus.Publish(new TestEvent("q1", ctx, 100L, "queued1"));
                bus.Publish(new TestEvent("q2", ctx, 100L, "queued2"));
            });

            bus.Publish(new TestEvent("e1", ctx, 100L, "trigger"));
            Assert.That(bus.PendingCount, Is.EqualTo(2));

            bus.ClearPending();
            Assert.That(bus.PendingCount, Is.EqualTo(0));
        }
    }
}
