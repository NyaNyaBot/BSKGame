using System.Collections.Generic;
using Game.Gameplay.EnemyAI;
using Game.Gameplay.Parry;
using NUnit.Framework;

namespace Game.Tests.EditMode.EnemyAI
{
    /// <summary>
    /// ADR-0008 / story-002：AttackSegment 内容事实与 seed 提交。
    /// </summary>
    public sealed class SegmentContentTests
    {
        [Test]
        public void CreateSeed_AbsolutizesRelativeDelays()
        {
            var segment = new AttackSegmentDefinition(
                "seg1", 0, 50, "profile_a",
                relativeWindupDelayMs: 0,
                relativeImpactDelayMs: 500,
                relativeDamageCommitDelayMs: 600,
                relativeRecoveryEndDelayMs: 800);

            var seed = AttackSegmentFactory.CreateSeed(segment, segmentStartMs: 1000);

            Assert.That(seed.WindupStartMs, Is.EqualTo(1000));
            Assert.That(seed.VisualImpactMs, Is.EqualTo(1500));
            Assert.That(seed.DamageCommitMs, Is.EqualTo(1600));
            Assert.That(seed.RecoveryEndMs, Is.EqualTo(1800));
        }

        [Test]
        public void CreateComboSeeds_ChainsSegments()
        {
            var segments = new List<AttackSegmentDefinition>
            {
                new AttackSegmentDefinition("seg0", 0, 30, "p1", 0, 400, 500, 700),
                new AttackSegmentDefinition("seg1", 1, 40, "p2", 0, 300, 400, 600),
            };

            var seeds = AttackSegmentFactory.CreateComboSeeds(segments, firstSegmentStartMs: 0);

            Assert.That(seeds.Count, Is.EqualTo(2));

            Assert.That(seeds[0].WindupStartMs, Is.EqualTo(0));
            Assert.That(seeds[0].RecoveryEndMs, Is.EqualTo(700));

            Assert.That(seeds[1].WindupStartMs, Is.EqualTo(700));
            Assert.That(seeds[1].VisualImpactMs, Is.EqualTo(1000));
            Assert.That(seeds[1].RecoveryEndMs, Is.EqualTo(1300));
        }

        [Test]
        public void ComboIndex_Increments()
        {
            var segments = new List<AttackSegmentDefinition>
            {
                new AttackSegmentDefinition("s0", 0, 30, "p1", 0, 400, 500, 700),
                new AttackSegmentDefinition("s1", 1, 40, "p2", 0, 300, 400, 600),
                new AttackSegmentDefinition("s2", 2, 50, "p3", 0, 200, 300, 500),
            };

            for (int i = 0; i < segments.Count; i++)
            {
                Assert.That(segments[i].ComboIndex, Is.EqualTo(i));
            }
        }

        [Test]
        public void Seed_ContainsNoPrimitiveUnityTypes()
        {
            var seedType = typeof(AttackSegmentTimelineSeed);
            foreach (var prop in seedType.GetProperties())
            {
                Assert.That(prop.PropertyType.Namespace, Is.Not.EqualTo("UnityEngine"),
                    $"Seed property {prop.Name} must not reference UnityEngine types");
            }
        }

        [Test]
        public void SegmentDefinition_ContainsNoUnityTypes()
        {
            var defType = typeof(AttackSegmentDefinition);
            foreach (var prop in defType.GetProperties())
            {
                Assert.That(prop.PropertyType.Namespace, Is.Not.EqualTo("UnityEngine"),
                    $"Definition property {prop.Name} must not reference UnityEngine types");
            }
        }

        [Test]
        public void SingleSegment_ValidSeedForParryDerivation()
        {
            var segment = new AttackSegmentDefinition(
                "seg1", 0, 50, "profile_a", 0, 1000, 1100, 1500);

            var seed = AttackSegmentFactory.CreateSeed(segment, 0);
            var profile = new ParryWindowProfile(50, 100, 100, 200);
            var result = TimelineDerivation.Derive(seed, profile);

            Assert.That(result.IsValid, Is.True, result.ValidationError);
        }
    }
}
