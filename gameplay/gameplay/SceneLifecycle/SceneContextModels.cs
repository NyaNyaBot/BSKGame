namespace Game.Gameplay.SceneLifecycle
{
    /// <summary>
    /// 当前可玩场景权威上下文（ADR-0001）。
    /// </summary>
    public readonly struct SceneContext
    {
        public SceneContext(string sceneContextId, int sceneVersion, SceneKind sceneKind, bool isTransitioning)
        {
            SceneContextId = sceneContextId;
            SceneVersion = sceneVersion;
            SceneKind = sceneKind;
            IsTransitioning = isTransitioning;
        }

        public string SceneContextId { get; }
        public int SceneVersion { get; }
        public SceneKind SceneKind { get; }
        public bool IsTransitioning { get; }
    }

    /// <summary>
    /// 单场战斗嵌套上下文（ADR-0001）。
    /// </summary>
    public readonly struct BattleContext
    {
        public BattleContext(string battleContextId, int battleVersion, string sceneContextId, int sceneVersion)
        {
            BattleContextId = battleContextId;
            BattleVersion = battleVersion;
            SceneContextId = sceneContextId;
            SceneVersion = sceneVersion;
        }

        public string BattleContextId { get; }
        public int BattleVersion { get; }
        public string SceneContextId { get; }
        public int SceneVersion { get; }

        public SceneEventContext ToEventContext()
        {
            return new SceneEventContext(SceneContextId, SceneVersion, BattleContextId, BattleVersion);
        }
    }

    /// <summary>
    /// 跨系统战斗事件必须携带的上下文快照（ADR-0001 / ADR-0002 对齐字段名）。
    /// </summary>
    public readonly struct SceneEventContext
    {
        public SceneEventContext(string sceneContextId, int sceneVersion, string battleContextId, int battleVersion)
        {
            SceneContextId = sceneContextId;
            SceneVersion = sceneVersion;
            BattleContextId = battleContextId;
            BattleVersion = battleVersion;
        }

        public string SceneContextId { get; }
        public int SceneVersion { get; }
        public string BattleContextId { get; }
        public int BattleVersion { get; }

        /// <summary>
        /// 无战斗作用域的事件（仅校验场景部分）。
        /// </summary>
        public static SceneEventContext ForSceneOnly(in SceneContext scene)
        {
            return new SceneEventContext(scene.SceneContextId, scene.SceneVersion, string.Empty, 0);
        }
    }
}
