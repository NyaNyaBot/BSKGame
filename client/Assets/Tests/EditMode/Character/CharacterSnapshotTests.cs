using Game.Gameplay.Character;
using NUnit.Framework;

namespace Game.Tests.EditMode.Character
{
    /// <summary>
    /// ADR-0011 / story-002：CharacterBattleSnapshot 不可变与版本递增。
    /// </summary>
    public sealed class CharacterSnapshotTests
    {
        private CharacterRepository _repo;
        private CharacterDefinition _def;
        private CharacterInstance _inst;

        [SetUp]
        public void SetUp()
        {
            _repo = new CharacterRepository();
            _def = new CharacterDefinition("mage_01", "法师", 800, 500, 40, 20);
            _inst = _repo.CreateInstance(_def);
        }

        [Test]
        public void Snapshot_IsReadonly_AfterCreation()
        {
            var snap = _inst.TakeSnapshot();
            Assert.That(snap.CurrentHp, Is.EqualTo(800));
            Assert.That(snap.Version, Is.EqualTo(1));
        }

        [Test]
        public void SetHp_IncrementsVersion_OldSnapshotUnchanged()
        {
            var snap1 = _inst.TakeSnapshot();
            Assert.That(snap1.Version, Is.EqualTo(1));

            _inst.SetHp(600);

            var snap2 = _inst.TakeSnapshot();
            Assert.That(snap2.Version, Is.EqualTo(2));
            Assert.That(snap2.CurrentHp, Is.EqualTo(600));

            Assert.That(snap1.CurrentHp, Is.EqualTo(800), "Old snapshot must not change");
            Assert.That(snap1.Version, Is.EqualTo(1));
        }

        [Test]
        public void SetShield_IncrementsVersion()
        {
            _inst.SetShield(100);
            var snap = _inst.TakeSnapshot();
            Assert.That(snap.Version, Is.EqualTo(2));
            Assert.That(snap.Shield, Is.EqualTo(100));
        }

        [Test]
        public void SetHp_ClampsToZeroAndMax()
        {
            _inst.SetHp(-50);
            Assert.That(_inst.CurrentHp, Is.EqualTo(0));

            _inst.SetHp(9999);
            Assert.That(_inst.CurrentHp, Is.EqualTo(800));
        }

        [Test]
        public void MarkDefeated_ChangesStatus_IncrementsVersion()
        {
            var v1 = _inst.SnapshotVersion;
            _inst.MarkDefeated();

            Assert.That(_inst.Status, Is.EqualTo(CharacterStatus.Defeated));
            Assert.That(_inst.SnapshotVersion, Is.GreaterThan(v1));
        }

        [Test]
        public void MarkDefeated_Idempotent_AlreadyDefeated()
        {
            _inst.MarkDefeated();
            var v = _inst.SnapshotVersion;

            _inst.MarkDefeated();
            Assert.That(_inst.SnapshotVersion, Is.EqualTo(v), "Double defeat should not bump version");
        }

        [Test]
        public void GetSnapshot_FromRepository_MatchesInstance()
        {
            _inst.SetHp(500);
            var repoSnap = _repo.GetSnapshot(_inst.InstanceId);
            var instSnap = _inst.TakeSnapshot();

            Assert.That(repoSnap.CurrentHp, Is.EqualTo(instSnap.CurrentHp));
            Assert.That(repoSnap.Version, Is.EqualTo(instSnap.Version));
        }

        [Test]
        public void Version_MonotonicallyIncreases()
        {
            var versions = new int[5];
            versions[0] = _inst.SnapshotVersion;

            _inst.SetHp(700);
            versions[1] = _inst.SnapshotVersion;

            _inst.SetShield(50);
            versions[2] = _inst.SnapshotVersion;

            _inst.SetMp(100);
            versions[3] = _inst.SnapshotVersion;

            _inst.MarkDefeated();
            versions[4] = _inst.SnapshotVersion;

            for (int i = 1; i < versions.Length; i++)
            {
                Assert.That(versions[i], Is.GreaterThan(versions[i - 1]),
                    $"Version at step {i} should be greater than step {i - 1}");
            }
        }
    }
}
