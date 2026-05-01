namespace Game.Gameplay.Damage
{
    /// <summary>
    /// 伤害处理结果（ADR-0005）。
    /// </summary>
    public readonly struct DamageResult
    {
        public string RequestId { get; }
        public string TargetInstanceId { get; }
        public DamageOutcome Outcome { get; }
        public int ShieldAbsorbed { get; }
        public int HpDamage { get; }
        public int FinalHp { get; }
        public int FinalShield { get; }
        public bool IsDefeated { get; }
        public string? RejectionReason { get; }

        public DamageResult(
            string requestId,
            string targetInstanceId,
            DamageOutcome outcome,
            int shieldAbsorbed,
            int hpDamage,
            int finalHp,
            int finalShield,
            bool isDefeated,
            string? rejectionReason = null)
        {
            RequestId = requestId;
            TargetInstanceId = targetInstanceId;
            Outcome = outcome;
            ShieldAbsorbed = shieldAbsorbed;
            HpDamage = hpDamage;
            FinalHp = finalHp;
            FinalShield = finalShield;
            IsDefeated = isDefeated;
            RejectionReason = rejectionReason;
        }

        public static DamageResult Rejected(string requestId, string targetInstanceId, string reason)
        {
            return new DamageResult(requestId, targetInstanceId, DamageOutcome.Rejected,
                0, 0, 0, 0, false, reason);
        }
    }

    public enum DamageOutcome : byte
    {
        Applied = 0,
        Rejected = 1,
    }
}
