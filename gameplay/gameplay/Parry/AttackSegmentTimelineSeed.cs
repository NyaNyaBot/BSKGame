namespace Game.Gameplay.Parry
{
    /// <summary>
    /// 攻击段时间线 seed（ADR-0007 / TR-parry-001）。
    /// 由攻击模式系统提供原始时间戳，弹反系统从中派生 FrozenAttackSegmentTimeline。
    /// </summary>
    public readonly struct AttackSegmentTimelineSeed
    {
        public long WindupStartMs { get; }
        public long VisualImpactMs { get; }
        public long DamageCommitMs { get; }
        public long RecoveryEndMs { get; }

        public AttackSegmentTimelineSeed(
            long windupStartMs,
            long visualImpactMs,
            long damageCommitMs,
            long recoveryEndMs)
        {
            WindupStartMs = windupStartMs;
            VisualImpactMs = visualImpactMs;
            DamageCommitMs = damageCommitMs;
            RecoveryEndMs = recoveryEndMs;
        }
    }

    /// <summary>
    /// 弹反窗口配置 profile（ADR-0007）。
    /// </summary>
    public readonly struct ParryWindowProfile
    {
        public long PerfectHalfWindowMs { get; }
        public long NormalEarlyWindowMs { get; }
        public long NormalLateWindowMs { get; }
        public long CenterBeforeImpactMs { get; }

        public ParryWindowProfile(
            long perfectHalfWindowMs,
            long normalEarlyWindowMs,
            long normalLateWindowMs,
            long centerBeforeImpactMs)
        {
            PerfectHalfWindowMs = perfectHalfWindowMs;
            NormalEarlyWindowMs = normalEarlyWindowMs;
            NormalLateWindowMs = normalLateWindowMs;
            CenterBeforeImpactMs = centerBeforeImpactMs;
        }
    }
}
