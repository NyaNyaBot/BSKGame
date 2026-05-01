namespace Game.Gameplay.Damage
{
    /// <summary>
    /// 伤害请求（ADR-0005 / TR-dmg-001）。
    /// 所有 HP 变更必须经此请求通过 IDamageService。
    /// </summary>
    public readonly struct DamageRequest
    {
        public string RequestId { get; }
        public string TargetInstanceId { get; }
        public int RawDamage { get; }
        public DamageType DamageType { get; }

        public DamageRequest(string requestId, string targetInstanceId, int rawDamage, DamageType damageType = DamageType.Physical)
        {
            RequestId = requestId;
            TargetInstanceId = targetInstanceId;
            RawDamage = rawDamage;
            DamageType = damageType;
        }
    }

    public enum DamageType : byte
    {
        Physical = 0,
        Magical = 1,
        True = 2,
    }
}
