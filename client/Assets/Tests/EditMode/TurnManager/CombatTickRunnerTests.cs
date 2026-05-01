using System.Collections.Generic;
using Game.Gameplay.Combat;
using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;
using Game.Gameplay.TurnManager;
using NUnit.Framework;

namespace Game.Tests.EditMode.TurnManager
{
    /// <summary>
    /// ADR-0004 / story-004：同 tick 事件 drain 与顺序编排。
    /// </summary>
    public sealed class CombatTickRunnerTests
    {
        private CombatClock _clock;
        private BattleEventBus _bus;
        private CombatTickRunner _runner;

        [SetUp]
        public void SetUp()
        {
            var ctx = new SceneContextService();
            ctx.EnterScene(SceneKind.Battle);
            _clock = new CombatClock();
            _bus = new BattleEventBus(ctx);
            _runner = new CombatTickRunner(_clock, _bus);
        }

        [Test]
        public void Tick_AdvancesClock()
        {
            _runner.Tick(100);
            Assert.That(_clock.NowMs, Is.EqualTo(100));
        }

        [Test]
        public void Steps_ExecuteInOrder()
        {
            var order = new List<int>();

            _runner.RegisterStep(0, () => order.Add(0));
            _runner.RegisterStep(2, () => order.Add(2));
            _runner.RegisterStep(5, () => order.Add(5));
            _runner.RegisterStep(9, () => order.Add(9));

            _runner.Tick(50);

            Assert.That(order, Is.EqualTo(new[] { 0, 2, 5, 9 }));
        }

        [Test]
        public void AllTenSteps_CanBeRegistered()
        {
            var order = new List<int>();
            for (int i = 0; i < CombatTickRunner.StepCount; i++)
            {
                int step = i;
                _runner.RegisterStep(i, () => order.Add(step));
            }

            _runner.Tick(10);

            Assert.That(order.Count, Is.EqualTo(10));
            for (int i = 0; i < 10; i++)
            {
                Assert.That(order[i], Is.EqualTo(i));
            }
        }

        [Test]
        public void Tick_DrainsBusQueue_AfterSteps()
        {
            var ctx = new SceneContextService();
            ctx.EnterScene(SceneKind.Battle);
            var battle = ctx.CreateBattleContext(default);
            var bus = new BattleEventBus(ctx);
            var runner = new CombatTickRunner(_clock, bus);

            var received = new List<string>();

            bus.Subscribe<TestTickEvent>(e =>
            {
                received.Add(e.Tag);
                if (e.Tag == "from_step")
                {
                    bus.Publish(new TestTickEvent(e.Context, "nested"));
                }
            });

            runner.RegisterStep(5, () =>
            {
                bus.Publish(new TestTickEvent(battle.ToEventContext(), "from_step"));
            });

            runner.Tick(10);

            Assert.That(received, Is.EqualTo(new[] { "from_step", "nested" }));
        }

        [Test]
        public void StepIndex_OutOfRange_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _runner.RegisterStep(-1, () => { }));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                _runner.RegisterStep(10, () => { }));
        }

        [Test]
        public void PhaseChange_ThenDamage_StrictOrder()
        {
            var order = new List<string>();

            _runner.RegisterStep(2, () => order.Add("input_phase"));
            _runner.RegisterStep(5, () => order.Add("damage"));
            _runner.RegisterStep(6, () => order.Add("death"));
            _runner.RegisterStep(8, () => order.Add("ui_feedback"));

            _runner.Tick(100);

            Assert.That(order, Is.EqualTo(new[] { "input_phase", "damage", "death", "ui_feedback" }));
        }

        #region Test helpers

        private readonly struct TestTickEvent : IBattleEvent
        {
            public string EventId => "test.tick";
            public int SchemaVersion => 1;
            public SceneEventContext Context { get; }
            public long OccurredAtCombatClockMs => 0;
            public string Tag { get; }

            public TestTickEvent(SceneEventContext ctx, string tag)
            {
                Context = ctx;
                Tag = tag;
            }
        }

        #endregion
    }
}
