namespace Game.Gameplay.Parry
{
    /// <summary>
    /// 弹反结算结果（ADR-0007 / TR-parry-002）。
    /// </summary>
    public readonly struct ParryResolution
    {
        public string ResolutionId { get; }
        public string AttackSegmentId { get; }
        public int TimelineSequenceId { get; }
        public ParryGrade Grade { get; }
        public long TimingOffsetMs { get; }
        public int DamageMultiplierBasisPoints { get; }
        public EchoIntent EchoIntent { get; }
        public ParryFailureReason FailureReason { get; }

        public ParryResolution(
            string resolutionId,
            string attackSegmentId,
            int timelineSequenceId,
            ParryGrade grade,
            long timingOffsetMs,
            int damageMultiplierBasisPoints,
            EchoIntent echoIntent,
            ParryFailureReason failureReason = ParryFailureReason.None)
        {
            ResolutionId = resolutionId;
            AttackSegmentId = attackSegmentId;
            TimelineSequenceId = timelineSequenceId;
            Grade = grade;
            TimingOffsetMs = timingOffsetMs;
            DamageMultiplierBasisPoints = damageMultiplierBasisPoints;
            EchoIntent = echoIntent;
            FailureReason = failureReason;
        }
    }
}
