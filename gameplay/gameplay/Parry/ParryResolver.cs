using System;
using System.Collections.Generic;
using Game.Gameplay.Input;
using SysMath = System.Math;

namespace Game.Gameplay.Parry
{
    /// <summary>
    /// 弹反结算器（ADR-0007 / TR-parry-002）。
    /// 从 FrozenAttackSegmentTimeline + ParryAttempt 计算 grade。
    /// </summary>
    public sealed class ParryResolver
    {
        public const long DefaultInputBufferMs = 100;

        private readonly Dictionary<string, ParryResolution> _resolved =
            new Dictionary<string, ParryResolution>(StringComparer.Ordinal);
        private int _nextResolutionId;

        /// <summary>
        /// 结算一个弹反尝试。
        /// </summary>
        public ParryResolution ResolveAttempt(
            in FrozenAttackSegmentTimeline timeline,
            string attackSegmentId,
            int timelineSequenceId,
            in ParryAttempt attempt)
        {
            var key = MakeKey(attackSegmentId, timelineSequenceId);
            if (_resolved.TryGetValue(key, out var cached))
                return cached;

            long effectiveTs = attempt.BattleTimestampMs;
            if (effectiveTs < timeline.WindowOpenMs &&
                effectiveTs >= timeline.WindowOpenMs - DefaultInputBufferMs)
            {
                effectiveTs = timeline.WindowOpenMs;
            }

            long offset = effectiveTs - timeline.ParryCenterMs;

            ParryGrade grade;
            int damageMultBp;
            EchoIntent echo;

            if (effectiveTs >= timeline.PerfectStartMs && effectiveTs <= timeline.PerfectEndMs)
            {
                grade = ParryGrade.PerfectParry;
                damageMultBp = 0;
                echo = EchoIntent.High;
            }
            else if (effectiveTs >= timeline.WindowOpenMs && effectiveTs <= timeline.NormalEndMs)
            {
                grade = ParryGrade.NormalParry;
                damageMultBp = 5000;
                echo = EchoIntent.None;
            }
            else
            {
                grade = ParryGrade.FailedParry;
                damageMultBp = 10000;
                echo = EchoIntent.None;
            }

            var resolution = new ParryResolution(
                GenerateResolutionId(),
                attackSegmentId,
                timelineSequenceId,
                grade,
                offset,
                damageMultBp,
                echo);

            _resolved[key] = resolution;
            return resolution;
        }

        /// <summary>
        /// 窗口关闭且无输入时调用。
        /// </summary>
        public ParryResolution ResolveNoInput(
            string attackSegmentId,
            int timelineSequenceId)
        {
            var key = MakeKey(attackSegmentId, timelineSequenceId);
            if (_resolved.TryGetValue(key, out var cached))
                return cached;

            var resolution = new ParryResolution(
                GenerateResolutionId(),
                attackSegmentId,
                timelineSequenceId,
                ParryGrade.FailedParry,
                0,
                10000,
                EchoIntent.None,
                ParryFailureReason.NoInput);

            _resolved[key] = resolution;
            return resolution;
        }

        public bool HasResolution(string attackSegmentId, int timelineSequenceId)
        {
            return _resolved.ContainsKey(MakeKey(attackSegmentId, timelineSequenceId));
        }

        public void Clear()
        {
            _resolved.Clear();
            _nextResolutionId = 0;
        }

        private string GenerateResolutionId()
        {
            _nextResolutionId++;
            return $"parry_res_{_nextResolutionId}";
        }

        private static string MakeKey(string segId, int seqId) => $"{segId}:{seqId}";
    }
}
