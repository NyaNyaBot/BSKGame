using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;

namespace Game.Gameplay.Damage
{
    /// <summary>
    /// 伤害成功事件（ADR-0005, ADR-0002 / TR-dmg-003）。
    /// </summary>
    public readonly struct DamageApplied : IBattleEvent
    {
        public string EventId => "damage.applied";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }

        public string RequestId { get; }
        public string TargetInstanceId { get; }
        public int ShieldAbsorbed { get; }
        public int HpDamage { get; }
        public int FinalHp { get; }
        public int FinalShield { get; }

        public DamageApplied(
            SceneEventContext context, long clockMs,
            string requestId, string targetInstanceId,
            int shieldAbsorbed, int hpDamage, int finalHp, int finalShield)
        {
            Context = context;
            OccurredAtCombatClockMs = clockMs;
            RequestId = requestId;
            TargetInstanceId = targetInstanceId;
            ShieldAbsorbed = shieldAbsorbed;
            HpDamage = hpDamage;
            FinalHp = finalHp;
            FinalShield = finalShield;
        }
    }

    /// <summary>
    /// 伤害拒绝事件（ADR-0005 / TR-dmg-003）。
    /// </summary>
    public readonly struct DamageRejected : IBattleEvent
    {
        public string EventId => "damage.rejected";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }

        public string RequestId { get; }
        public string TargetInstanceId { get; }
        public string Reason { get; }

        public DamageRejected(
            SceneEventContext context, long clockMs,
            string requestId, string targetInstanceId, string reason)
        {
            Context = context;
            OccurredAtCombatClockMs = clockMs;
            RequestId = requestId;
            TargetInstanceId = targetInstanceId;
            Reason = reason;
        }
    }

    /// <summary>
    /// 角色击败事件（ADR-0005, ADR-0011 / TR-dmg-004）。
    /// </summary>
    public readonly struct CharacterDefeated : IBattleEvent
    {
        public string EventId => "character.defeated";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }

        public string InstanceId { get; }
        public string LastDamageRequestId { get; }

        public CharacterDefeated(
            SceneEventContext context, long clockMs,
            string instanceId, string lastDamageRequestId)
        {
            Context = context;
            OccurredAtCombatClockMs = clockMs;
            InstanceId = instanceId;
            LastDamageRequestId = lastDamageRequestId;
        }
    }
}
