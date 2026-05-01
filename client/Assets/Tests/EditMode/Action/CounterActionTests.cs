using Game.Gameplay.BattleAction;
using Game.Gameplay.Character;
using Game.Gameplay.Combat;
using NUnit.Framework;

namespace Game.Tests.EditMode.Action
{
    /// <summary>
    /// ADR-0012 / story-002：CounterAction 授权与 counter 消耗。
    /// </summary>
    public sealed class CounterActionTests
    {
        private CharacterRepository _repo;
        private CombatClock _clock;
        private CounterActionService _svc;
        private CharacterInstance _player;
        private CharacterInstance _enemy;

        [SetUp]
        public void SetUp()
        {
            _repo = new CharacterRepository();
            _clock = new CombatClock { MaxStepMs = 10000 };
            _svc = new CounterActionService(_repo, _clock);

            var playerDef = new CharacterDefinition("hero", "英雄", 500, 100, 40, 20);
            var enemyDef = new CharacterDefinition("boss", "Boss", 1000, 0, 50, 30);
            _player = _repo.CreateInstance(playerDef);
            _enemy = _repo.CreateInstance(enemyDef);
        }

        [Test]
        public void SubmitCounter_ValidEntry_Accepted()
        {
            _clock.Advance(800);
            _svc.RegisterCounter("res1", _player.InstanceId, _enemy.InstanceId, 2800);

            var result = _svc.SubmitCounter("act1", "res1");
            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Accepted));
            Assert.That(result.GeneratedDamageRequest, Is.Not.Null);

            int expectedDmg = BattleActionService.RoundHalfUp(40, 15000);
            Assert.That(result.GeneratedDamageRequest!.Value.RawDamage, Is.EqualTo(expectedDmg));
        }

        [Test]
        public void SubmitCounter_Expired_Rejected()
        {
            _clock.Advance(3000);
            _svc.RegisterCounter("res1", _player.InstanceId, _enemy.InstanceId, 2800);

            var result = _svc.SubmitCounter("act1", "res1");
            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("Expired"));
        }

        [Test]
        public void SubmitCounter_AlreadyConsumed_Rejected()
        {
            _clock.Advance(800);
            _svc.RegisterCounter("res1", _player.InstanceId, _enemy.InstanceId, 2800);

            _svc.SubmitCounter("act1", "res1");
            var result = _svc.SubmitCounter("act2", "res1");

            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("AlreadyConsumed"));
        }

        [Test]
        public void SubmitCounter_NotFound_Rejected()
        {
            var result = _svc.SubmitCounter("act1", "nonexistent");
            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("NotFound"));
        }

        [Test]
        public void SubmitCounter_TargetDead_Rejected()
        {
            _clock.Advance(800);
            _svc.RegisterCounter("res1", _player.InstanceId, _enemy.InstanceId, 2800);
            _enemy.MarkDefeated();

            var result = _svc.SubmitCounter("act1", "res1");
            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("TargetInvalid"));
        }

        [Test]
        public void SubmitCounter_Idempotent()
        {
            _clock.Advance(800);
            _svc.RegisterCounter("res1", _player.InstanceId, _enemy.InstanceId, 2800);

            var r1 = _svc.SubmitCounter("act1", "res1");
            var r2 = _svc.SubmitCounter("act1", "res1");

            Assert.That(r1.ActionRequestId, Is.EqualTo(r2.ActionRequestId));
            Assert.That(r1.Outcome, Is.EqualTo(r2.Outcome));
        }

        [Test]
        public void OnCounterConsumed_Fired()
        {
            _clock.Advance(800);
            _svc.RegisterCounter("res1", _player.InstanceId, _enemy.InstanceId, 2800);

            string? consumed = null;
            _svc.OnCounterConsumed += id => consumed = id;

            _svc.SubmitCounter("act1", "res1");
            Assert.That(consumed, Is.EqualTo("res1"));
        }

        [Test]
        public void IsCounterActive_FalseAfterConsume()
        {
            _clock.Advance(800);
            _svc.RegisterCounter("res1", _player.InstanceId, _enemy.InstanceId, 2800);
            Assert.That(_svc.IsCounterActive("res1"), Is.True);

            _svc.SubmitCounter("act1", "res1");
            Assert.That(_svc.IsCounterActive("res1"), Is.False);
        }
    }
}
