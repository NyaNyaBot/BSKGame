using Game.Gameplay.Combat;
using Game.Gameplay.Feedback;
using Game.Gameplay.Parry;
using NUnit.Framework;

namespace Game.Tests.EditMode.Feedback
{
    /// <summary>
    /// ADR-0009 / story-003：质量降级集成测试。
    /// 验证 QualityTier 对反馈管线（HitStop 生成 + HitStopController 裁决）的影响。
    /// </summary>
    public sealed class QualityDegradationTests
    {
        private FeedbackProfileLookup _lookup;
        private CombatClock _clock;
        private HitStopController _hitStop;

        [SetUp]
        public void SetUp()
        {
            _lookup = new FeedbackProfileLookup();
            _clock = new CombatClock();
            _hitStop = new HitStopController(_clock);
        }

        [Test]
        public void HighTier_PerfectParry_FullHitStop()
        {
            _lookup.SetQualityTier(QualityTier.High);
            var bundle = _lookup.BuildFromParryResolution("r1", ParryGrade.PerfectParry, "t1");
            Assert.That(bundle!.Value.HitStop, Is.Not.Null);
            Assert.That(bundle.Value.HitStop!.Value.DurationMs, Is.EqualTo(FeedbackProfileLookup.PerfectHitStopMs));

            var accepted = _hitStop.Submit(bundle.Value.HitStop.Value);
            Assert.That(accepted, Is.True);
            Assert.That(_clock.IsPaused, Is.True);
        }

        [Test]
        public void MediumTier_PerfectParry_StillHasHitStop()
        {
            _lookup.SetQualityTier(QualityTier.Medium);
            var bundle = _lookup.BuildFromParryResolution("r1", ParryGrade.PerfectParry, "t1");

            Assert.That(bundle!.Value.HitStop, Is.Not.Null,
                "Medium tier should still produce HitStop for PerfectParry");
        }

        [Test]
        public void MediumTier_NormalParry_NoHitStop()
        {
            _lookup.SetQualityTier(QualityTier.Medium);
            var bundle = _lookup.BuildFromParryResolution("r1", ParryGrade.NormalParry, "t1");

            Assert.That(bundle!.Value.HitStop, Is.Null,
                "Medium tier should skip HitStop for NormalParry");
        }

        [Test]
        public void LowTier_PerfectParry_NoHitStop()
        {
            _lookup.SetQualityTier(QualityTier.Low);
            var bundle = _lookup.BuildFromParryResolution("r1", ParryGrade.PerfectParry, "t1");

            Assert.That(bundle!.Value.HitStop, Is.Null,
                "Low tier should skip all HitStop");
        }

        [Test]
        public void LowTier_StillEmits_FeedbackRequest()
        {
            _lookup.SetQualityTier(QualityTier.Low);
            var bundle = _lookup.BuildFromParryResolution("r1", ParryGrade.PerfectParry, "t1");

            Assert.That(bundle!.Value.Feedback.Kind, Is.EqualTo(FeedbackKind.PerfectParry),
                "Quality degradation must not remove feedback entirely");
            Assert.That(bundle.Value.Feedback.Intensity, Is.EqualTo(FeedbackIntensity.Critical),
                "Quality degradation must not change intensity");
        }

        [Test]
        public void TierSwitch_MidBattle_Idempotent()
        {
            _lookup.SetQualityTier(QualityTier.High);
            var b1 = _lookup.BuildFromParryResolution("r1", ParryGrade.PerfectParry, "t1");
            Assert.That(b1!.Value.HitStop, Is.Not.Null);

            _lookup.SetQualityTier(QualityTier.Low);
            var b2 = _lookup.BuildFromParryResolution("r2", ParryGrade.PerfectParry, "t2");
            Assert.That(b2!.Value.HitStop, Is.Null);
        }

        [Test]
        public void FullPipeline_HighTier()
        {
            _lookup.SetQualityTier(QualityTier.High);
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "hero1");

            Assert.That(bundle, Is.Not.Null);
            Assert.That(bundle!.Value.Feedback.Kind, Is.EqualTo(FeedbackKind.PerfectParry));

            _hitStop.Submit(bundle.Value.HitStop!.Value);
            Assert.That(_clock.IsPaused, Is.True);

            _clock.Advance(1000);
            Assert.That(_clock.NowMs, Is.EqualTo(0), "Clock frozen during HitStop");

            _hitStop.TickRealtime(90);
            Assert.That(_clock.IsPaused, Is.False, "Clock resumed after HitStop expires");

            _clock.Advance(100);
            Assert.That(_clock.NowMs, Is.EqualTo(100));
        }

        [Test]
        public void FullPipeline_LowTier_SkipsHitStop()
        {
            _lookup.SetQualityTier(QualityTier.Low);
            var bundle = _lookup.BuildFromParryResolution("res1", ParryGrade.PerfectParry, "hero1");

            Assert.That(bundle!.Value.Feedback.Kind, Is.EqualTo(FeedbackKind.PerfectParry));
            Assert.That(bundle.Value.HitStop, Is.Null);
            Assert.That(_clock.IsPaused, Is.False, "No HitStop submitted");
        }
    }
}
