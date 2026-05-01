using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;

namespace Game.Gameplay.Parry
{
    /// <summary>
    /// 弹反结算事件（ADR-0007 / TR-parry-003）。
    /// </summary>
    public readonly struct ParryResolved : IBattleEvent
    {
        public string EventId => "parry.resolved";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }

        public string ResolutionId { get; }
        public string AttackSegmentId { get; }
        public int TimelineSequenceId { get; }
        public ParryGrade Grade { get; }
        public long TimingOffsetMs { get; }
        public int DamageMultiplierBasisPoints { get; }
        public EchoIntent EchoIntent { get; }

        public ParryResolved(
            SceneEventContext ctx, long clockMs,
            string resolutionId, string attackSegmentId, int timelineSequenceId,
            ParryGrade grade, long timingOffsetMs, int damageMultBp, EchoIntent echo)
        {
            Context = ctx;
            OccurredAtCombatClockMs = clockMs;
            ResolutionId = resolutionId;
            AttackSegmentId = attackSegmentId;
            TimelineSequenceId = timelineSequenceId;
            Grade = grade;
            TimingOffsetMs = timingOffsetMs;
            DamageMultiplierBasisPoints = damageMultBp;
            EchoIntent = echo;
        }
    }

    public readonly struct ParryCancelled : IBattleEvent
    {
        public string EventId => "parry.cancelled";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }
        public string AttackSegmentId { get; }
        public int TimelineSequenceId { get; }
        public string Reason { get; }

        public ParryCancelled(SceneEventContext ctx, long clockMs, string segId, int seqId, string reason)
        {
            Context = ctx;
            OccurredAtCombatClockMs = clockMs;
            AttackSegmentId = segId;
            TimelineSequenceId = seqId;
            Reason = reason;
        }
    }

    public readonly struct AttackSegmentTimelineRejected : IBattleEvent
    {
        public string EventId => "parry.timeline_rejected";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }
        public string AttackSegmentId { get; }
        public string ValidationError { get; }

        public AttackSegmentTimelineRejected(SceneEventContext ctx, long clockMs, string segId, string error)
        {
            Context = ctx;
            OccurredAtCombatClockMs = clockMs;
            AttackSegmentId = segId;
            ValidationError = error;
        }
    }

    /// <summary>
    /// Counter 入口开启事件（ADR-0007 / TR-parry-004）。
    /// 仅 PerfectParry 触发。
    /// </summary>
    public readonly struct CounterEntryOpened : IBattleEvent
    {
        public string EventId => "counter.entry_opened";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }
        public string ResolutionId { get; }
        public string AttackSegmentId { get; }
        public int TimelineSequenceId { get; }
        public long ExpiresAtMs { get; }

        public CounterEntryOpened(
            SceneEventContext ctx, long clockMs,
            string resolutionId, string segId, int seqId, long expiresAtMs)
        {
            Context = ctx;
            OccurredAtCombatClockMs = clockMs;
            ResolutionId = resolutionId;
            AttackSegmentId = segId;
            TimelineSequenceId = seqId;
            ExpiresAtMs = expiresAtMs;
        }
    }

    public readonly struct CounterEntryClosed : IBattleEvent
    {
        public string EventId => "counter.entry_closed";
        public int SchemaVersion => 1;
        public SceneEventContext Context { get; }
        public long OccurredAtCombatClockMs { get; }
        public string ResolutionId { get; }
        public CounterCloseReason CloseReason { get; }

        public CounterEntryClosed(SceneEventContext ctx, long clockMs, string resolutionId, CounterCloseReason reason)
        {
            Context = ctx;
            OccurredAtCombatClockMs = clockMs;
            ResolutionId = resolutionId;
            CloseReason = reason;
        }
    }

    public enum CounterCloseReason : byte
    {
        Consumed = 0,
        Expired = 1,
        Cancelled = 2,
    }
}
