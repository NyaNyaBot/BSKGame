using Game.Gameplay.Character;
using Game.Gameplay.Damage;
using NUnit.Framework;

namespace Game.Tests.EditMode.Character
{
    /// <summary>
    /// ADR-0011 / story-003：UI 只读投影验证。
    /// ICharacterReadModel 不暴露任何写入 API。
    /// </summary>
    public sealed class CharacterReadModelTests
    {
        private CharacterRepository _repo;
        private DamageService _dmgSvc;
        private ICharacterReadModel _readModel;
        private CharacterInstance _hero;

        [SetUp]
        public void SetUp()
        {
            _repo = new CharacterRepository();
            _dmgSvc = new DamageService(_repo);
            _readModel = new CharacterReadModel(_repo);
            var def = new CharacterDefinition("hero", "英雄", 500, 100, 40, 20);
            _hero = _repo.CreateInstance(def);
        }

        [Test]
        public void ReadModel_ReturnsCurrentSnapshot()
        {
            var snap = _readModel.GetSnapshot(_hero.InstanceId);
            Assert.That(snap.CurrentHp, Is.EqualTo(500));
            Assert.That(snap.Version, Is.EqualTo(1));
        }

        [Test]
        public void ReadModel_ReflectsChanges_AfterDamage()
        {
            var snapBefore = _readModel.GetSnapshot(_hero.InstanceId);

            _dmgSvc.ApplyDamage(new DamageRequest("r1", _hero.InstanceId, 100));

            var snapAfter = _readModel.GetSnapshot(_hero.InstanceId);
            Assert.That(snapAfter.CurrentHp, Is.EqualTo(400));
            Assert.That(snapAfter.Version, Is.GreaterThan(snapBefore.Version));

            Assert.That(snapBefore.CurrentHp, Is.EqualTo(500), "Old snapshot must not change");
        }

        [Test]
        public void GetAllSnapshots_ReturnsAllActive()
        {
            var def2 = new CharacterDefinition("mage", "法师", 300, 500, 60, 10);
            _repo.CreateInstance(def2);

            var all = _readModel.GetAllSnapshots();
            Assert.That(all.Count, Is.EqualTo(2));
        }

        [Test]
        public void TryGetSnapshot_UnknownId_ReturnsFalse()
        {
            var found = _readModel.TryGetSnapshot("nonexistent", out _);
            Assert.That(found, Is.False);
        }

        [Test]
        public void ReadModel_Interface_HasNoWriteMethods()
        {
            var type = typeof(ICharacterReadModel);
            var methods = type.GetMethods();
            foreach (var m in methods)
            {
                Assert.That(m.Name, Does.Not.StartWith("Set"),
                    $"ICharacterReadModel must not expose Set* methods, found: {m.Name}");
                Assert.That(m.Name, Does.Not.StartWith("Apply"),
                    $"ICharacterReadModel must not expose Apply* methods, found: {m.Name}");
                Assert.That(m.Name, Does.Not.StartWith("Mark"),
                    $"ICharacterReadModel must not expose Mark* methods, found: {m.Name}");
            }
        }
    }
}
