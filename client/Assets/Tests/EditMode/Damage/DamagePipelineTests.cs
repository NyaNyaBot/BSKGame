using Game.Gameplay.Character;
using Game.Gameplay.Damage;
using NUnit.Framework;

namespace Game.Tests.EditMode.Damage
{
    /// <summary>
    /// ADR-0005 / story-001+002：DamagePipeline 幂等、护盾吸收、HP 扣减、死亡判定。
    /// </summary>
    public sealed class DamagePipelineTests
    {
        private CharacterRepository _repo;
        private DamageService _svc;
        private CharacterInstance _target;

        [SetUp]
        public void SetUp()
        {
            _repo = new CharacterRepository();
            _svc = new DamageService(_repo);
            var def = new CharacterDefinition("tank_01", "坦克", 1000, 100, 30, 50);
            _target = _repo.CreateInstance(def);
        }

        [Test]
        public void ApplyDamage_ReducesHp()
        {
            var req = new DamageRequest("r1", _target.InstanceId, 200);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.Outcome, Is.EqualTo(DamageOutcome.Applied));
            Assert.That(result.HpDamage, Is.EqualTo(200));
            Assert.That(result.FinalHp, Is.EqualTo(800));
            Assert.That(_target.CurrentHp, Is.EqualTo(800));
        }

        [Test]
        public void SameRequestId_Idempotent()
        {
            var req = new DamageRequest("r1", _target.InstanceId, 200);
            var r1 = _svc.ApplyDamage(req);
            var r2 = _svc.ApplyDamage(req);

            Assert.That(r1.FinalHp, Is.EqualTo(r2.FinalHp));
            Assert.That(_target.CurrentHp, Is.EqualTo(800), "HP should only decrease once");
        }

        [Test]
        public void NegativeDamage_ClampedToZero()
        {
            var req = new DamageRequest("r1", _target.InstanceId, -50);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.HpDamage, Is.EqualTo(0));
            Assert.That(_target.CurrentHp, Is.EqualTo(1000));
        }

        [Test]
        public void TargetNotFound_Rejected()
        {
            var req = new DamageRequest("r1", "nonexistent", 100);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.Outcome, Is.EqualTo(DamageOutcome.Rejected));
            Assert.That(result.RejectionReason, Does.Contain("not found"));
        }

        [Test]
        public void AlreadyDefeated_Rejected()
        {
            _target.MarkDefeated();

            var req = new DamageRequest("r1", _target.InstanceId, 100);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.Outcome, Is.EqualTo(DamageOutcome.Rejected));
        }

        [Test]
        public void Shield_AbsorbsBeforeHp()
        {
            _target.SetShield(300);

            var req = new DamageRequest("r1", _target.InstanceId, 500);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.ShieldAbsorbed, Is.EqualTo(300));
            Assert.That(result.HpDamage, Is.EqualTo(200));
            Assert.That(result.FinalShield, Is.EqualTo(0));
            Assert.That(result.FinalHp, Is.EqualTo(800));
        }

        [Test]
        public void Shield_FullAbsorption_NoHpDamage()
        {
            _target.SetShield(500);

            var req = new DamageRequest("r1", _target.InstanceId, 200);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.ShieldAbsorbed, Is.EqualTo(200));
            Assert.That(result.HpDamage, Is.EqualTo(0));
            Assert.That(result.FinalShield, Is.EqualTo(300));
            Assert.That(result.FinalHp, Is.EqualTo(1000));
        }

        [Test]
        public void LethalDamage_TriggersDefeat()
        {
            var req = new DamageRequest("r1", _target.InstanceId, 1000);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.IsDefeated, Is.True);
            Assert.That(result.FinalHp, Is.EqualTo(0));
            Assert.That(_target.Status, Is.EqualTo(CharacterStatus.Defeated));
        }

        [Test]
        public void OverKillDamage_HpClampsToZero()
        {
            var req = new DamageRequest("r1", _target.InstanceId, 9999);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.FinalHp, Is.EqualTo(0));
            Assert.That(result.HpDamage, Is.EqualTo(1000));
        }

        [Test]
        public void OnDamageProcessed_FiredForApplied()
        {
            DamageResult? received = null;
            _svc.OnDamageProcessed += r => received = r;

            var req = new DamageRequest("r1", _target.InstanceId, 100);
            _svc.ApplyDamage(req);

            Assert.That(received, Is.Not.Null);
            Assert.That(received!.Value.Outcome, Is.EqualTo(DamageOutcome.Applied));
        }

        [Test]
        public void OnDamageProcessed_FiredForRejected()
        {
            DamageResult? received = null;
            _svc.OnDamageProcessed += r => received = r;

            var req = new DamageRequest("r1", "nonexistent", 100);
            _svc.ApplyDamage(req);

            Assert.That(received, Is.Not.Null);
            Assert.That(received!.Value.Outcome, Is.EqualTo(DamageOutcome.Rejected));
        }

        [Test]
        public void EmptyRequestId_Rejected()
        {
            var req = new DamageRequest("", _target.InstanceId, 100);
            var result = _svc.ApplyDamage(req);

            Assert.That(result.Outcome, Is.EqualTo(DamageOutcome.Rejected));
        }

        [Test]
        public void SnapshotVersion_BumpsAfterDamage()
        {
            var vBefore = _target.SnapshotVersion;
            var req = new DamageRequest("r1", _target.InstanceId, 100);
            _svc.ApplyDamage(req);

            Assert.That(_target.SnapshotVersion, Is.GreaterThan(vBefore));
        }
    }
}
