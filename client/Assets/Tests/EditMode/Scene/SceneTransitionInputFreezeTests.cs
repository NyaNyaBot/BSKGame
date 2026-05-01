using System.Collections.Generic;
using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Scene
{
    /// <summary>
    /// ADR-0001 + ADR-0002 / scene-lifecycle story-002：
    /// 场景切换期间输入冻结与迟到事件拒绝。
    /// </summary>
    public sealed class SceneTransitionInputFreezeTests
    {
        private readonly struct FakeInputEvent : IBattleEvent
        {
            public string EventId { get; }
            public int SchemaVersion { get; }
            public SceneEventContext Context { get; }
            public long OccurredAtCombatClockMs { get; }

            public FakeInputEvent(string eventId, SceneEventContext ctx, long clockMs)
            {
                EventId = eventId;
                SchemaVersion = 1;
                Context = ctx;
                OccurredAtCombatClockMs = clockMs;
            }
        }

        [Test]
        public void IsInputFrozen_TrueDuring_SceneTransition()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            Assert.That(svc.IsInputFrozen, Is.False);

            svc.BeginSceneTransition(SceneTransitionReason.UnloadStarting);
            Assert.That(svc.IsInputFrozen, Is.True);

            svc.CompleteSceneTransition();
            Assert.That(svc.IsInputFrozen, Is.False);
        }

        [Test]
        public void StaleEvent_AfterTransition_RejectedByBus()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);

            var preTransitionCtx = battle.ToEventContext();

            svc.BeginSceneTransition(SceneTransitionReason.UnloadStarting);

            var bus = new BattleEventBus(svc);
            var received = new List<string>();
            bus.Subscribe<FakeInputEvent>(e => received.Add(e.EventId),
                new BattleEventSubscriptionOptions(dropStaleContext: true));

            var staleEvt = new FakeInputEvent("stale-input", preTransitionCtx, 500L);
            bus.Publish(staleEvt);

            Assert.That(received, Is.Empty,
                "Events with pre-transition context should be rejected during transition");
        }

        [Test]
        public void FreshEvent_AfterNewScene_AcceptedByBus()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            svc.BeginSceneTransition(SceneTransitionReason.UnloadStarting);
            svc.CompleteSceneTransition();
            svc.EnterScene(SceneKind.Battle);
            var newBattle = svc.CreateBattleContext(default);

            var bus = new BattleEventBus(svc);
            var received = new List<string>();
            bus.Subscribe<FakeInputEvent>(e => received.Add(e.EventId),
                new BattleEventSubscriptionOptions(dropStaleContext: true));

            var freshEvt = new FakeInputEvent("fresh-input", newBattle.ToEventContext(), 100L);
            bus.Publish(freshEvt);

            Assert.That(received, Has.Count.EqualTo(1));
            Assert.That(received[0], Is.EqualTo("fresh-input"));
        }

        [Test]
        public void InputFreeze_PreventsNewInput_WhileTransitioning()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            svc.CreateBattleContext(default);

            Assert.That(svc.IsInputFrozen, Is.False, "Input should be active before transition");

            svc.BeginSceneTransition(SceneTransitionReason.Retry);

            Assert.That(svc.IsInputFrozen, Is.True, "Input should be frozen during transition");
            Assert.That(svc.CurrentScene.IsTransitioning, Is.True, "Scene should be transitioning");
        }

        [Test]
        public void StaleEvent_SameSceneId_OldVersion_RejectedByBus()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);

            var oldScene = svc.CurrentScene;
            var oldCtx = SceneEventContext.ForSceneOnly(in oldScene);

            svc.BeginSceneTransition(SceneTransitionReason.Retry);
            svc.CompleteSceneTransition();

            var bus = new BattleEventBus(svc);
            var received = new List<string>();
            bus.Subscribe<FakeInputEvent>(e => received.Add(e.EventId),
                new BattleEventSubscriptionOptions(dropStaleContext: true));

            var staleEvt = new FakeInputEvent("old-version", oldCtx, 200L);
            bus.Publish(staleEvt);

            Assert.That(received, Is.Empty,
                "Event with old sceneVersion should be rejected even if sceneContextId matches");
        }
    }
}
