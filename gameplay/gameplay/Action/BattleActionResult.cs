using Game.Gameplay.Damage;

namespace Game.Gameplay.BattleAction
{
    /// <summary>
    /// 行动结果（ADR-0012）。
    /// </summary>
    public readonly struct BattleActionResult
    {
        public string ActionRequestId { get; }
        public BattleActionOutcome Outcome { get; }
        public DamageRequest? GeneratedDamageRequest { get; }
        public string? RejectionReason { get; }

        private BattleActionResult(string id, BattleActionOutcome outcome, DamageRequest? dmg, string? reason)
        {
            ActionRequestId = id;
            Outcome = outcome;
            GeneratedDamageRequest = dmg;
            RejectionReason = reason;
        }

        public static BattleActionResult Accepted(string id, DamageRequest dmgReq)
            => new BattleActionResult(id, BattleActionOutcome.Accepted, dmgReq, null);

        public static BattleActionResult Rejected(string id, string reason)
            => new BattleActionResult(id, BattleActionOutcome.Rejected, null, reason);
    }

    public enum BattleActionOutcome : byte
    {
        Accepted = 0,
        Rejected = 1,
    }
}
