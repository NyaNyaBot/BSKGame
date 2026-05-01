using System.Collections.Generic;
using Game.Gameplay.Character;
using Game.Gameplay.EnemyAI;
using NUnit.Framework;

namespace Game.Tests.EditMode.EnemyAI
{
    /// <summary>
    /// ADR-0008 / story-001：攻击模式选择器，确定性 + 冷却 + HP 阶段。
    /// </summary>
    public sealed class PatternSelectorTests
    {
        private CharacterRepository _repo;
        private EnemyAttackPlanner _planner;
        private CharacterInstance _enemy;
        private CharacterInstance _player;

        [SetUp]
        public void SetUp()
        {
            _repo = new CharacterRepository();
            _planner = new EnemyAttackPlanner(_repo);

            var enemyDef = new CharacterDefinition("boss", "Boss", 1000, 0, 50, 30);
            var playerDef = new CharacterDefinition("hero", "英雄", 500, 100, 40, 20);

            _enemy = _repo.CreateInstance(enemyDef);
            _player = _repo.CreateInstance(playerDef);
        }

        private EnemyActionQuery MakeQuery(int turn = 1)
        {
            return new EnemyActionQuery(_enemy.InstanceId, _player.InstanceId, turn);
        }

        [Test]
        public void SelectAction_ValidTarget_ReturnsActionPlan()
        {
            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("slash", 10, 0, HpPhaseRequirement.Any, 30),
            };

            var plan = _planner.SelectAction(MakeQuery(), patterns, 42);
            Assert.That(plan, Is.Not.Null);
            Assert.That(plan!.Value.PatternId, Is.EqualTo("slash"));
        }

        [Test]
        public void CooldownPattern_ScoreIsZero()
        {
            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("slam", 10, 2, HpPhaseRequirement.Any, 50),
                new AttackPatternDefinition("slash", 10, 0, HpPhaseRequirement.Any, 30),
            };

            _planner.ApplyCooldown("slam", 2);

            var plan = _planner.SelectAction(MakeQuery(), patterns, 42);
            Assert.That(plan, Is.Not.Null);
            Assert.That(plan!.Value.PatternId, Is.EqualTo("slash"));
        }

        [Test]
        public void HpPhase_AboveHalf_FiltersBelowHalfPattern()
        {
            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("enrage", 10, 0, HpPhaseRequirement.BelowHalf, 80),
                new AttackPatternDefinition("slash", 10, 0, HpPhaseRequirement.Any, 30),
            };

            var plan = _planner.SelectAction(MakeQuery(), patterns, 42);
            Assert.That(plan, Is.Not.Null);
            Assert.That(plan!.Value.PatternId, Is.EqualTo("slash"),
                "Boss HP=1000/1000 (above half), enrage should be filtered");
        }

        [Test]
        public void HpPhase_BelowHalf_AllowsBelowHalfPattern()
        {
            _enemy.SetHp(400);

            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("enrage", 100, 0, HpPhaseRequirement.BelowHalf, 80),
                new AttackPatternDefinition("slash", 1, 0, HpPhaseRequirement.Any, 30),
            };

            var plan = _planner.SelectAction(MakeQuery(), patterns, 42);
            Assert.That(plan, Is.Not.Null);
            Assert.That(plan!.Value.PatternId, Is.EqualTo("enrage"),
                "Boss HP=400/1000 (below half), enrage should dominate by weight");
        }

        [Test]
        public void NoValidTarget_ReturnsNull()
        {
            _player.MarkDefeated();

            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("slash", 10, 0, HpPhaseRequirement.Any, 30),
            };

            var plan = _planner.SelectAction(MakeQuery(), patterns, 42);
            Assert.That(plan, Is.Null);
        }

        [Test]
        public void Deterministic_FixedSeed_100Runs_SameResults()
        {
            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("slash", 5, 0, HpPhaseRequirement.Any, 30),
                new AttackPatternDefinition("thrust", 3, 0, HpPhaseRequirement.Any, 40),
                new AttackPatternDefinition("sweep", 2, 0, HpPhaseRequirement.Any, 25),
            };

            var results = new string[100];
            for (int i = 0; i < 100; i++)
            {
                var plan = _planner.SelectAction(MakeQuery(turn: 5), patterns, 42);
                results[i] = plan!.Value.PatternId;
            }

            for (int i = 1; i < 100; i++)
            {
                Assert.That(results[i], Is.EqualTo(results[0]),
                    $"Run {i} differed from run 0: {results[i]} vs {results[0]}");
            }
        }

        [Test]
        public void AllPatternsCooled_ReturnsNull()
        {
            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("slash", 10, 2, HpPhaseRequirement.Any, 30),
            };

            _planner.ApplyCooldown("slash", 2);

            var plan = _planner.SelectAction(MakeQuery(), patterns, 42);
            Assert.That(plan, Is.Null);
        }

        [Test]
        public void TickCooldowns_ReducesByOne()
        {
            var patterns = new List<AttackPatternDefinition>
            {
                new AttackPatternDefinition("slam", 10, 2, HpPhaseRequirement.Any, 50),
            };

            _planner.ApplyCooldown("slam", 2);
            Assert.That(_planner.SelectAction(MakeQuery(), patterns, 42), Is.Null);

            _planner.TickCooldowns();
            Assert.That(_planner.SelectAction(MakeQuery(), patterns, 42), Is.Null);

            _planner.TickCooldowns();
            Assert.That(_planner.SelectAction(MakeQuery(), patterns, 42), Is.Not.Null);
        }
    }
}
