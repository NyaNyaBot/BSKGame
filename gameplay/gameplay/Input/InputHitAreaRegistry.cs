using System;
using System.Collections.Generic;

namespace Game.Gameplay.Input
{
    /// <summary>
    /// 战斗输入触区注册表（ADR-0006）。
    /// 拥有注册区域、优先级、遮挡、stateVersion、重复过滤。
    /// </summary>
    public sealed class InputHitAreaRegistry
    {
        public const int PriorityCounter = 300;
        public const int PriorityModalPause = 250;
        public const int PriorityStandardUIAction = 200;
        public const int PriorityParry = 100;
        public const int PriorityBackground = 0;

        public const int DefaultMaxAreas = 32;

        private readonly List<HitAreaEntry> _areas = new List<HitAreaEntry>();
        private bool _dirty = true;

        public int MaxAreas { get; set; } = DefaultMaxAreas;

        public void Register(HitAreaRegistration registration)
        {
            if (string.IsNullOrEmpty(registration.AreaId))
                throw new ArgumentException("AreaId must not be null or empty.", nameof(registration));

            if (_areas.Count >= MaxAreas)
                throw new InvalidOperationException(
                    $"InputHitAreaRegistry: max area count ({MaxAreas}) exceeded.");

            for (int i = 0; i < _areas.Count; i++)
            {
                if (_areas[i].AreaId == registration.AreaId)
                    throw new InvalidOperationException(
                        $"Area '{registration.AreaId}' is already registered. Unregister first.");
            }

            _areas.Add(new HitAreaEntry(registration));
            _dirty = true;
        }

        public void Unregister(string areaId)
        {
            for (int i = _areas.Count - 1; i >= 0; i--)
            {
                if (_areas[i].AreaId == areaId)
                {
                    _areas.RemoveAt(i);
                    _dirty = true;
                    return;
                }
            }
        }

        public void UpdateStateVersion(string areaId, int newVersion)
        {
            for (int i = 0; i < _areas.Count; i++)
            {
                if (_areas[i].AreaId == areaId)
                {
                    _areas[i] = _areas[i].WithStateVersion(newVersion);
                    return;
                }
            }
        }

        /// <summary>
        /// 命中测试：返回优先级最高的有效区域，或 null。
        /// 尊重 BlocksUnderlying 和 stateVersion。
        /// </summary>
        public HitTestResult? HitTest(int expectedStateVersion)
        {
            EnsureSorted();

            for (int i = 0; i < _areas.Count; i++)
            {
                var area = _areas[i];

                if (!area.IsEnabled)
                    continue;

                if (area.StateVersion != expectedStateVersion)
                    continue;

                return new HitTestResult(area.AreaId, area.Priority, area.StateVersion);
            }

            return null;
        }

        /// <summary>
        /// 命中测试（不检查 stateVersion，返回最高优先级有效区域）。
        /// </summary>
        public HitTestResult? HitTest()
        {
            EnsureSorted();

            for (int i = 0; i < _areas.Count; i++)
            {
                var area = _areas[i];
                if (!area.IsEnabled) continue;

                return new HitTestResult(area.AreaId, area.Priority, area.StateVersion);
            }

            return null;
        }

        public void SetEnabled(string areaId, bool enabled)
        {
            for (int i = 0; i < _areas.Count; i++)
            {
                if (_areas[i].AreaId == areaId)
                {
                    _areas[i] = _areas[i].WithEnabled(enabled);
                    return;
                }
            }
        }

        public int RegisteredCount => _areas.Count;

        public void Clear()
        {
            _areas.Clear();
            _dirty = true;
        }

        private void EnsureSorted()
        {
            if (!_dirty) return;
            _areas.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            _dirty = false;
        }
    }

    public readonly struct HitAreaRegistration
    {
        public string AreaId { get; }
        public int Priority { get; }
        public bool BlocksUnderlying { get; }

        public HitAreaRegistration(string areaId, int priority, bool blocksUnderlying = false)
        {
            AreaId = areaId;
            Priority = priority;
            BlocksUnderlying = blocksUnderlying;
        }
    }

    public struct HitAreaEntry
    {
        public string AreaId { get; }
        public int Priority { get; }
        public bool BlocksUnderlying { get; }
        public int StateVersion { get; private set; }
        public bool IsEnabled { get; private set; }

        public HitAreaEntry(HitAreaRegistration reg)
        {
            AreaId = reg.AreaId;
            Priority = reg.Priority;
            BlocksUnderlying = reg.BlocksUnderlying;
            StateVersion = 1;
            IsEnabled = true;
        }

        public HitAreaEntry WithStateVersion(int version)
        {
            var copy = this;
            copy.StateVersion = version;
            return copy;
        }

        public HitAreaEntry WithEnabled(bool enabled)
        {
            var copy = this;
            copy.IsEnabled = enabled;
            return copy;
        }
    }

    public readonly struct HitTestResult
    {
        public string AreaId { get; }
        public int Priority { get; }
        public int StateVersion { get; }

        public HitTestResult(string areaId, int priority, int stateVersion)
        {
            AreaId = areaId;
            Priority = priority;
            StateVersion = stateVersion;
        }
    }
}
