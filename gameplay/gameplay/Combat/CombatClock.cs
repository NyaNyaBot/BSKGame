
namespace Game.Gameplay.Combat
{
    /// <summary>
    /// 默认战斗时钟实现（ADR-0004）。
    /// </summary>
    public sealed class CombatClock : ICombatClockController
    {
        public const long DefaultMaxStepMs = 500;

        private long _nowMs;
        private bool _isPaused;

        public long NowMs => _nowMs;
        public bool IsPaused => _isPaused;
        public long MaxStepMs { get; set; } = DefaultMaxStepMs;

        public void Advance(long deltaMs)
        {
            if (deltaMs < 0)
                throw new System.ArgumentOutOfRangeException(nameof(deltaMs), "CombatClock.Advance rejects negative delta.");

            if (_isPaused)
                return;

            if (deltaMs > MaxStepMs)
                deltaMs = MaxStepMs;

            _nowMs += deltaMs;
        }

        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;

        public void Reset()
        {
            _nowMs = 0;
            _isPaused = false;
        }
    }
}
