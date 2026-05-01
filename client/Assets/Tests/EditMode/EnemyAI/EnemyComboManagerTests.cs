using System.Collections.Generic;
using Game.Gameplay.EnemyAI;
using NUnit.Framework;

namespace Game.Tests.EditMode.EnemyAI
{
    /// <summary>
    /// ADR-0008 / story-003：连击中断、重叠重排/取消。
    /// </summary>
    public sealed class EnemyComboManagerTests
    {
        private EnemyComboManager _mgr;

        [SetUp]
        public void SetUp()
        {
            _mgr = new EnemyComboManager();
        }

        [Test]
        public void InterruptOnPerfect_CancelsSubsequentSegments()
        {
            _mgr.ScheduleSegments(new List<ScheduledSegment>
            {
                new ScheduledSegment("s0", 0, 0, 1),
                new ScheduledSegment("s1", 1, 700, 2),
                new ScheduledSegment("s2", 2, 1400, 3),
            });

            int removed = _mgr.InterruptCombo(0, ComboInterruptRule.InterruptOnPerfect);

            Assert.That(removed, Is.EqualTo(2));
            Assert.That(_mgr.PendingSegments.Count, Is.EqualTo(1));
            Assert.That(_mgr.PendingSegments[0].SegmentId, Is.EqualTo("s0"));
        }

        [Test]
        public void ContinueOnPerfect_DoesNotCancel()
        {
            _mgr.ScheduleSegments(new List<ScheduledSegment>
            {
                new ScheduledSegment("s0", 0, 0, 1),
                new ScheduledSegment("s1", 1, 700, 2),
            });

            int removed = _mgr.InterruptCombo(0, ComboInterruptRule.ContinueOnPerfect);

            Assert.That(removed, Is.EqualTo(0));
            Assert.That(_mgr.PendingSegments.Count, Is.EqualTo(2));
        }

        [Test]
        public void OverlapRejected_Reschedule_Success()
        {
            _mgr.ScheduleSegments(new List<ScheduledSegment>
            {
                new ScheduledSegment("s0", 0, 0, 1),
                new ScheduledSegment("s1", 1, 500, 2),
            });

            bool result = _mgr.HandleOverlapRejected("s1", 300, 3);

            Assert.That(result, Is.True);
            Assert.That(_mgr.PendingSegments[1].StartMs, Is.EqualTo(800));
            Assert.That(_mgr.PendingSegments[1].TimelineSequenceId, Is.EqualTo(3));
            Assert.That(_mgr.PendingSegments[1].RescheduleCount, Is.EqualTo(1));
        }

        [Test]
        public void OverlapRejected_MaxReschedule_Cancels()
        {
            _mgr.ScheduleSegments(new List<ScheduledSegment>
            {
                new ScheduledSegment("s0", 0, 0, 1),
                new ScheduledSegment("s1", 1, 500, 2, rescheduleCount: 1),
            });

            bool result = _mgr.HandleOverlapRejected("s1", 300, 3);

            Assert.That(result, Is.False, "Should cancel after max reschedule attempts");
            Assert.That(_mgr.PendingSegments.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveSegment_Works()
        {
            _mgr.ScheduleSegments(new List<ScheduledSegment>
            {
                new ScheduledSegment("s0", 0, 0, 1),
                new ScheduledSegment("s1", 1, 700, 2),
            });

            _mgr.RemoveSegment("s0");
            Assert.That(_mgr.PendingSegments.Count, Is.EqualTo(1));
            Assert.That(_mgr.PendingSegments[0].SegmentId, Is.EqualTo("s1"));
        }

        [Test]
        public void Clear_RemovesAll()
        {
            _mgr.ScheduleSegments(new List<ScheduledSegment>
            {
                new ScheduledSegment("s0", 0, 0, 1),
            });

            _mgr.Clear();
            Assert.That(_mgr.PendingSegments.Count, Is.EqualTo(0));
        }
    }
}
