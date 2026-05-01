namespace Game.Gameplay.Parry
{
    /// <summary>
    /// 不可变攻击段时间线（ADR-0007 / TR-parry-001）。
    /// 所有时间戳满足：windupStartMs <= windowOpenMs <= perfectStartMs <= parryCenterMs
    ///   <= perfectEndMs <= normalEndMs <= visualImpactMs <= damageCommitMs <= recoveryEndMs。
    /// </summary>
    public readonly struct FrozenAttackSegmentTimeline
    {
        public long WindupStartMs { get; }
        public long WindowOpenMs { get; }
        public long PerfectStartMs { get; }
        public long ParryCenterMs { get; }
        public long PerfectEndMs { get; }
        public long NormalEndMs { get; }
        public long VisualImpactMs { get; }
        public long DamageCommitMs { get; }
        public long RecoveryEndMs { get; }

        public FrozenAttackSegmentTimeline(
            long windupStartMs,
            long windowOpenMs,
            long perfectStartMs,
            long parryCenterMs,
            long perfectEndMs,
            long normalEndMs,
            long visualImpactMs,
            long damageCommitMs,
            long recoveryEndMs)
        {
            WindupStartMs = windupStartMs;
            WindowOpenMs = windowOpenMs;
            PerfectStartMs = perfectStartMs;
            ParryCenterMs = parryCenterMs;
            PerfectEndMs = perfectEndMs;
            NormalEndMs = normalEndMs;
            VisualImpactMs = visualImpactMs;
            DamageCommitMs = damageCommitMs;
            RecoveryEndMs = recoveryEndMs;
        }
    }
}
