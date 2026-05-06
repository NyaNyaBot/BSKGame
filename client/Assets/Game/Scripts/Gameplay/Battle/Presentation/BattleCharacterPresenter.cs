using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// 单个战斗角色的表现层代理。
    /// 持有Animator + VFX挂点，由BattlePresenterManager调度。
    /// </summary>
    public class BattleCharacterPresenter : MonoBehaviour
    {
        [SerializeField] private BattleCharacterAnimator _animator;
        [SerializeField] private Transform _hitVfxPoint;
        [SerializeField] private Transform _weaponVfxPoint;

        public BattleCharacterAnimator Animator => _animator;
        public Transform HitVfxPoint => _hitVfxPoint;
        public Transform WeaponVfxPoint => _weaponVfxPoint;
        public string InstanceId { get; private set; }

        public void Initialize(string instanceId)
        {
            InstanceId = instanceId;
            if (_animator == null)
                _animator = GetComponentInChildren<BattleCharacterAnimator>();

            _animator.PlayIdle();
        }

        public float PlayAttack()
        {
            return _animator.PlayAttack();
        }

        public float PlayHitReaction()
        {
            return _animator.PlayHitReaction();
        }

        public float PlayDeath()
        {
            return _animator.PlayDeath();
        }

        public float PlayBlock()
        {
            return _animator.PlayBlock();
        }

        public void StopBlock()
        {
            _animator.StopBlock();
        }

        public void PlayIdle()
        {
            _animator.PlayIdle();
        }
    }
}
