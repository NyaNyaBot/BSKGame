using System;
using Game.Gameplay.Input;
using NUnit.Framework;

namespace Game.Tests.EditMode.Input
{
    /// <summary>
    /// ADR-0006 / story-003：InputHitAreaRegistry 优先级、遮挡与去重。
    /// </summary>
    public sealed class InputHitAreaRegistryTests
    {
        [Test]
        public void Register_AndHitTest_ReturnsHighestPriority()
        {
            var registry = new InputHitAreaRegistry();
            registry.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry));
            registry.Register(new HitAreaRegistration("counter", InputHitAreaRegistry.PriorityCounter));
            registry.Register(new HitAreaRegistration("bg", InputHitAreaRegistry.PriorityBackground));

            var result = registry.HitTest();
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.AreaId, Is.EqualTo("counter"));
            Assert.That(result.Value.Priority, Is.EqualTo(300));
        }

        [Test]
        public void Unregister_RemovesArea_FallsToNext()
        {
            var registry = new InputHitAreaRegistry();
            registry.Register(new HitAreaRegistration("counter", InputHitAreaRegistry.PriorityCounter));
            registry.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry));

            registry.Unregister("counter");

            var result = registry.HitTest();
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.AreaId, Is.EqualTo("parry"));
        }

        [Test]
        public void SetEnabled_False_SkipsArea()
        {
            var registry = new InputHitAreaRegistry();
            registry.Register(new HitAreaRegistration("counter", InputHitAreaRegistry.PriorityCounter));
            registry.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry));

            registry.SetEnabled("counter", false);

            var result = registry.HitTest();
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.AreaId, Is.EqualTo("parry"));
        }

        [Test]
        public void HitTest_WithStateVersion_SkipsMismatch()
        {
            var registry = new InputHitAreaRegistry();
            registry.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry));

            var result = registry.HitTest(expectedStateVersion: 1);
            Assert.That(result, Is.Not.Null);

            registry.UpdateStateVersion("parry", 2);
            result = registry.HitTest(expectedStateVersion: 1);
            Assert.That(result, Is.Null, "Stale stateVersion should not match");
        }

        [Test]
        public void HitTest_EmptyRegistry_ReturnsNull()
        {
            var registry = new InputHitAreaRegistry();
            Assert.That(registry.HitTest(), Is.Null);
        }

        [Test]
        public void Register_DuplicateAreaId_Throws()
        {
            var registry = new InputHitAreaRegistry();
            registry.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry));
            Assert.Throws<InvalidOperationException>(() =>
                registry.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry)));
        }

        [Test]
        public void CounterEntryOpened_CounterHigherThanParry()
        {
            var registry = new InputHitAreaRegistry();
            registry.Register(new HitAreaRegistration("parry", InputHitAreaRegistry.PriorityParry));
            registry.Register(new HitAreaRegistration("counter", InputHitAreaRegistry.PriorityCounter));

            var result = registry.HitTest();
            Assert.That(result!.Value.AreaId, Is.EqualTo("counter"),
                "Counter (300) should be higher priority than Parry (100)");
        }

        [Test]
        public void Clear_RemovesAllAreas()
        {
            var registry = new InputHitAreaRegistry();
            registry.Register(new HitAreaRegistration("a", 100));
            registry.Register(new HitAreaRegistration("b", 200));
            Assert.That(registry.RegisteredCount, Is.EqualTo(2));

            registry.Clear();
            Assert.That(registry.RegisteredCount, Is.EqualTo(0));
            Assert.That(registry.HitTest(), Is.Null);
        }

        [Test]
        public void PriorityConstants_MatchAdr0006()
        {
            Assert.That(InputHitAreaRegistry.PriorityCounter, Is.EqualTo(300));
            Assert.That(InputHitAreaRegistry.PriorityModalPause, Is.EqualTo(250));
            Assert.That(InputHitAreaRegistry.PriorityStandardUIAction, Is.EqualTo(200));
            Assert.That(InputHitAreaRegistry.PriorityParry, Is.EqualTo(100));
            Assert.That(InputHitAreaRegistry.PriorityBackground, Is.EqualTo(0));
        }
    }
}
