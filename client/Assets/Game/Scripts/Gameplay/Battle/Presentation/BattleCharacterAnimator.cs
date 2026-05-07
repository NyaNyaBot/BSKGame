using System;
using Animancer;
using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// 3层Animancer动画控制器：FullBody(0) / UpperBody(1) / Additive(2)。
    /// 挂载在角色预制体上，由BattleCharacterPresenter驱动。
    /// </summary>
    public class BattleCharacterAnimator : MonoBehaviour
    {
        [Header("Animancer")]
        [SerializeField] private AnimancerComponent _animancer;

        [Header("Masks")]
        [SerializeField] private AvatarMask _upperBodyMask;

        [Header("FullBody Clips")]
        [SerializeField] private ClipTransition _idle;
        [SerializeField] private ClipTransition _walk;
        [SerializeField] private ClipTransition _run;
        [SerializeField] private ClipTransition _death;

        [Header("UpperBody Clips")]
        [SerializeField] private ClipTransition _attack;
        [SerializeField] private ClipTransition _block;
        [SerializeField] private ClipTransition _hitReaction;

        [Header("Additive Clips")]
        [SerializeField] private ClipTransition _breathIdle;

        private AnimancerLayer _fullBodyLayer;
        private AnimancerLayer _upperBodyLayer;
        private AnimancerLayer _additiveLayer;

        public event Action OnAttackAnimEnd;
        public event Action OnDeathAnimEnd;
        public event Action OnHitReactionEnd;

        private void Awake()
        {
            if (_animancer == null)
                _animancer = GetComponentInChildren<AnimancerComponent>();

            InitializeLayers();
        }

        private void InitializeLayers()
        {
            _fullBodyLayer = _animancer.Layers[0];
            _upperBodyLayer = _animancer.Layers[1];
            _additiveLayer = _animancer.Layers[2];

            if (_upperBodyMask != null)
                _upperBodyLayer.SetMask(_upperBodyMask);

            _additiveLayer.IsAdditive = true;
        }

        public void PlayIdle()
        {
            _fullBodyLayer.Play(_idle);
            _additiveLayer.StartFade(0f, 0.2f);
        }

        public void PlayWalk()
        {
            _fullBodyLayer.Play(_walk);
        }

        public void PlayRun()
        {
            _fullBodyLayer.Play(_run);
        }

        public float PlayAttack()
        {
            var state = _fullBodyLayer.Play(_attack);

            state.Events.OnEnd = () =>
            {
                _fullBodyLayer.Play(_idle);
                OnAttackAnimEnd?.Invoke();
            };

            return state.Length;
        }

        public float PlayBlock()
        {
            var state = _fullBodyLayer.Play(_block);
            return state.Length;
        }

        public void StopBlock()
        {
            _fullBodyLayer.Play(_idle);
        }

        public float PlayHitReaction()
        {
            var state = _fullBodyLayer.Play(_hitReaction);

            state.Events.OnEnd = () =>
            {
                _fullBodyLayer.Play(_idle);
                OnHitReactionEnd?.Invoke();
            };

            return state.Length;
        }

        public float PlayDeath()
        {
            var state = _fullBodyLayer.Play(_death);
            state.Events.OnEnd = () =>
            {
                OnDeathAnimEnd?.Invoke();
            };

            return state.Length;
        }

        public void SetTimeScale(float scale)
        {
            _animancer.Playable.Speed = scale;
        }

        private void OnDestroy()
        {
            OnAttackAnimEnd = null;
            OnDeathAnimEnd = null;
            OnHitReactionEnd = null;
        }
    }
}
