using System;
using Game.Gameplay.Combat;
using NUnit.Framework;

namespace Game.Tests.EditMode.Combat
{
    /// <summary>
    /// ADR-0004：战斗时钟基础行为。
    /// </summary>
    public sealed class CombatClockTests
    {
        [Test]
        public void Advance_IncrementsNowMs()
        {
            var clock = new CombatClock();
            clock.Advance(100);
            Assert.That(clock.NowMs, Is.EqualTo(100));
            clock.Advance(50);
            Assert.That(clock.NowMs, Is.EqualTo(150));
        }

        [Test]
        public void Advance_NegativeDelta_Throws()
        {
            var clock = new CombatClock();
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(-1));
        }

        [Test]
        public void Advance_ClampsToMaxStep()
        {
            var clock = new CombatClock { MaxStepMs = 100 };
            clock.Advance(999);
            Assert.That(clock.NowMs, Is.EqualTo(100));
        }

        [Test]
        public void Pause_StopsAdvance()
        {
            var clock = new CombatClock();
            clock.Advance(100);
            clock.Pause();
            Assert.That(clock.IsPaused, Is.True);
            clock.Advance(50);
            Assert.That(clock.NowMs, Is.EqualTo(100), "Paused clock should not advance");
        }

        [Test]
        public void Resume_AllowsAdvance()
        {
            var clock = new CombatClock();
            clock.Advance(100);
            clock.Pause();
            clock.Resume();
            Assert.That(clock.IsPaused, Is.False);
            clock.Advance(50);
            Assert.That(clock.NowMs, Is.EqualTo(150));
        }

        [Test]
        public void Reset_ClearsClockState()
        {
            var clock = new CombatClock();
            clock.Advance(200);
            clock.Pause();
            clock.Reset();
            Assert.That(clock.NowMs, Is.EqualTo(0));
            Assert.That(clock.IsPaused, Is.False);
        }
    }
}
