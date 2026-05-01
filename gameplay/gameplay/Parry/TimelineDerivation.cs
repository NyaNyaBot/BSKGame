namespace Game.Gameplay.Parry
{
    /// <summary>
    /// 从 seed + profile 派生 FrozenAttackSegmentTimeline（ADR-0007）。
    /// </summary>
    public static class TimelineDerivation
    {
        public readonly struct DerivationResult
        {
            public bool IsValid { get; }
            public FrozenAttackSegmentTimeline Timeline { get; }
            public string? ValidationError { get; }

            private DerivationResult(bool isValid, FrozenAttackSegmentTimeline timeline, string? error)
            {
                IsValid = isValid;
                Timeline = timeline;
                ValidationError = error;
            }

            public static DerivationResult Success(FrozenAttackSegmentTimeline timeline)
                => new DerivationResult(true, timeline, null);

            public static DerivationResult Failure(string error)
                => new DerivationResult(false, default, error);
        }

        public static DerivationResult Derive(
            in AttackSegmentTimelineSeed seed,
            in ParryWindowProfile profile)
        {
            if (seed.WindupStartMs < 0)
                return DerivationResult.Failure("windupStartMs must be >= 0");

            if (seed.VisualImpactMs < seed.WindupStartMs)
                return DerivationResult.Failure("visualImpactMs must be >= windupStartMs");

            if (seed.DamageCommitMs < seed.VisualImpactMs)
                return DerivationResult.Failure("damageCommitMs must be >= visualImpactMs");

            if (seed.RecoveryEndMs < seed.DamageCommitMs)
                return DerivationResult.Failure("recoveryEndMs must be >= damageCommitMs");

            long parryCenterMs = seed.VisualImpactMs - profile.CenterBeforeImpactMs;
            long perfectStartMs = parryCenterMs - profile.PerfectHalfWindowMs;
            long perfectEndMs = parryCenterMs + profile.PerfectHalfWindowMs;
            long windowOpenMs = perfectStartMs - profile.NormalEarlyWindowMs;
            long normalEndMs = perfectEndMs + profile.NormalLateWindowMs;

            if (windowOpenMs < seed.WindupStartMs)
                return DerivationResult.Failure(
                    $"Derived windowOpenMs ({windowOpenMs}) < windupStartMs ({seed.WindupStartMs}). " +
                    "Parry window opens before attack windup starts.");

            if (normalEndMs > seed.VisualImpactMs)
                return DerivationResult.Failure(
                    $"Derived normalEndMs ({normalEndMs}) > visualImpactMs ({seed.VisualImpactMs}). " +
                    "Parry window extends past visual impact.");

            if (windowOpenMs > perfectStartMs ||
                perfectStartMs > parryCenterMs ||
                parryCenterMs > perfectEndMs ||
                perfectEndMs > normalEndMs)
            {
                return DerivationResult.Failure("Timeline invariant violation: window timestamps not monotonically ordered.");
            }

            var timeline = new FrozenAttackSegmentTimeline(
                seed.WindupStartMs,
                windowOpenMs,
                perfectStartMs,
                parryCenterMs,
                perfectEndMs,
                normalEndMs,
                seed.VisualImpactMs,
                seed.DamageCommitMs,
                seed.RecoveryEndMs);

            return DerivationResult.Success(timeline);
        }
    }
}
