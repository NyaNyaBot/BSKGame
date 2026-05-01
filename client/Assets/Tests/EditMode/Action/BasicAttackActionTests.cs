using Game.Gameplay.BattleAction;
using Game.Gameplay.Character;
using Game.Gameplay.TurnManager;
using NUnit.Framework;

namespace Game.Tests.EditMode.Action
{
    /// <summary>
    /// ADR-0012 / story-001：BasicAttackAction 授权与 DamageRequest 生成。
    /// </summary>
    public sealed class BasicAttackActionTests
    {
        private InputPhaseStateMachine _sm;
        private CharacterRepository _repo;
        private BattleActionService _svc;
        private CharacterInstance _actor;
        private CharacterInstance _target;

        [SetUp]
        public void SetUp()
        {
            _sm = new InputPhaseStateMachine();
            _repo = new CharacterRepository();
            _svc = new BattleActionService(_sm, _repo);

            var actorDef = new CharacterDefinition("warrior", "战士", 1000, 100, 50, 30);
            var targetDef = new CharacterDefinition("slime", "史莱姆", 300, 0, 10, 5);

            _actor = _repo.CreateInstance(actorDef);
            _target = _repo.CreateInstance(targetDef);

            _sm.TryTransition(BattleInputPhase.BattleStart, out _);
            _sm.TryTransition(BattleInputPhase.PlayerCommand, out _);
        }

        [Test]
        public void Submit_PlayerCommand_ValidActorTarget_Accepted()
        {
            var req = new BattleActionRequest("a1", _actor.InstanceId, _target.InstanceId);
            var result = _svc.Submit(req);

            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Accepted));
            Assert.That(result.GeneratedDamageRequest, Is.Not.Null);
            Assert.That(result.GeneratedDamageRequest!.Value.TargetInstanceId, Is.EqualTo(_target.InstanceId));
        }

        [Test]
        public void Submit_WrongPhase_Rejected()
        {
            _sm.TryTransition(BattleInputPhase.EnemyAction, out _);

            var req = new BattleActionRequest("a1", _actor.InstanceId, _target.InstanceId);
            var result = _svc.Submit(req);

            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("PhaseMismatch"));
        }

        [Test]
        public void Submit_DeadActor_Rejected()
        {
            _actor.MarkDefeated();

            var req = new BattleActionRequest("a1", _actor.InstanceId, _target.InstanceId);
            var result = _svc.Submit(req);

            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("Actor"));
        }

        [Test]
        public void Submit_DeadTarget_Rejected()
        {
            _target.MarkDefeated();

            var req = new BattleActionRequest("a1", _actor.InstanceId, _target.InstanceId);
            var result = _svc.Submit(req);

            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("Target"));
        }

        [Test]
        public void Submit_Idempotent_SameRequestId()
        {
            var req = new BattleActionRequest("a1", _actor.InstanceId, _target.InstanceId);
            var r1 = _svc.Submit(req);
            var r2 = _svc.Submit(req);

            Assert.That(r1.Outcome, Is.EqualTo(r2.Outcome));
            Assert.That(r1.GeneratedDamageRequest!.Value.RequestId,
                Is.EqualTo(r2.GeneratedDamageRequest!.Value.RequestId));
        }

        [Test]
        public void BaseDamage_CalculatesCorrectly()
        {
            var req = new BattleActionRequest("a1", _actor.InstanceId, _target.InstanceId);
            var result = _svc.Submit(req);

            Assert.That(result.GeneratedDamageRequest!.Value.RawDamage, Is.EqualTo(50));
        }

        [Test]
        public void BaseDamage_WithMultiplier_RoundsHalfUp()
        {
            int damage = BattleActionService.RoundHalfUp(21, 5000);
            Assert.That(damage, Is.EqualTo(11));
        }

        [Test]
        public void EmptyRequestId_Rejected()
        {
            var req = new BattleActionRequest("", _actor.InstanceId, _target.InstanceId);
            var result = _svc.Submit(req);

            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
        }
    }
}
