using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Scene
{
    /// <summary>
    /// ADR-0001 / story-001：场景与战斗上下文身份与版本。
    /// </summary>
    public sealed class SceneContextIdentityTests
    {
        [Test]
        public void EnterScene_EachEntry_NewContextId_AndIncrementsVersion()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var first = svc.CurrentScene;
            Assert.That(first.SceneContextId, Is.Not.Empty);
            Assert.That(first.SceneVersion, Is.EqualTo(1));

            svc.EnterScene(SceneKind.Battle);
            var second = svc.CurrentScene;
            Assert.That(second.SceneContextId, Is.Not.EqualTo(first.SceneContextId));
            Assert.That(second.SceneVersion, Is.EqualTo(2));
        }

        [Test]
        public void BeginSceneTransition_IncrementsSceneVersion_AndSetsTransitioning()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Menu);
            var before = svc.CurrentScene;
            svc.BeginSceneTransition(SceneTransitionReason.UnloadStarting);
            var mid = svc.CurrentScene;
            Assert.That(mid.SceneVersion, Is.EqualTo(before.SceneVersion + 1));
            Assert.That(mid.IsTransitioning, Is.True);

            svc.CompleteSceneTransition();
            var after = svc.CurrentScene;
            Assert.That(after.IsTransitioning, Is.False);
            Assert.That(after.SceneContextId, Is.EqualTo(mid.SceneContextId));
            Assert.That(after.SceneVersion, Is.EqualTo(mid.SceneVersion));
        }

        [Test]
        public void Battle_Dispose_ThenOldEventContext_IsNotCurrent()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var battle = svc.CreateBattleContext(default);
            var ctx = battle.ToEventContext();
            Assert.That(svc.IsCurrent(ctx), Is.True);

            svc.DisposeBattle(battle.BattleContextId, BattleDisposeReason.SceneUnload);
            Assert.That(svc.IsCurrent(ctx), Is.False);
        }

        [Test]
        public void Battle_Restart_OldContext_IsStale_NewContext_IsCurrent()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var firstBattle = svc.CreateBattleContext(default);
            var firstCtx = firstBattle.ToEventContext();
            Assert.That(svc.IsCurrent(firstCtx), Is.True);

            var secondBattle = svc.CreateBattleContext(default);
            Assert.That(secondBattle.BattleContextId, Is.Not.EqualTo(firstBattle.BattleContextId));
            Assert.That(svc.IsCurrent(firstCtx), Is.False);

            var secondCtx = secondBattle.ToEventContext();
            Assert.That(svc.IsCurrent(secondCtx), Is.True);
        }

        [Test]
        public void SceneOnlyEvent_AfterTransition_StaleBySceneVersion()
        {
            var svc = new SceneContextService();
            svc.EnterScene(SceneKind.Battle);
            var sceneSnap = svc.CurrentScene;
            var sceneOnly = SceneEventContext.ForSceneOnly(in sceneSnap);
            Assert.That(svc.IsCurrent(sceneOnly), Is.True);

            svc.BeginSceneTransition(SceneTransitionReason.Retry);
            svc.CompleteSceneTransition();
            Assert.That(svc.IsCurrent(sceneOnly), Is.False);
        }
    }
}
