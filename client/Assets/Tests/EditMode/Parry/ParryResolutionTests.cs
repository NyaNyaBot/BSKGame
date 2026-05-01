using Game.Gameplay.Input;
using Game.Gameplay.Parry;
using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Parry
{
    /// <summary>
    /// ADR-0007 / story-002：ParryAttempt 结算与 grade 判定。
    /// </summary>
    public sealed class ParryResolutionTests
    {
        private FrozenAttackSegmentTimeline _timeline;
        private ParryResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _resolver = new ParryResolver();

            var seed = new AttackSegmentTimelineSeed(0, 1000, 1100, 1500);
            var profile = new ParryWindowProfile(50, 100, 100, 200);

            var result = TimelineDerivation.Derive(seed, profile);
            Assert.That(result.IsValid, Is.True, result.ValidationError);
            _timeline = result.Timeline;
        }

        [Test]
        public void PerfectParry_InPerfectWindow()
        {
            var attempt = MakeAttempt(_timeline.ParryCenterMs);
            var res = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt);

            Assert.That(res.Grade, Is.EqualTo(ParryGrade.PerfectParry));
            Assert.That(res.DamageMultiplierBasisPoints, Is.EqualTo(0));
            Assert.That(res.EchoIntent, Is.EqualTo(EchoIntent.High));
        }

        private static ParryAttempt MakeAttempt(long battleTs)
        {
            return new ParryAttempt(battleTs, 0, false, "area1", 1, default);
        }

        [Test]
        public void NormalParry_InNormalWindow_BeforePerfect()
        {
            long ts = _timeline.WindowOpenMs + 10;
            var attempt = MakeAttempt(ts);
            var res = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt);

            Assert.That(res.Grade, Is.EqualTo(ParryGrade.NormalParry));
            Assert.That(res.DamageMultiplierBasisPoints, Is.EqualTo(5000));
        }

        [Test]
        public void NormalParry_InNormalWindow_AfterPerfect()
        {
            long ts = _timeline.PerfectEndMs + 10;
            var attempt = MakeAttempt(ts);
            var res = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt);

            Assert.That(res.Grade, Is.EqualTo(ParryGrade.NormalParry));
            Assert.That(res.DamageMultiplierBasisPoints, Is.EqualTo(5000));
        }

        [Test]
        public void FailedParry_OutOfWindow()
        {
            long ts = _timeline.NormalEndMs + 100;
            var attempt = MakeAttempt(ts);
            var res = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt);

            Assert.That(res.Grade, Is.EqualTo(ParryGrade.FailedParry));
            Assert.That(res.DamageMultiplierBasisPoints, Is.EqualTo(10000));
        }

        [Test]
        public void FailedParry_NoInput()
        {
            var res = _resolver.ResolveNoInput("seg1", 1);

            Assert.That(res.Grade, Is.EqualTo(ParryGrade.FailedParry));
            Assert.That(res.FailureReason, Is.EqualTo(ParryFailureReason.NoInput));
            Assert.That(res.DamageMultiplierBasisPoints, Is.EqualTo(10000));
        }

        [Test]
        public void BufferedInput_SnappedToWindowOpen()
        {
            long bufferTs = _timeline.WindowOpenMs - 50;
            var attempt = MakeAttempt(bufferTs);
            var res = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt);

            Assert.That(res.Grade, Is.Not.EqualTo(ParryGrade.FailedParry),
                "Buffered input within 100ms should snap to window open");
        }

        [Test]
        public void Idempotent_SameSegment_ReturnsCachedResult()
        {
            var attempt = MakeAttempt(_timeline.ParryCenterMs);
            var res1 = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt);

            var attempt2 = MakeAttempt(_timeline.WindowOpenMs);
            var res2 = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt2);

            Assert.That(res2.ResolutionId, Is.EqualTo(res1.ResolutionId));
            Assert.That(res2.Grade, Is.EqualTo(res1.Grade));
        }

        [Test]
        public void PerfectBoundary_Start()
        {
            var attempt = MakeAttempt(_timeline.PerfectStartMs);
            var res = _resolver.ResolveAttempt(_timeline, "seg1", 1, attempt);
            Assert.That(res.Grade, Is.EqualTo(ParryGrade.PerfectParry));
        }

        [Test]
        public void PerfectBoundary_End()
        {
            var attempt = MakeAttempt(_timeline.PerfectEndMs);
            var res = _resolver.ResolveAttempt(_timeline, "seg2", 1, attempt);
            Assert.That(res.Grade, Is.EqualTo(ParryGrade.PerfectParry));
        }

        [Test]
        public void NormalBoundary_WindowOpen()
        {
            var attempt = MakeAttempt(_timeline.WindowOpenMs);
            var res = _resolver.ResolveAttempt(_timeline, "seg3", 1, attempt);
            Assert.That(res.Grade, Is.EqualTo(ParryGrade.NormalParry));
        }
    }
}
