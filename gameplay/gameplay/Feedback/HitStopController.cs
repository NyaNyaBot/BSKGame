using Game.Gameplay.Combat;

namespace Game.Gameplay.Feedback
{
    /// <summary>
    /// HitStop 裁决器（ADR-0009 / TR-fx-002）。
    /// 接收 HitStopRequest，暂停 CombatClock 并在持续时间结束后恢复。
    /// 更高优先级的请求可覆盖当前 HitStop。
    /// </summary>
    public sealed class HitStopController
    {
        private readonly ICombatClockController _clock;

        private bool _active;
        private int _remainingMs;
        private FeedbackIntensity _currentPriority;

        public bool IsActive => _active;
        public int RemainingMs => _remainingMs;
        public FeedbackIntensity CurrentPriority => _currentPriority;

        public HitStopController(ICombatClockController clock)
        {
            _clock = clock ?? throw new System.ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// 提交 HitStop 请求。优先级 >= 当前才生效。
        /// </summary>
        public bool Submit(HitStopRequest request)
        {
            if (request.DurationMs <= 0)
                return false;

            if (_active && request.Priority < _currentPriority)
                return false;

            _active = true;
            _remainingMs = request.DurationMs;
            _currentPriority = request.Priority;
            _clock.Pause();
            return true;
        }

        /// <summary>
        /// 每帧调用，消耗真实 deltaMs。
        /// 当 HitStop 剩余时间归零时恢复时钟。
        /// </summary>
        public void TickRealtime(int deltaMs)
        {
            if (!_active)
                return;

            _remainingMs -= deltaMs;
            if (_remainingMs <= 0)
            {
                _remainingMs = 0;
                _active = false;
                _clock.Resume();
            }
        }

        public void ForceCancel()
        {
            if (!_active)
                return;

            _remainingMs = 0;
            _active = false;
            _clock.Resume();
        }

        public void Reset()
        {
            if (_active)
                _clock.Resume();

            _active = false;
            _remainingMs = 0;
        }
    }
}
