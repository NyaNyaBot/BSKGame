using System;
using System.Collections.Generic;
using Game.Gameplay.Parry;

namespace Game.Gameplay.EnemyAI
{
    /// <summary>
    /// 从攻击模式生成 AttackSegmentTimelineSeed（ADR-0008 / TR-enemy-002）。
    /// 将相对延迟绝对化为 CombatClockMs 域。
    /// </summary>
    public static class AttackSegmentFactory
    {
        /// <summary>
        /// 生成攻击段的绝对时间 seed。
        /// absolute_time_ms = segment_start_ms + relative_delay_ms
        /// </summary>
        public static AttackSegmentTimelineSeed CreateSeed(
            in AttackSegmentDefinition segment,
            long segmentStartMs)
        {
            return new AttackSegmentTimelineSeed(
                windupStartMs: segmentStartMs + segment.RelativeWindupDelayMs,
                visualImpactMs: segmentStartMs + segment.RelativeImpactDelayMs,
                damageCommitMs: segmentStartMs + segment.RelativeDamageCommitDelayMs,
                recoveryEndMs: segmentStartMs + segment.RelativeRecoveryEndDelayMs);
        }

        /// <summary>
        /// 从多段定义批量生成 seed 列表，每段的 startMs 串联上一段的 recoveryEndMs。
        /// </summary>
        public static List<AttackSegmentTimelineSeed> CreateComboSeeds(
            IReadOnlyList<AttackSegmentDefinition> segments,
            long firstSegmentStartMs)
        {
            var seeds = new List<AttackSegmentTimelineSeed>(segments.Count);
            long currentStart = firstSegmentStartMs;

            for (int i = 0; i < segments.Count; i++)
            {
                var seed = CreateSeed(segments[i], currentStart);
                seeds.Add(seed);
                currentStart = seed.RecoveryEndMs;
            }

            return seeds;
        }
    }
}
