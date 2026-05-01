using Game.Gameplay.Combat;
using Game.Gameplay.Input;
using NUnit.Framework;

namespace Game.Tests.EditMode.Input
{
    /// <summary>
    /// ADR-0006 / story-001：触摸时间戳映射到 CombatClockMs。
    /// </summary>
    public sealed class TouchTimestampAdapterTests
    {
        [Test]
        public void Map_NormalCase_ComputesBridgeDelay()
        {
            var clock = new CombatClock { MaxStepMs = 2000 };
            clock.Advance(1000);
            var adapter = new TouchTimestampAdapter(clock);

            var result = adapter.Map(
                normalizedRawTimestampMs: 900,
                receivedRealtimeMs: 950);

            Assert.That(result.BridgeDelayMs, Is.EqualTo(50));
            Assert.That(result.BattleTimestampMs, Is.EqualTo(950));
            Assert.That(result.IsFallback, Is.False);
        }

        [Test]
        public void Map_NegativeBridgeDelay_ClampedToZero()
        {
            var clock = new CombatClock();
            clock.Advance(500);
            var adapter = new TouchTimestampAdapter(clock);

            var result = adapter.Map(
                normalizedRawTimestampMs: 1000,
                receivedRealtimeMs: 900);

            Assert.That(result.BridgeDelayMs, Is.EqualTo(0));
            Assert.That(result.IsFallback, Is.True, "Non-monotonic timestamps should flag fallback");
        }

        [Test]
        public void Map_ExcessiveBridgeDelay_ClampsToMax()
        {
            var clock = new CombatClock { MaxStepMs = 2000 };
            clock.Advance(1000);
            var adapter = new TouchTimestampAdapter(clock) { MaxBridgeDelayMs = 100 };

            var result = adapter.Map(
                normalizedRawTimestampMs: 500,
                receivedRealtimeMs: 900);

            Assert.That(result.BridgeDelayMs, Is.EqualTo(100));
            Assert.That(result.IsFallback, Is.True);
            Assert.That(result.BattleTimestampMs, Is.EqualTo(900));
        }

        [Test]
        public void Map_WithCalibratedOffset_AdjustsBridgeDelay()
        {
            var clock = new CombatClock();
            clock.Advance(1000);
            var adapter = new TouchTimestampAdapter(clock, calibratedBridgeOffsetMs: 20);

            var result = adapter.Map(
                normalizedRawTimestampMs: 900,
                receivedRealtimeMs: 950);

            Assert.That(result.BridgeDelayMs, Is.EqualTo(30));
        }

        [Test]
        public void Map_BattleTimestampMs_NeverNegative()
        {
            var clock = new CombatClock();
            clock.Advance(10);
            var adapter = new TouchTimestampAdapter(clock);

            var result = adapter.Map(
                normalizedRawTimestampMs: 0,
                receivedRealtimeMs: 100);

            Assert.That(result.BattleTimestampMs, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Map_ZeroDelta_ZeroBridgeDelay()
        {
            var clock = new CombatClock();
            clock.Advance(500);
            var adapter = new TouchTimestampAdapter(clock);

            var result = adapter.Map(
                normalizedRawTimestampMs: 100,
                receivedRealtimeMs: 100);

            Assert.That(result.BridgeDelayMs, Is.EqualTo(0));
            Assert.That(result.IsFallback, Is.False);
        }
    }
}
