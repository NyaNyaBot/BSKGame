using System;
using System.Collections.Generic;
using Game.Gameplay.Parry;

namespace Game.Gameplay.Feedback
{
    /// <summary>
    /// 反馈 profile 查表服务（ADR-0009 / TR-fx-001, TR-fx-002）。
    /// 将战斗事件映射为 FeedbackRequest + 可选 HitStopRequest。
    /// 幂等键: sourceEventId。
    /// </summary>
    public sealed class FeedbackProfileLookup
    {
        public const int PerfectHitStopMs = 90;
        public const int NormalHitStopMs = 35;

        private readonly HashSet<string> _processedEventIds = new HashSet<string>(StringComparer.Ordinal);
        private QualityTier _qualityTier = QualityTier.High;

        public QualityTier CurrentTier => _qualityTier;

        public void SetQualityTier(QualityTier tier) => _qualityTier = tier;

        /// <summary>
        /// 从弹反结算结果构建反馈和 hit stop。
        /// 返回 null 表示已处理过（幂等）。
        /// </summary>
        public FeedbackBundle? BuildFromParryResolution(string resolutionId, ParryGrade grade, string targetInstanceId)
        {
            if (!_processedEventIds.Add(resolutionId))
                return null;

            FeedbackKind kind;
            FeedbackIntensity intensity;
            int hitStopMs;

            switch (grade)
            {
                case ParryGrade.PerfectParry:
                    kind = FeedbackKind.PerfectParry;
                    intensity = FeedbackIntensity.Critical;
                    hitStopMs = _qualityTier >= QualityTier.Medium ? PerfectHitStopMs : 0;
                    break;
                case ParryGrade.NormalParry:
                    kind = FeedbackKind.NormalParry;
                    intensity = FeedbackIntensity.High;
                    hitStopMs = _qualityTier >= QualityTier.High ? NormalHitStopMs : 0;
                    break;
                default:
                    kind = FeedbackKind.FailedParry;
                    intensity = FeedbackIntensity.Medium;
                    hitStopMs = 0;
                    break;
            }

            var feedback = new FeedbackRequest(resolutionId, kind, intensity, targetInstanceId);
            HitStopRequest? hitStop = hitStopMs > 0
                ? new HitStopRequest(resolutionId, hitStopMs, intensity)
                : (HitStopRequest?)null;

            return new FeedbackBundle(feedback, hitStop);
        }

        public FeedbackRequest? BuildDamageReceived(string requestId, string targetInstanceId)
        {
            if (!_processedEventIds.Add($"dmg_{requestId}"))
                return null;

            return new FeedbackRequest($"dmg_{requestId}", FeedbackKind.DamageReceived, FeedbackIntensity.Medium, targetInstanceId);
        }

        public FeedbackRequest? BuildDefeated(string instanceId, string requestId)
        {
            if (!_processedEventIds.Add($"defeated_{instanceId}"))
                return null;

            return new FeedbackRequest($"defeated_{instanceId}", FeedbackKind.CharacterDefeated,
                FeedbackIntensity.Critical, instanceId);
        }

        public void Clear() => _processedEventIds.Clear();
    }

    public readonly struct FeedbackBundle
    {
        public FeedbackRequest Feedback { get; }
        public HitStopRequest? HitStop { get; }

        public FeedbackBundle(FeedbackRequest feedback, HitStopRequest? hitStop)
        {
            Feedback = feedback;
            HitStop = hitStop;
        }
    }

    public enum QualityTier : byte
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }
}
