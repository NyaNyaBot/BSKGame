using Game.Gameplay.Character;
using NUnit.Framework;

namespace Game.Tests.EditMode.Character
{
    /// <summary>
    /// ADR-0011 / story-001：权威角色 Schema、实例、InstanceId。
    /// </summary>
    public sealed class CharacterSchemaTests
    {
        private CharacterRepository _repo;
        private CharacterDefinition _warrior;

        [SetUp]
        public void SetUp()
        {
            _repo = new CharacterRepository();
            _warrior = new CharacterDefinition("warrior_01", "战士", 1000, 200, 50, 30);
        }

        [Test]
        public void CreateInstance_AssignsUniqueInstanceId()
        {
            var a = _repo.CreateInstance(_warrior);
            var b = _repo.CreateInstance(_warrior);

            Assert.That(a.InstanceId, Is.Not.EqualTo(b.InstanceId));
            Assert.That(a.Definition.CharacterId, Is.EqualTo(b.Definition.CharacterId));
        }

        [Test]
        public void CreateInstance_InitializesFullHp()
        {
            var inst = _repo.CreateInstance(_warrior);

            Assert.That(inst.CurrentHp, Is.EqualTo(1000));
            Assert.That(inst.MaxHp, Is.EqualTo(1000));
            Assert.That(inst.CurrentMp, Is.EqualTo(200));
            Assert.That(inst.Status, Is.EqualTo(CharacterStatus.Active));
        }

        [Test]
        public void GetInstance_ByInstanceId_ReturnsCorrect()
        {
            var inst = _repo.CreateInstance(_warrior);
            var found = _repo.GetInstance(inst.InstanceId);

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.InstanceId, Is.EqualTo(inst.InstanceId));
        }

        [Test]
        public void GetInstance_UnknownId_ReturnsNull()
        {
            var found = _repo.GetInstance("nonexistent");
            Assert.That(found, Is.Null);
        }

        [Test]
        public void GetAllInstances_ReturnsAll()
        {
            _repo.CreateInstance(_warrior);
            _repo.CreateInstance(_warrior);

            Assert.That(_repo.GetAllInstances().Count, Is.EqualTo(2));
        }

        [Test]
        public void RemoveInstance_MarksRemoved_AndRemovesFromLookup()
        {
            var inst = _repo.CreateInstance(_warrior);
            _repo.RemoveInstance(inst.InstanceId);

            Assert.That(inst.Status, Is.EqualTo(CharacterStatus.Removed));
            Assert.That(_repo.GetInstance(inst.InstanceId), Is.Null);
        }

        [Test]
        public void Defeated_AndRemoved_AreDifferentStatuses()
        {
            var inst = _repo.CreateInstance(_warrior);

            Assert.That(inst.Status, Is.EqualTo(CharacterStatus.Active));
            Assert.That(CharacterStatus.Defeated, Is.Not.EqualTo(CharacterStatus.Removed));
        }

        [Test]
        public void Snapshot_ReflectsCurrentState()
        {
            var inst = _repo.CreateInstance(_warrior);
            var snap = inst.TakeSnapshot();

            Assert.That(snap.InstanceId, Is.EqualTo(inst.InstanceId));
            Assert.That(snap.CharacterId, Is.EqualTo("warrior_01"));
            Assert.That(snap.CurrentHp, Is.EqualTo(1000));
            Assert.That(snap.Version, Is.EqualTo(1));
        }

        [Test]
        public void Clear_RemovesAllInstances()
        {
            _repo.CreateInstance(_warrior);
            _repo.CreateInstance(_warrior);
            _repo.Clear();

            Assert.That(_repo.GetAllInstances().Count, Is.EqualTo(0));
        }
    }
}
