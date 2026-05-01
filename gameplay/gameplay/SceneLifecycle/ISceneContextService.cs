namespace Game.Gameplay.SceneLifecycle
{
    /// <summary>
    /// 场景与战斗上下文单一真相源（ADR-0001）。
    /// </summary>
    public interface ISceneContextService
    {
        SceneContext CurrentScene { get; }

        bool TryGetActiveBattle(out BattleContext battle);

        bool IsCurrent(in SceneEventContext context);

        /// <summary>
        /// 进入可玩场景（含重试进入）：分配新 SceneContextId，并按 ADR 递增 SceneVersion。
        /// </summary>
        void EnterScene(SceneKind kind);

        /// <summary>
        /// 卸载 / 重入前：在 teardown 副作用完成前递增 SceneVersion，并标记 transitioning。
        /// </summary>
        void BeginSceneTransition(SceneTransitionReason reason);

        /// <summary>
        /// teardown 完成后的收尾：清除 transitioning（SceneContextId 是否在此时轮换由调用方策略决定，默认不变）。
        /// </summary>
        void CompleteSceneTransition();

        /// <summary>
        /// 战斗输入是否被冻结（场景切换期间为 true）。
        /// 输入适配器应在采集前检查此标志，拒绝新输入。
        /// </summary>
        bool IsInputFrozen { get; }

        BattleContext CreateBattleContext(in StartBattleRequest request);

        void DisposeBattle(string battleContextId, BattleDisposeReason reason);
    }
}
