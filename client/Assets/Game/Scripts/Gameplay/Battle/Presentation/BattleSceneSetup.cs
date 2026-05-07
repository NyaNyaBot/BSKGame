using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// 战斗场景初始化组装器。
    /// 挂在战斗场景根GameObject上，负责引用场景中已配置好的各个组件并连接到BattleFlowController。
    /// </summary>
    public class BattleSceneSetup : MonoBehaviour
    {
        [Header("Presenters")]
        [SerializeField] private BattleCharacterPresenter _playerPresenter;
        [SerializeField] private BattleCharacterPresenter _enemyPresenter;

        [Header("Systems")]
        [SerializeField] private BattlePresenterManager _presenterManager;
        [SerializeField] private BattleCameraController _cameraController;
        [SerializeField] private BattleVfxController _vfxController;

        [Header("Fallback VFX (auto-create if no prefabs assigned)")]
        [SerializeField] private bool _useFactoryVfx = true;

        public BattleCharacterPresenter PlayerPresenter => _playerPresenter;
        public BattleCharacterPresenter EnemyPresenter => _enemyPresenter;
        public BattlePresenterManager PresenterManager => _presenterManager;
        public BattleCameraController CameraController => _cameraController;
        public BattleVfxController VfxController => _vfxController;

        public void InitializePresentation(Game.Gameplay.Events.IBattleEventBus bus, string playerInstanceId, string enemyInstanceId)
        {
            if (_useFactoryVfx && _vfxController == null)
                SetupFallbackVfx();

            if (_cameraController != null && _playerPresenter != null && _enemyPresenter != null)
                _cameraController.Initialize(_playerPresenter.transform, _enemyPresenter.transform);

            if (_presenterManager != null)
            {
                _presenterManager.Initialize(bus, _cameraController, _vfxController);

                if (_playerPresenter != null)
                    _presenterManager.RegisterPresenter(playerInstanceId, _playerPresenter);
                if (_enemyPresenter != null)
                    _presenterManager.RegisterPresenter(enemyInstanceId, _enemyPresenter);
            }

            Log.Info("[BattleSceneSetup] Presentation initialized. Player={0} Enemy={1}", playerInstanceId, enemyInstanceId);
        }

        private void SetupFallbackVfx()
        {
            var vfxGo = new GameObject("BattleVfxController_Fallback");
            vfxGo.transform.SetParent(transform, false);
            _vfxController = vfxGo.AddComponent<BattleVfxController>();
        }
    }
}
