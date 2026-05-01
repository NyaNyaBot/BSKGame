using Game.Gameplay.Feedback;
using Game.Gameplay.Parry;
using NUnit.Framework;

namespace Game.Tests.EditMode.Feedback
{
    /// <summary>
    /// ADR-0009 / story-001+002+003：反馈 profile 查表、HitStop、质量降级。
    /// </summary>
    public sealed class FeedbackProfileTests
    {
        private FeedbackProfileLookup _lookup;

        [SetUp]
        public void SetUp()
        {
            _lookup = new FeedbackProfileLookup();
        }

        [Test]
        public void PerfectParry_CriticalIntensity()
        {
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "target1");
            Assert.That(bundle, Is.Not.Null);
            Assert.That(bundle!.Value.Feedback.Kind, Is.EqualTo(FeedbackKind.PerfectParry));
            Assert.That(bundle.Value.Feedback.Intensity, Is.EqualTo(FeedbackIntensity.Critical));
        }

        [Test]
        public void PerfectParry_HitStopRequest_90ms()
        {
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "target1");
            Assert.That(bundle!.Value.HitStop, Is.Not.Null);
            Assert.That(bundle.Value.HitStop!.Value.DurationMs, Is.EqualTo(90));
            Assert.That(bundle.Value.HitStop.Value.Priority, Is.EqualTo(FeedbackIntensity.Critical));
        }

        [Test]
        public void NormalParry_HighIntensity_HitStop35ms()
        {
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.NormalParry, "target1");
            Assert.That(bundle!.Value.Feedback.Kind, Is.EqualTo(FeedbackKind.NormalParry));
            Assert.That(bundle.Value.Feedback.Intensity, Is.EqualTo(FeedbackIntensity.High));
            Assert.That(bundle.Value.HitStop!.Value.DurationMs, Is.EqualTo(35));
        }

        [Test]
        public void FailedParry_MediumIntensity_NoHitStop()
        {
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.FailedParry, "target1");
            Assert.That(bundle!.Value.Feedback.Kind, Is.EqualTo(FeedbackKind.FailedParry));
            Assert.That(bundle.Value.HitStop, Is.Null);
        }

        [Test]
        public void Idempotent_SameResolutionId_ReturnsNull()
        {
            _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "target1");
            var second = _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "target1");
            Assert.That(second, Is.Null, "Second call should be idempotent");
        }

        [Test]
        public void LowQualityTier_PerfectHitStop_Zero()
        {
            _lookup.SetQualityTier(QualityTier.Low);
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "target1");
            Assert.That(bundle!.Value.HitStop, Is.Null.Or.Property("DurationMs").EqualTo(0));
        }

        [Test]
        public void MediumQualityTier_NormalHitStop_Zero()
        {
            _lookup.SetQualityTier(QualityTier.Medium);
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.NormalParry, "target1");
            Assert.That(bundle!.Value.HitStop, Is.Null);
        }

        [Test]
        public void DamageReceived_Idempotent()
        {
            var first = _lookup.BuildDamageReceived("r1", "t1");
            Assert.That(first, Is.Not.Null);

            var second = _lookup.BuildDamageReceived("r1", "t1");
            Assert.That(second, Is.Null);
        }

        [Test]
        public void CharacterDefeated_CriticalIntensity()
        {
            var fb = _lookup.BuildDefeated("inst1", "r1");
            Assert.That(fb, Is.Not.Null);
            Assert.That(fb!.Value.Kind, Is.EqualTo(FeedbackKind.CharacterDefeated));
            Assert.That(fb.Value.Intensity, Is.EqualTo(FeedbackIntensity.Critical));
        }

        [Test]
        public void QualityTier_DoesNotAffect_ParryGrade()
        {
            _lookup.SetQualityTier(QualityTier.Low);
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "target1");

            Assert.That(bundle!.Value.Feedback.Kind, Is.EqualTo(FeedbackKind.PerfectParry),
                "Quality degradation must not change parry grade");
        }
    }
}
