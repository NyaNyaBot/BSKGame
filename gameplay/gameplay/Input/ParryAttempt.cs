using Game.Gameplay.SceneLifecycle;

namespace Game.Gameplay.Input
{
    /// <summary>
    /// 结构化弹反尝试（ADR-0006 / ADR-0007）。
    /// 从 PointerDown 采样到此结构体，供 IParryResolver 消费。
    /// 纯 DTO，无 Unity 引用。
    /// </summary>
    public readonly struct ParryAttempt
    {
        /// <summary>
        /// 映射后的战斗逻辑时间戳（ms）。
        /// </summary>
        public long BattleTimestampMs { get; }

        /// <summary>
        /// 桥接延迟（ms），诊断用。
        /// </summary>
        public long BridgeDelayMs { get; }

        /// <summary>
        /// 是否使用了 fallback 路径。
        /// </summary>
        public bool IsFallback { get; }

        /// <summary>
        /// 命中的触区 ID（从 InputHitAreaRegistry 查询）。
        /// </summary>
        public string HitAreaId { get; }

        /// <summary>
        /// 触区状态版本，用于过滤过期命中。
        /// </summary>
        public int StateVersion { get; }

        /// <summary>
        /// 事件发生时的场景/战斗上下文快照。
        /// </summary>
        public SceneEventContext Context { get; }

        public ParryAttempt(
            long battleTimestampMs,
            long bridgeDelayMs,
            bool isFallback,
            string hitAreaId,
            int stateVersion,
            SceneEventContext context)
        {
            BattleTimestampMs = battleTimestampMs;
            BridgeDelayMs = bridgeDelayMs;
            IsFallback = isFallback;
            HitAreaId = hitAreaId;
            StateVersion = stateVersion;
            Context = context;
        }
    }
}
