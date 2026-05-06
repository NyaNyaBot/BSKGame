using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay.Damage;
using Game.Gameplay.Events;
using Game.Gameplay.Integration;
using Game.Gameplay.TurnManager;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// 战斗表现管理器。
    /// 订阅BattleEventBus，将逻辑事件翻译为视觉动画序列（攻击→受击→死亡）。
    /// 通过协程排队动画，确保表现不重叠。
    /// </summary>
    public class BattlePresenterManager : MonoBehaviour
    {
        private IBattleEventBus _bus;
        private readonly Dictionary<string, BattleCharacterPresenter> _presenters = new();
        private readonly Queue<Action> _presentationQueue = new();
        private bool _isPlaying;

        private BattleCameraController _cameraController;
        private BattleVfxController _vfxController;

        private IDisposable _subDamage;
        private IDisposable _subDeath;
        private IDisposable _subPhase;

        public void Initialize(IBattleEventBus bus, BattleCameraController cameraController, BattleVfxController vfxController)
        {
            _bus = bus;
            _cameraController = cameraController;
            _vfxController = vfxController;

            _subDamage = _bus.Subscribe<DamageApplied>(OnDamageApplied);
            _subDeath = _bus.Subscribe<CharacterDefeated>(OnCharacterDefeated);
            _subPhase = _bus.Subscribe<BattleInputPhaseChanged>(OnPhaseChanged);
        }

        public void RegisterPresenter(string instanceId, BattleCharacterPresenter presenter)
        {
            _presenters[instanceId] = presenter;
            presenter.Initialize(instanceId);
        }

        public BattleCharacterPresenter GetPresenter(string instanceId)
        {
            _presenters.TryGetValue(instanceId, out var p);
            return p;
        }

        private void OnDamageApplied(DamageApplied evt)
        {
            EnqueuePresentation(() => StartCoroutine(DamageSequence(evt)));
        }

        private void OnCharacterDefeated(CharacterDefeated evt)
        {
            EnqueuePresentation(() => StartCoroutine(DeathSequence(evt)));
        }

        private void OnPhaseChanged(BattleInputPhaseChanged evt)
        {
            Log.Info("[Presenter] Phase: {0} → {1}", evt.PreviousPhase, evt.CurrentPhase);
        }

        private IEnumerator DamageSequence(DamageApplied evt)
        {
            _isPlaying = true;

            var attackerId = FindAttackerFor(evt.TargetInstanceId);
            var attacker = attackerId != null ? GetPresenter(attackerId) : null;
            var target = GetPresenter(evt.TargetInstanceId);

            // 攻击动画
            float attackDuration = 0f;
            if (attacker != null)
            {
                if (_cameraController != null)
                    _cameraController.SwitchToCloseup(attacker.transform, target != null ? target.transform : attacker.transform);

                attackDuration = attacker.PlayAttack();
                yield return new WaitForSeconds(attackDuration * 0.4f);
            }

            // 受击动画 + VFX
            if (target != null)
            {
                float hitDuration = target.PlayHitReaction();

                if (_vfxController != null)
                    _vfxController.PlayHitEffect(target.HitVfxPoint != null ? target.HitVfxPoint.position : target.transform.position);

                if (_cameraController != null)
                    _cameraController.TriggerHitImpulse(0.4f);

                yield return new WaitForSeconds(hitDuration);
            }
            else
            {
                yield return new WaitForSeconds(attackDuration * 0.6f);
            }

            if (_cameraController != null)
                _cameraController.SwitchToDefault();

            _isPlaying = false;
            ProcessNextPresentation();
        }

        private IEnumerator DeathSequence(CharacterDefeated evt)
        {
            _isPlaying = true;

            var target = GetPresenter(evt.InstanceId);
            if (target != null)
            {
                float duration = target.PlayDeath();

                if (_vfxController != null)
                    _vfxController.PlayDeathEffect(target.transform.position);

                yield return new WaitForSeconds(duration);
            }

            _isPlaying = false;
            ProcessNextPresentation();
        }

        private void EnqueuePresentation(Action action)
        {
            _presentationQueue.Enqueue(action);
            if (!_isPlaying)
                ProcessNextPresentation();
        }

        private void ProcessNextPresentation()
        {
            if (_presentationQueue.Count == 0) return;
            var next = _presentationQueue.Dequeue();
            next?.Invoke();
        }

        private string _lastAttackerId;

        public void SetLastAttacker(string instanceId)
        {
            _lastAttackerId = instanceId;
        }

        private string FindAttackerFor(string targetId)
        {
            return _lastAttackerId;
        }

        private void OnDestroy()
        {
            _subDamage?.Dispose();
            _subDeath?.Dispose();
            _subPhase?.Dispose();
        }
    }
}
