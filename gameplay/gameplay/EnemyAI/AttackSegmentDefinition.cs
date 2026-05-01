namespace Game.Gameplay.EnemyAI
{
    /// <summary>
    /// 攻击段定义（ADR-0008 / TR-enemy-002）。
    /// 内容事实：baseDamage、相对延迟时间、profileId、comboIndex。
    /// </summary>
    public readonly struct AttackSegmentDefinition
    {
        public string SegmentId { get; }
        public int ComboIndex { get; }
        public int BaseDamage { get; }
        public string ParryProfileId { get; }

        public long RelativeWindupDelayMs { get; }
        public long RelativeImpactDelayMs { get; }
        public long RelativeDamageCommitDelayMs { get; }
        public long RelativeRecoveryEndDelayMs { get; }

        public AttackSegmentDefinition(
            string segmentId,
            int comboIndex,
            int baseDamage,
            string parryProfileId,
            long relativeWindupDelayMs,
            long relativeImpactDelayMs,
            long relativeDamageCommitDelayMs,
            long relativeRecoveryEndDelayMs)
        {
            SegmentId = segmentId;
            ComboIndex = comboIndex;
            BaseDamage = baseDamage;
            ParryProfileId = parryProfileId;
            RelativeWindupDelayMs = relativeWindupDelayMs;
            RelativeImpactDelayMs = relativeImpactDelayMs;
            RelativeDamageCommitDelayMs = relativeDamageCommitDelayMs;
            RelativeRecoveryEndDelayMs = relativeRecoveryEndDelayMs;
        }
    }
}
