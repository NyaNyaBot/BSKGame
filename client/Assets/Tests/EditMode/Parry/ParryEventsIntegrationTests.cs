using System.Collections.Generic;
using Game.Gameplay.Events;
using Game.Gameplay.Input;
using Game.Gameplay.Parry;
using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Parry
{
    /// <summary>
    /// ADR-0007 / story-003+004：弹反事件发布 + Counter 生命周期。
    /// </summary>
    public sealed class ParryEventsIntegrationTests
    {
        private SceneContextService _ctx;
        private BattleEventBus _bus;
        private SceneEventContext _eventCtx;

        [SetUp]
        public void SetUp()
        {
            _ctx = new SceneContextService();
            _ctx.EnterScene(SceneKind.Battle);
            _bus = new BattleEventBus(_ctx);
            var battle = _ctx.CreateBattleContext(default);
            _eventCtx = battle.ToEventContext();
        }

        [Test]
        public void ParryResolved_PerfectParry_PublishesCorrectEvent()
        {
            var received = new List<ParryResolved>();
            _bus.Subscribe<ParryResolved>(e => received.Add(e));

            _bus.Publish(new ParryResolved(
                _eventCtx, 800, "res1", "seg1", 1,
                ParryGrade.PerfectParry, 0, 0, EchoIntent.High));

            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].Grade, Is.EqualTo(ParryGrade.PerfectParry));
            Assert.That(received[0].EchoIntent, Is.EqualTo(EchoIntent.High));
        }

        [Test]
        public void ParryCancelled_NotFailedParry()
        {
            var resolved = new List<ParryResolved>();
            var cancelled = new List<ParryCancelled>();
            _bus.Subscribe<ParryResolved>(e => resolved.Add(e));
            _bus.Subscribe<ParryCancelled>(e => cancelled.Add(e));

            _bus.Publish(new ParryCancelled(_eventCtx, 900, "seg1", 1, "TargetInvalid"));

            Assert.That(cancelled.Count, Is.EqualTo(1));
            Assert.That(cancelled[0].Reason, Is.EqualTo("TargetInvalid"));
            Assert.That(resolved.Count, Is.EqualTo(0), "Cancellation must not produce ParryResolved");
        }

        [Test]
        public void TimelineRejected_PublishesValidationError()
        {
            var received = new List<AttackSegmentTimelineRejected>();
            _bus.Subscribe<AttackSegmentTimelineRejected>(e => received.Add(e));

            _bus.Publish(new AttackSegmentTimelineRejected(_eventCtx, 0, "seg1", "negative windup"));

            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].ValidationError, Does.Contain("negative"));
        }

        [Test]
        public void PerfectParry_OpensCounterEntry()
        {
            var counters = new List<CounterEntryOpened>();
            _bus.Subscribe<CounterEntryOpened>(e => counters.Add(e));

            _bus.Subscribe<ParryResolved>(e =>
            {
                if (e.Grade == ParryGrade.PerfectParry)
                {
                    _bus.Publish(new CounterEntryOpened(
                        e.Context, e.OccurredAtCombatClockMs,
                        e.ResolutionId, e.AttackSegmentId, e.TimelineSequenceId,
                        e.OccurredAtCombatClockMs + 2000));
                }
            });

            _bus.Publish(new ParryResolved(
                _eventCtx, 800, "res1", "seg1", 1,
                ParryGrade.PerfectParry, 0, 0, EchoIntent.High));

            _bus.DrainQueue();
            Assert.That(counters.Count, Is.EqualTo(1));
            Assert.That(counters[0].ExpiresAtMs, Is.EqualTo(2800));
        }

        [Test]
        public void NormalParry_DoesNotOpenCounter()
        {
            int counterCount = 0;
            _bus.Subscribe<CounterEntryOpened>(e => counterCount++);

            _bus.Subscribe<ParryResolved>(e =>
            {
                if (e.Grade == ParryGrade.PerfectParry)
                {
                    _bus.Publish(new CounterEntryOpened(
                        e.Context, e.OccurredAtCombatClockMs,
                        e.ResolutionId, e.AttackSegmentId, e.TimelineSequenceId,
                        e.OccurredAtCombatClockMs + 2000));
                }
            });

            _bus.Publish(new ParryResolved(
                _eventCtx, 800, "res1", "seg1", 1,
                ParryGrade.NormalParry, 50, 5000, EchoIntent.None));

            _bus.DrainQueue();
            Assert.That(counterCount, Is.EqualTo(0));
        }

        [Test]
        public void CounterClosed_Consumed()
        {
            var closed = new List<CounterEntryClosed>();
            _bus.Subscribe<CounterEntryClosed>(e => closed.Add(e));

            _bus.Publish(new CounterEntryClosed(_eventCtx, 900, "res1", CounterCloseReason.Consumed));

            Assert.That(closed.Count, Is.EqualTo(1));
            Assert.That(closed[0].CloseReason, Is.EqualTo(CounterCloseReason.Consumed));
        }

        [Test]
        public void AllParryEvents_AreValueTypes()
        {
            Assert.That(typeof(ParryResolved).IsValueType, Is.True);
            Assert.That(typeof(ParryCancelled).IsValueType, Is.True);
            Assert.That(typeof(AttackSegmentTimelineRejected).IsValueType, Is.True);
            Assert.That(typeof(CounterEntryOpened).IsValueType, Is.True);
            Assert.That(typeof(CounterEntryClosed).IsValueType, Is.True);
        }

        [Test]
        public void AllParryEvents_CarryContext()
        {
            var resolved = new ParryResolved(_eventCtx, 100, "r1", "s1", 1, ParryGrade.PerfectParry, 0, 0, EchoIntent.High);
            Assert.That(resolved.Context.SceneContextId, Is.EqualTo(_eventCtx.SceneContextId));

            var cancelled = new ParryCancelled(_eventCtx, 200, "s1", 1, "test");
            Assert.That(cancelled.Context.SceneContextId, Is.EqualTo(_eventCtx.SceneContextId));
        }
    }
}
