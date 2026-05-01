namespace Game.Gameplay.Feedback
{
    /// <summary>
    /// Hit stop 请求（ADR-0009, ADR-0004 / TR-fx-002）。
    /// 由反馈服务构建，提交给回合管理器裁决是否暂停 CombatClock。
    /// </summary>
    public readonly struct HitStopRequest
    {
        public string SourceResolutionId { get; }
        public int DurationMs { get; }
        public FeedbackIntensity Priority { get; }

        public HitStopRequest(string sourceResolutionId, int durationMs, FeedbackIntensity priority)
        {
            SourceResolutionId = sourceResolutionId;
            DurationMs = durationMs;
            Priority = priority;
        }
    }
}
