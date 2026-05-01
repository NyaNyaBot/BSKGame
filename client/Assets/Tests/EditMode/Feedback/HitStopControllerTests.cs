using Game.Gameplay.Combat;
using Game.Gameplay.Feedback;
using NUnit.Framework;

namespace Game.Tests.EditMode.Feedback
{
    /// <summary>
    /// ADR-0009 / story-002：HitStop 裁决器与 CombatClock 暂停/恢复。
    /// </summary>
    public sealed class HitStopControllerTests
    {
        private CombatClock _clock;
        private HitStopController _controller;

        [SetUp]
        public void SetUp()
        {
            _clock = new CombatClock();
            _controller = new HitStopController(_clock);
        }

        [Test]
        public void Submit_PausesClock()
        {
            var req = new HitStopRequest("r1", 90, FeedbackIntensity.Critical);
            var accepted = _controller.Submit(req);

            Assert.That(accepted, Is.True);
            Assert.That(_clock.IsPaused, Is.True);
            Assert.That(_controller.IsActive, Is.True);
            Assert.That(_controller.RemainingMs, Is.EqualTo(90));
        }

        [Test]
        public void TickRealtime_DecrementsRemaining()
        {
            _controller.Submit(new HitStopRequest("r1", 90, FeedbackIntensity.Critical));
            _controller.TickRealtime(30);

            Assert.That(_controller.RemainingMs, Is.EqualTo(60));
            Assert.That(_controller.IsActive, Is.True);
            Assert.That(_clock.IsPaused, Is.True);
        }

        [Test]
        public void TickRealtime_Expiry_ResumesClock()
        {
            _controller.Submit(new HitStopRequest("r1", 50, FeedbackIntensity.High));
            _controller.TickRealtime(50);

            Assert.That(_controller.IsActive, Is.False);
            Assert.That(_controller.RemainingMs, Is.EqualTo(0));
            Assert.That(_clock.IsPaused, Is.False);
        }

        [Test]
        public void TickRealtime_Overshoot_ClampZero()
        {
            _controller.Submit(new HitStopRequest("r1", 30, FeedbackIntensity.Medium));
            _controller.TickRealtime(100);

            Assert.That(_controller.RemainingMs, Is.EqualTo(0));
            Assert.That(_controller.IsActive, Is.False);
            Assert.That(_clock.IsPaused, Is.False);
        }

        [Test]
        public void HigherPriority_Overrides_Active()
        {
            _controller.Submit(new HitStopRequest("r1", 50, FeedbackIntensity.Medium));
            var overridden = _controller.Submit(new HitStopRequest("r2", 90, FeedbackIntensity.Critical));

            Assert.That(overridden, Is.True);
            Assert.That(_controller.RemainingMs, Is.EqualTo(90));
            Assert.That(_controller.CurrentPriority, Is.EqualTo(FeedbackIntensity.Critical));
        }

        [Test]
        public void LowerPriority_Rejected()
        {
            _controller.Submit(new HitStopRequest("r1", 90, FeedbackIntensity.Critical));
            var rejected = _controller.Submit(new HitStopRequest("r2", 50, FeedbackIntensity.Low));

            Assert.That(rejected, Is.False);
            Assert.That(_controller.RemainingMs, Is.EqualTo(90), "Original HitStop unchanged");
        }

        [Test]
        public void EqualPriority_Accepted()
        {
            _controller.Submit(new HitStopRequest("r1", 50, FeedbackIntensity.High));
            var accepted = _controller.Submit(new HitStopRequest("r2", 70, FeedbackIntensity.High));

            Assert.That(accepted, Is.True);
            Assert.That(_controller.RemainingMs, Is.EqualTo(70));
        }

        [Test]
        public void ForceCancel_ResumesClock()
        {
            _controller.Submit(new HitStopRequest("r1", 90, FeedbackIntensity.Critical));
            _controller.ForceCancel();

            Assert.That(_controller.IsActive, Is.False);
            Assert.That(_clock.IsPaused, Is.False);
        }

        [Test]
        public void ZeroDuration_Rejected()
        {
            var rejected = _controller.Submit(new HitStopRequest("r1", 0, FeedbackIntensity.High));
            Assert.That(rejected, Is.False);
            Assert.That(_controller.IsActive, Is.False);
        }

        [Test]
        public void ClockAdvance_Blocked_DuringHitStop()
        {
            _clock.Advance(100);
            Assert.That(_clock.NowMs, Is.EqualTo(100));

            _controller.Submit(new HitStopRequest("r1", 50, FeedbackIntensity.High));
            _clock.Advance(200);

            Assert.That(_clock.NowMs, Is.EqualTo(100), "Clock should not advance during HitStop");
        }

        [Test]
        public void Reset_ClearsState()
        {
            _controller.Submit(new HitStopRequest("r1", 90, FeedbackIntensity.Critical));
            _controller.Reset();

            Assert.That(_controller.IsActive, Is.False);
            Assert.That(_clock.IsPaused, Is.False);
        }

        [Test]
        public void NoActiveHitStop_TickRealtime_NoOp()
        {
            _controller.TickRealtime(100);

            Assert.That(_controller.IsActive, Is.False);
            Assert.That(_clock.IsPaused, Is.False);
        }
    }
}
