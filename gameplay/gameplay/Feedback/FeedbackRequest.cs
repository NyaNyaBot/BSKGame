namespace Game.Gameplay.Feedback
{
    /// <summary>
    /// 反馈请求（ADR-0009 / TR-fx-001）。
    /// 由反馈服务从战斗事件构建，提交给 Unity 表现层播放。
    /// </summary>
    public readonly struct FeedbackRequest
    {
        public string SourceEventId { get; }
        public FeedbackKind Kind { get; }
        public FeedbackIntensity Intensity { get; }
        public string TargetInstanceId { get; }

        public FeedbackRequest(string sourceEventId, FeedbackKind kind, FeedbackIntensity intensity, string targetInstanceId)
        {
            SourceEventId = sourceEventId;
            Kind = kind;
            Intensity = intensity;
            TargetInstanceId = targetInstanceId;
        }
    }

    public enum FeedbackKind : byte
    {
        PerfectParry = 0,
        NormalParry = 1,
        FailedParry = 2,
        DamageReceived = 3,
        CharacterDefeated = 4,
        CounterHit = 5,
    }

    public enum FeedbackIntensity : byte
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3,
    }
}
