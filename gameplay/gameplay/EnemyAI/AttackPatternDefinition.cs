namespace Game.Gameplay.EnemyAI
{
    /// <summary>
    /// 攻击模式定义（ADR-0008 / TR-enemy-001）。
    /// </summary>
    public sealed class AttackPatternDefinition
    {
        public string PatternId { get; }
        public int BaseWeight { get; }
        public int CooldownTurns { get; }
        public HpPhaseRequirement HpPhase { get; }
        public int BaseDamage { get; }

        public AttackPatternDefinition(
            string patternId,
            int baseWeight,
            int cooldownTurns,
            HpPhaseRequirement hpPhase,
            int baseDamage)
        {
            PatternId = patternId;
            BaseWeight = baseWeight;
            CooldownTurns = cooldownTurns;
            HpPhase = hpPhase;
            BaseDamage = baseDamage;
        }
    }

    public enum HpPhaseRequirement : byte
    {
        Any = 0,
        AboveHalf = 1,
        BelowHalf = 2,
    }
}
