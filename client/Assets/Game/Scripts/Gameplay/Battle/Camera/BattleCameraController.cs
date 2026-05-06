using Cinemachine;
using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// 战斗相机控制器。管理默认等距俯视和战斗特写两个虚拟相机。
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
        [SerializeField] private int _closeupPriority = 20;
        [SerializeField] private float _closeupHoldTime = 1.5f;

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
                _defaultVCam.Follow = _targetGroup != null ? _targetGroup.Transform : player;
                _defaultVCam.LookAt = _targetGroup != null ? _targetGroup.Transform : player;
                _defaultVCam.Priority = _defaultPriority;
            }

            if (_closeupVCam != null)
                _closeupVCam.Priority = 0;
        }

        public void SwitchToCloseup(Transform attacker, Transform target)
        {
            if (_closeupVCam == null) return;

            _closeupVCam.Follow = attacker;
            _closeupVCam.LookAt = target;
            _closeupVCam.Priority = _closeupPriority;
            _closeupActive = true;
            _closeupTimer = _closeupHoldTime;
        }

        public void SwitchToDefault()
        {
            if (_closeupVCam == null) return;
            _closeupVCam.Priority = 0;
            _closeupActive = false;
        }

        public void TriggerHitImpulse(float force = 0.5f)
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
