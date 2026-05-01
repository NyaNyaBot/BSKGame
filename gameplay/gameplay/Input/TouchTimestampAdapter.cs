using Game.Gameplay.Combat;
using SysMath = System.Math;

namespace Game.Gameplay.Input
{
    /// <summary>
    /// 将平台触摸/PointerDown 时间戳映射到 battleTimestampMs（ADR-0006）。
    /// </summary>
    public sealed class TouchTimestampAdapter
    {
        public const long DefaultMaxBridgeDelayMs = 200;

        private readonly ICombatClock _clock;
        private long _calibratedBridgeOffsetMs;

        public long MaxBridgeDelayMs { get; set; } = DefaultMaxBridgeDelayMs;

        public TouchTimestampAdapter(ICombatClock clock, long calibratedBridgeOffsetMs = 0)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _calibratedBridgeOffsetMs = calibratedBridgeOffsetMs;
        }

        /// <summary>
        /// 映射平台触摸时间戳到战斗时钟时间。
        /// </summary>
        /// <param name="normalizedRawTimestampMs">平台原始时间戳（已归一到 ms）。</param>
        /// <param name="receivedRealtimeMs">收到事件时的实时时间（已归一到 ms）。</param>
        /// <returns>映射结果。</returns>
        public TouchTimestampResult Map(long normalizedRawTimestampMs, long receivedRealtimeMs)
        {
            long bridgeDelayMs = SysMath.Max(0,
                receivedRealtimeMs - normalizedRawTimestampMs - _calibratedBridgeOffsetMs);

            bool isFallback = false;

            if (bridgeDelayMs > MaxBridgeDelayMs)
            {
                bridgeDelayMs = MaxBridgeDelayMs;
                isFallback = true;
            }

            if (normalizedRawTimestampMs > receivedRealtimeMs)
            {
                bridgeDelayMs = 0;
                isFallback = true;
            }

            long battleTimestampMs = SysMath.Max(0, _clock.NowMs - bridgeDelayMs);

            return new TouchTimestampResult(
                battleTimestampMs,
                bridgeDelayMs,
                isFallback,
                _clock.NowMs);
        }

        public void SetCalibratedOffset(long offsetMs)
        {
            _calibratedBridgeOffsetMs = offsetMs;
        }
    }

    public readonly struct TouchTimestampResult
    {
        public long BattleTimestampMs { get; }
        public long BridgeDelayMs { get; }
        public bool IsFallback { get; }
        public long CombatClockNowMs { get; }

        public TouchTimestampResult(long battleTimestampMs, long bridgeDelayMs, bool isFallback, long combatClockNowMs)
        {
            BattleTimestampMs = battleTimestampMs;
            BridgeDelayMs = bridgeDelayMs;
            IsFallback = isFallback;
            CombatClockNowMs = combatClockNowMs;
        }
    }
}
