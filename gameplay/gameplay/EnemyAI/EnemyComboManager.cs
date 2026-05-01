using System.Collections.Generic;

namespace Game.Gameplay.EnemyAI
{
    /// <summary>
    /// 敌人连击管理器（ADR-0008 / TR-enemy-003）。
    /// 响应 PerfectParry 中断后续段，处理重叠重排/取消。
    /// </summary>
    public sealed class EnemyComboManager
    {
        public const int MaxRescheduleAttempts = 1;

        private readonly List<ScheduledSegment> _pendingSegments = new List<ScheduledSegment>();

        public IReadOnlyList<ScheduledSegment> PendingSegments => _pendingSegments;

        public void ScheduleSegments(IReadOnlyList<ScheduledSegment> segments)
        {
            _pendingSegments.Clear();
            _pendingSegments.AddRange(segments);
        }

        /// <summary>
        /// PerfectParry + InterruptOnPerfect 时取消所有剩余段。
        /// 返回被取消的段数。
        /// </summary>
        public int InterruptCombo(int afterComboIndex, ComboInterruptRule rule)
        {
            if (rule != ComboInterruptRule.InterruptOnPerfect)
                return 0;

            int removed = 0;
            for (int i = _pendingSegments.Count - 1; i >= 0; i--)
            {
                if (_pendingSegments[i].ComboIndex > afterComboIndex)
                {
                    _pendingSegments.RemoveAt(i);
                    removed++;
                }
            }
            return removed;
        }

        /// <summary>
        /// 重叠被拒绝时尝试重排。若重排仍冲突则取消。
        /// 返回 true 表示重排成功，false 表示已取消。
        /// </summary>
        public bool HandleOverlapRejected(
            string segmentId,
            long rescheduleDelayMs,
            int newTimelineSequenceId)
        {
            for (int i = 0; i < _pendingSegments.Count; i++)
            {
                if (_pendingSegments[i].SegmentId == segmentId)
                {
                    var seg = _pendingSegments[i];
                    if (seg.RescheduleCount >= MaxRescheduleAttempts)
                    {
                        _pendingSegments.RemoveAt(i);
                        return false;
                    }

                    _pendingSegments[i] = new ScheduledSegment(
                        seg.SegmentId,
                        seg.ComboIndex,
                        seg.StartMs + rescheduleDelayMs,
                        newTimelineSequenceId,
                        seg.RescheduleCount + 1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除已完成或过期的段。
        /// </summary>
        public void RemoveSegment(string segmentId)
        {
            for (int i = _pendingSegments.Count - 1; i >= 0; i--)
            {
                if (_pendingSegments[i].SegmentId == segmentId)
                {
                    _pendingSegments.RemoveAt(i);
                    return;
                }
            }
        }

        public void Clear() => _pendingSegments.Clear();
    }

    public struct ScheduledSegment
    {
        public string SegmentId { get; }
        public int ComboIndex { get; }
        public long StartMs { get; }
        public int TimelineSequenceId { get; }
        public int RescheduleCount { get; }

        public ScheduledSegment(string segmentId, int comboIndex, long startMs, int timelineSequenceId, int rescheduleCount = 0)
        {
            SegmentId = segmentId;
            ComboIndex = comboIndex;
            StartMs = startMs;
            TimelineSequenceId = timelineSequenceId;
            RescheduleCount = rescheduleCount;
        }
    }
}
