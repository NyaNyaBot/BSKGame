using System;

namespace Game.Gameplay.SceneLifecycle
{
    /// <summary>
    /// 默认内存内实现；Unity 侧由场景管理在加载/卸载钩子中调用。
    /// </summary>
    public sealed class SceneContextService : ISceneContextService
    {
        private string _sceneContextId = string.Empty;
        private int _sceneVersion;
        private SceneKind _sceneKind;
        private bool _isTransitioning;

        private string? _battleContextId;
        private int _battleVersion;

        public bool IsInputFrozen => _isTransitioning;

        public SceneContext CurrentScene => new SceneContext(_sceneContextId, _sceneVersion, _sceneKind, _isTransitioning);

        public void EnterScene(SceneKind kind)
        {
            _sceneContextId = Guid.NewGuid().ToString("N");
            _sceneVersion++;
            _sceneKind = kind;
            _isTransitioning = false;
            ClearBattleLocked();
        }

        public void BeginSceneTransition(SceneTransitionReason reason)
        {
            _ = reason;
            if (string.IsNullOrEmpty(_sceneContextId))
            {
                return;
            }

            _sceneVersion++;
            _isTransitioning = true;
        }

        public void CompleteSceneTransition()
        {
            _isTransitioning = false;
        }

        public bool TryGetActiveBattle(out BattleContext battle)
        {
            if (string.IsNullOrEmpty(_battleContextId))
            {
                battle = default;
                return false;
            }

            battle = new BattleContext(_battleContextId, _battleVersion, _sceneContextId, _sceneVersion);
            return true;
        }

        public BattleContext CreateBattleContext(in StartBattleRequest request)
        {
            _ = request;
            if (string.IsNullOrEmpty(_sceneContextId))
            {
                throw new InvalidOperationException("Cannot create battle context before EnterScene.");
            }

            if (!string.IsNullOrEmpty(_battleContextId))
            {
                DisposeBattle(_battleContextId, BattleDisposeReason.BattleRestart);
            }

            _battleContextId = Guid.NewGuid().ToString("N");
            _battleVersion = 1;
            return new BattleContext(_battleContextId, _battleVersion, _sceneContextId, _sceneVersion);
        }

        public void DisposeBattle(string battleContextId, BattleDisposeReason reason)
        {
            _ = reason;
            if (string.IsNullOrEmpty(_battleContextId) || !string.Equals(_battleContextId, battleContextId, StringComparison.Ordinal))
            {
                return;
            }

            _battleVersion++;
            _battleContextId = null;
        }

        public bool IsCurrent(in SceneEventContext context)
        {
            if (string.IsNullOrEmpty(_sceneContextId))
            {
                return false;
            }

            if (!string.Equals(context.SceneContextId, _sceneContextId, StringComparison.Ordinal) ||
                context.SceneVersion != _sceneVersion)
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.BattleContextId))
            {
                return true;
            }

            if (string.IsNullOrEmpty(_battleContextId))
            {
                return false;
            }

            return string.Equals(context.BattleContextId, _battleContextId, StringComparison.Ordinal) &&
                   context.BattleVersion == _battleVersion;
        }

        private void ClearBattleLocked()
        {
            _battleContextId = null;
            _battleVersion = 0;
        }
    }
}
