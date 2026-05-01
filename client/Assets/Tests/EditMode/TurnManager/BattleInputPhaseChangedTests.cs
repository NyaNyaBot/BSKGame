using System.Collections.Generic;
using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;
using Game.Gameplay.TurnManager;
using NUnit.Framework;

namespace Game.Tests.EditMode.TurnManager
{
    /// <summary>
    /// ADR-0002/ADR-0004 / story-002：BattleInputPhaseChanged 事件发布与总线集成。
    /// </summary>
    public sealed class BattleInputPhaseChangedTests
    {
        private SceneContextService _ctx;
        private BattleEventBus _bus;
        private InputPhaseStateMachine _sm;

        [SetUp]
        public void SetUp()
        {
            _ctx = new SceneContextService();
            _ctx.EnterScene(SceneKind.Battle);
            _bus = new BattleEventBus(_ctx);
            _sm = new InputPhaseStateMachine();
        }

        private SceneEventContext MakeContext()
        {
            var battle = _ctx.CreateBattleContext(default);
            return battle.ToEventContext();
        }

        [Test]
        public void LegalTransition_PublishesEvent_WithCorrectPayload()
        {
            var eventCtx = MakeContext();
            var received = new List<BattleInputPhaseChanged>();
            _bus.Subscribe<BattleInputPhaseChanged>(e => received.Add(e));

            _sm.PhaseChanged += (from, to) =>
            {
                _bus.Publish(new BattleInputPhaseChanged(eventCtx, 0, from, to));
            };

            _sm.TryTransition(BattleInputPhase.BattleStart, out _);

            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].PreviousPhase, Is.EqualTo(BattleInputPhase.None));
            Assert.That(received[0].CurrentPhase, Is.EqualTo(BattleInputPhase.BattleStart));
        }

        [Test]
        public void MultipleTransitions_ProduceOrderedEvents()
        {
            var eventCtx = MakeContext();
            var phases = new List<BattleInputPhase>();
            _bus.Subscribe<BattleInputPhaseChanged>(e => phases.Add(e.CurrentPhase));

            _sm.PhaseChanged += (from, to) =>
            {
                _bus.Publish(new BattleInputPhaseChanged(eventCtx, 0, from, to));
            };

            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);
            _sm.TryTransition(BattleInputPhase.EnemyAction, out _);

            Assert.That(phases, Is.EqualTo(new[]
            {
                BattleInputPhase.BattleStart,
                BattleInputPhase.PlayerCommand,
                BattleInputPhase.EnemyAction
            }));
        }

        [Test]
        public void RejectedTransition_DoesNotPublish()
        {
            var eventCtx = MakeContext();
            int count = 0;
            _bus.Subscribe<BattleInputPhaseChanged>(e => count++);

            _sm.PhaseChanged += (from, to) =>
            {
                _bus.Publish(new BattleInputPhaseChanged(eventCtx, 0, from, to));
            };

            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            count = 0;

            _sm.TryTransition(BattleInputPhase.BattleEnd, out _);
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void Event_IsImmutableDTO()
        {
            var evt = new BattleInputPhaseChanged(
                default, 100,
                BattleInputPhase.PlayerCommand,
                BattleInputPhase.EnemyAction);

            Assert.That(evt.EventId, Is.EqualTo("battle.input_phase_changed"));
            Assert.That(evt.SchemaVersion, Is.EqualTo(1));
            Assert.That(evt.OccurredAtCombatClockMs, Is.EqualTo(100));
        }

        [Test]
        public void QueuedPublish_DrainsCorrectly()
        {
            var eventCtx = MakeContext();
            var received = new List<BattleInputPhaseChanged>();

            _bus.Subscribe<BattleInputPhaseChanged>(e =>
            {
                received.Add(e);
                if (e.CurrentPhase == BattleInputPhase.BattleStart)
                {
                    _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);
                }
            });

            _sm.PhaseChanged += (from, to) =>
            {
                _bus.Publish(new BattleInputPhaseChanged(eventCtx, 0, from, to));
            };

            _sm.TryTransition(BattleInputPhase.BattleStart, out _);

            Assert.That(received.Count, Is.EqualTo(1), "Nested publish should queue");
            Assert.That(_bus.PendingCount, Is.EqualTo(1));

            _bus.DrainQueue();
            Assert.That(received.Count, Is.EqualTo(2));
            Assert.That(received[1].CurrentPhase, Is.EqualTo(BattleInputPhase.PlayerCommand));
        }
    }
}
