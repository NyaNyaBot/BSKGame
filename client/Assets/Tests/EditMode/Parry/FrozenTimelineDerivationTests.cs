using Game.Gameplay.Parry;
using NUnit.Framework;

namespace Game.Tests.EditMode.Parry
{
    /// <summary>
    /// ADR-0007 / story-001：FrozenAttackSegmentTimeline 派生与不变量验证。
    /// </summary>
    public sealed class FrozenTimelineDerivationTests
    {
        private ParryWindowProfile _defaultProfile;

        [SetUp]
        public void SetUp()
        {
            _defaultProfile = new ParryWindowProfile(
                perfectHalfWindowMs: 50,
                normalEarlyWindowMs: 100,
                normalLateWindowMs: 100,
                centerBeforeImpactMs: 200);
        }

        [Test]
        public void ValidSeed_Derives_AllInvariantsHold()
        {
            var seed = new AttackSegmentTimelineSeed(
                windupStartMs: 0,
                visualImpactMs: 1000,
                damageCommitMs: 1100,
                recoveryEndMs: 1500);

            var result = TimelineDerivation.Derive(seed, _defaultProfile);

            Assert.That(result.IsValid, Is.True, result.ValidationError);
            var t = result.Timeline;

            Assert.That(t.WindupStartMs, Is.LessThanOrEqualTo(t.WindowOpenMs));
            Assert.That(t.WindowOpenMs, Is.LessThanOrEqualTo(t.PerfectStartMs));
            Assert.That(t.PerfectStartMs, Is.LessThanOrEqualTo(t.ParryCenterMs));
            Assert.That(t.ParryCenterMs, Is.LessThanOrEqualTo(t.PerfectEndMs));
            Assert.That(t.PerfectEndMs, Is.LessThanOrEqualTo(t.NormalEndMs));
            Assert.That(t.NormalEndMs, Is.LessThanOrEqualTo(t.VisualImpactMs));
            Assert.That(t.VisualImpactMs, Is.LessThanOrEqualTo(t.DamageCommitMs));
            Assert.That(t.DamageCommitMs, Is.LessThanOrEqualTo(t.RecoveryEndMs));
        }

        [Test]
        public void NegativeWindupStart_Rejected()
        {
            var seed = new AttackSegmentTimelineSeed(-10, 1000, 1100, 1500);
            var result = TimelineDerivation.Derive(seed, _defaultProfile);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Does.Contain("windupStartMs"));
        }

        [Test]
        public void DamageCommit_LessThan_VisualImpact_Rejected()
        {
            var seed = new AttackSegmentTimelineSeed(0, 1000, 900, 1500);
            var result = TimelineDerivation.Derive(seed, _defaultProfile);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Does.Contain("damageCommitMs"));
        }

        [Test]
        public void WindowOpens_BeforeWindup_Rejected()
        {
            var profile = new ParryWindowProfile(50, 900, 100, 200);
            var seed = new AttackSegmentTimelineSeed(500, 1000, 1100, 1500);
            var result = TimelineDerivation.Derive(seed, profile);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Does.Contain("windowOpenMs"));
        }

        [Test]
        public void NormalEnd_ExceedsImpact_Rejected()
        {
            var profile = new ParryWindowProfile(50, 100, 900, 200);
            var seed = new AttackSegmentTimelineSeed(0, 1000, 1100, 1500);
            var result = TimelineDerivation.Derive(seed, profile);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Does.Contain("normalEndMs"));
        }

        [Test]
        public void Timeline_IsReadonlyStruct()
        {
            var type = typeof(FrozenAttackSegmentTimeline);
            Assert.That(type.IsValueType, Is.True);

            foreach (var prop in type.GetProperties())
            {
                Assert.That(prop.CanWrite, Is.False,
                    $"Property {prop.Name} should be readonly");
            }
        }

        [Test]
        public void BoundaryValues_EqualWindows_Accepted()
        {
            var profile = new ParryWindowProfile(
                perfectHalfWindowMs: 0,
                normalEarlyWindowMs: 0,
                normalLateWindowMs: 0,
                centerBeforeImpactMs: 200);

            var seed = new AttackSegmentTimelineSeed(0, 1000, 1100, 1500);
            var result = TimelineDerivation.Derive(seed, profile);

            Assert.That(result.IsValid, Is.True, result.ValidationError);
            var t = result.Timeline;
            Assert.That(t.WindowOpenMs, Is.EqualTo(t.PerfectStartMs));
            Assert.That(t.PerfectStartMs, Is.EqualTo(t.ParryCenterMs));
        }

        [Test]
        public void DerivedValues_Match_ExpectedCalculation()
        {
            var seed = new AttackSegmentTimelineSeed(0, 1000, 1100, 1500);
            var result = TimelineDerivation.Derive(seed, _defaultProfile);

            Assert.That(result.IsValid, Is.True);
            var t = result.Timeline;

            Assert.That(t.ParryCenterMs, Is.EqualTo(800));
            Assert.That(t.PerfectStartMs, Is.EqualTo(750));
            Assert.That(t.PerfectEndMs, Is.EqualTo(850));
            Assert.That(t.WindowOpenMs, Is.EqualTo(650));
            Assert.That(t.NormalEndMs, Is.EqualTo(950));
        }
    }
}
