using Game.Gameplay.TurnManager;
using NUnit.Framework;

namespace Game.Tests.EditMode.TurnManager
{
    /// <summary>
    /// ADR-0004 / story-001：显式战斗输入阶段状态机。
    /// </summary>
    public sealed class InputPhaseStateMachineTests
    {
        private InputPhaseStateMachine _sm;

        [SetUp]
        public void SetUp()
        {
            _sm = new InputPhaseStateMachine();
        }

        [Test]
        public void InitialPhase_IsNone()
        {
            Assert.That(_sm.CurrentPhase, Is.EqualTo(BattleInputPhase.None));
        }

        [Test]
        public void LegalTransition_None_To_BattleStart()
        {
            var ok = _sm.TryTransition(BattleInputPhase.BattleStart, out var reason);
            Assert.That(ok, Is.True);
            Assert.That(reason, Is.Null);
            Assert.That(_sm.CurrentPhase, Is.EqualTo(BattleInputPhase.BattleStart));
        }

        [Test]
        public void FullLegalSequence_BattleStart_To_BattleEnd()
        {
            Assert.That(_sm.TryTransition(BattleInputPhase.BattleStart, out _), Is.True);
            Assert.That(_sm.TryTransition(BattleInputPhase.PlayerCommand, out _), Is.True);
            Assert.That(_sm.TryTransition(BattleInputPhase.EnemyAction, out _), Is.True);
            Assert.That(_sm.TryTransition(BattleInputPhase.Resolution, out _), Is.True);
            Assert.That(_sm.TryTransition(BattleInputPhase.TurnEnd, out _), Is.True);
            Assert.That(_sm.TryTransition(BattleInputPhase.BattleEnd, out _), Is.True);
            Assert.That(_sm.CurrentPhase, Is.EqualTo(BattleInputPhase.BattleEnd));
        }

        [Test]
        public void IllegalTransition_Rejected_StateUnchanged()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);

            var ok = _sm.TryTransition(BattleInputPhase.Resolution, out var reason);
            Assert.That(ok, Is.False);
            Assert.That(reason, Is.Not.Null.And.Contains("Illegal"));
            Assert.That(_sm.CurrentPhase, Is.EqualTo(BattleInputPhase.BattleStart));
        }

        [Test]
        public void SamePhase_IsNoOp_ReturnsTrue()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);

            int eventCount = 0;
            _sm.PhaseChanged += (_, __) => eventCount++;

            var ok = _sm.TryTransition(BattleInputPhase.BattleStart, out var reason);
            Assert.That(ok, Is.True);
            Assert.That(reason, Is.Null);
            Assert.That(eventCount, Is.EqualTo(0), "Re-entering same phase must not fire event");
        }

        [Test]
        public void PhaseChanged_Event_FiredOnLegalTransition()
        {
            BattleInputPhase? from = null, to = null;
            _sm.PhaseChanged += (f, t) => { from = f; to = t; };

            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            Assert.That(from, Is.EqualTo(BattleInputPhase.None));
            Assert.That(to, Is.EqualTo(BattleInputPhase.BattleStart));
        }

        [Test]
        public void PhaseChanged_NotFired_OnRejection()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);

            int eventCount = 0;
            _sm.PhaseChanged += (_, __) => eventCount++;

            _sm.TryTransition(BattleInputPhase.BattleEnd, out _);
            Assert.That(eventCount, Is.EqualTo(0));
        }

        [Test]
        public void BattleEnd_HasNoLegalTransitions()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);
            _sm.TryTransition(BattleInputPhase.Resolution, out _);
            _sm.TryTransition(BattleInputPhase.TurnEnd, out _);
            _sm.TryTransition(BattleInputPhase.BattleEnd, out _);

            Assert.That(_sm.TryTransition(BattleInputPhase.None, out _), Is.False);
            Assert.That(_sm.TryTransition(BattleInputPhase.BattleStart, out _), Is.False);
        }

        [Test]
        public void TurnEnd_CanLoop_BackToPlayerCommand()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);
            _sm.TryTransition(BattleInputPhase.EnemyAction, out _);
            _sm.TryTransition(BattleInputPhase.Resolution, out _);
            _sm.TryTransition(BattleInputPhase.TurnEnd, out _);

            var ok = _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);
            Assert.That(ok, Is.True, "TurnEnd should allow looping back to PlayerCommand");
        }

        [Test]
        public void PlayerCommand_CanSkipEnemyAction_ToResolution()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);

            var ok = _sm.TryTransition(BattleInputPhase.Resolution, out _);
            Assert.That(ok, Is.True, "PlayerCommand → Resolution should be legal (skip enemy in some turns)");
        }

        [Test]
        public void Reset_ReturnsToNone()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);

            _sm.Reset();
            Assert.That(_sm.CurrentPhase, Is.EqualTo(BattleInputPhase.None));
        }

        [Test]
        public void IsTransitionLegal_WithoutMutating()
        {
            _sm.TryTransition(BattleInputPhase.BattleStart, out _);

            Assert.That(_sm.IsTransitionLegal(BattleInputPhase.PlayerCommand), Is.True);
            Assert.That(_sm.IsTransitionLegal(BattleInputPhase.BattleEnd), Is.False);
            Assert.That(_sm.CurrentPhase, Is.EqualTo(BattleInputPhase.BattleStart), "Must not mutate");
        }
    }
}
