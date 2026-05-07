using Cinemachine;
using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// 战斗相机控制器。固定等距俯视 + 轻微冲击效果。
    /// Closeup 相机暂时禁用以避免运动畸变，仅保留 impulse 反馈。
    /// </summary>
    public class BattleCameraController : MonoBehaviour
    {
        [Header("Virtual Cameras")]
        [SerializeField] private CinemachineVirtualCamera _defaultVCam;
        [SerializeField] private CinemachineVirtualCamera _closeupVCam;

        [Header("Target Group")]
        [SerializeField] private CinemachineTargetGroup _targetGroup;

        [Header("Impulse")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Settings")]
        [SerializeField] private int _defaultPriority = 10;
        [SerializeField] private float _closeupHoldTime = 1.0f;
        [SerializeField] private bool _enableCloseup = false;

        private float _closeupTimer;
        private bool _closeupActive;

        public void Initialize(Transform player, Transform enemy)
        {
            if (_targetGroup != null)
            {
                _targetGroup.AddMember(player, 1f, 1f);
                _targetGroup.AddMember(enemy, 1f, 1f);
            }

            if (_defaultVCam != null)
            {
                var groupTransform = _targetGroup != null ? _targetGroup.Transform : player;
                _defaultVCam.Follow = groupTransform;
                _defaultVCam.LookAt = groupTransform;
                _defaultVCam.Priority = _defaultPriority;
            }

            if (_closeupVCam != null)
                _closeupVCam.Priority = 0;
        }

        public void SwitchToCloseup(Transform attacker, Transform target)
        {
            if (!_enableCloseup || _closeupVCam == null) return;

            var midpoint = (attacker.position + target.position) * 0.5f;
            _closeupVCam.Follow = _targetGroup != null ? _targetGroup.Transform : attacker;
            _closeupVCam.LookAt = _targetGroup != null ? _targetGroup.Transform : target;
            _closeupVCam.Priority = _defaultPriority + 5;
            _closeupActive = true;
            _closeupTimer = _closeupHoldTime;
        }

        public void SwitchToDefault()
        {
            if (_closeupVCam == null) return;
            _closeupVCam.Priority = 0;
            _closeupActive = false;
        }

        public void TriggerHitImpulse(float force = 0.3f)
        {
            if (_impulseSource != null)
                _impulseSource.GenerateImpulse(force);
        }

        private void Update()
        {
            if (!_closeupActive) return;

            _closeupTimer -= Time.deltaTime;
            if (_closeupTimer <= 0f)
                SwitchToDefault();
        }
    }
}
