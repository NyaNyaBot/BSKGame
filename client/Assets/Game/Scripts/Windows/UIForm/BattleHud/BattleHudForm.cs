using System;
using System.Collections.Generic;
using Game.Gameplay.Character;
using Game.Gameplay.Combat;
using Game.Gameplay.Damage;
using Game.Gameplay.Events;
using Game.Gameplay.Parry;
using Game.Gameplay.TurnManager;
using Game.Gameplay.Integration;
using Game.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    /// <summary>
    /// 战斗 HUD 窗口（ADR-0010 / TR-ui-001, TR-ui-002, TR-ui-003）。
    /// 事件驱动，不轮询可变 gameplay 对象。
    /// 场景卸载 / OnClose 时清理全部订阅。
    /// </summary>
    public class BattleHudForm : UGuiForm
    {
        [Header("Player Status")]
        public Slider PlayerHpBar;
        public Text PlayerHpText;

        [Header("Enemy Status")]
        public Slider EnemyHpBar;
        public Text EnemyHpText;

        [Header("Action")]
        public CanvasGroup ParryReadyIndicator;
        public CanvasGroup CounterReadyIndicator;

        [Header("Phase")]
        public Text PhaseText;

        [Header("Result")]
        public CanvasGroup ResultPanel;
        public Text ResultText;

        [Header("Debug Overlay")]
        public CanvasGroup DebugOverlay;
        public Text DebugClockText;
        public Text DebugPhaseText;
        public Text DebugSegmentIdText;

        private BattleHudViewModel _viewModel;
        private BattleEventBus _bus;
        private ICharacterReadModel _readModel;
        private ICombatClock _clock;

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        public BattleHudViewModel ViewModel => _viewModel;

        /// <summary>
        /// 当前活跃订阅数，用于验收测试（AC-2 清理）。
        /// </summary>
        public int ActiveSubscriptionCount => _subscriptions.Count;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _viewModel = new BattleHudViewModel();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (userData is BattleHudOpenData data)
            {
                _bus = data.Bus;
                _readModel = data.ReadModel;
                _clock = data.Clock;
            }

            if (_bus == null)
            {
                Log.Warning("BattleHudForm opened without BattleEventBus.");
                return;
            }

            SubscribeEvents();
            InitializeFromSnapshots();

            _viewModel.OnChanged += RefreshUI;
            RefreshUI();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            DisposeAllSubscriptions();

            if (_viewModel != null)
            {
                _viewModel.OnChanged -= RefreshUI;
                _viewModel.Reset();
            }

            _bus = null;
            _readModel = null;
            _clock = null;

            base.OnClose(isShutdown, userData);
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (_viewModel != null && _viewModel.IsDebugOverlayEnabled && _clock != null)
            {
                _viewModel.UpdateDebugInfo(_clock.NowMs, string.Empty);
                RefreshDebugOverlay();
            }
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            _subscriptions.Add(_bus.Subscribe<DamageApplied>(HandleDamageApplied));
            _subscriptions.Add(_bus.Subscribe<CharacterDefeated>(HandleCharacterDefeated));
            _subscriptions.Add(_bus.Subscribe<BattleInputPhaseChanged>(HandlePhaseChanged));
            _subscriptions.Add(_bus.Subscribe<ParryResolved>(HandleParryResolved));
            _subscriptions.Add(_bus.Subscribe<CounterEntryOpened>(HandleCounterOpened));
            _subscriptions.Add(_bus.Subscribe<CounterEntryClosed>(HandleCounterClosed));
        }

        private void DisposeAllSubscriptions()
        {
            for (int i = 0; i < _subscriptions.Count; i++)
                _subscriptions[i].Dispose();
            _subscriptions.Clear();
        }

        #endregion

        #region Event Handlers

        private void InitializeFromSnapshots()
        {
            if (_readModel == null) return;

            var snapshots = _readModel.GetAllSnapshots();
            for (int i = 0; i < snapshots.Count; i++)
            {
                var snap = snapshots[i];
                if (snap.InstanceId != null)
                {
                    _viewModel.UpdatePlayerSnapshot(snap);
                    break;
                }
            }
        }

        private void HandleDamageApplied(DamageApplied evt)
        {
            if (_readModel == null) return;

            var snap = _readModel.GetSnapshot(evt.TargetInstanceId);
            if (snap.InstanceId == _viewModel.PlayerSnapshot.InstanceId)
                _viewModel.UpdatePlayerSnapshot(snap);
            else
                _viewModel.UpdateEnemySnapshot(snap);
        }

        private void HandleCharacterDefeated(CharacterDefeated evt)
        {
            if (evt.InstanceId == _viewModel.PlayerSnapshot.InstanceId)
                _viewModel.SetBattleResult(new BattleResultInfo(BattleOutcome.Defeat, evt.InstanceId));
            else
                _viewModel.SetBattleResult(new BattleResultInfo(BattleOutcome.Victory, evt.InstanceId));
        }

        private void HandlePhaseChanged(BattleInputPhaseChanged evt)
        {
            _viewModel.UpdatePhase(evt.CurrentPhase);
        }

        private void HandleParryResolved(ParryResolved evt)
        {
            // UI 只关注 counter 可用状态（来自 CounterEntryOpened/Closed）。
        }

        private void HandleCounterOpened(CounterEntryOpened evt)
        {
            _viewModel.SetCounterAvailable(true);
        }

        private void HandleCounterClosed(CounterEntryClosed evt)
        {
            _viewModel.SetCounterAvailable(false);
        }

        #endregion

        #region UI Refresh

        private void RefreshUI()
        {
            RefreshPlayerStatus();
            RefreshEnemyStatus();
            RefreshActionIndicators();
            RefreshPhase();
            RefreshResult();
        }

        private void RefreshPlayerStatus()
        {
            var snap = _viewModel.PlayerSnapshot;
            if (snap.InstanceId == null) return;

            if (PlayerHpBar != null)
                PlayerHpBar.value = snap.MaxHp > 0 ? (float)snap.CurrentHp / snap.MaxHp : 0f;
            if (PlayerHpText != null)
                PlayerHpText.text = $"{snap.CurrentHp}/{snap.MaxHp}";
        }

        private void RefreshEnemyStatus()
        {
            var snap = _viewModel.EnemySnapshot;
            if (snap.InstanceId == null) return;

            if (EnemyHpBar != null)
                EnemyHpBar.value = snap.MaxHp > 0 ? (float)snap.CurrentHp / snap.MaxHp : 0f;
            if (EnemyHpText != null)
                EnemyHpText.text = $"{snap.CurrentHp}/{snap.MaxHp}";
        }

        private void RefreshActionIndicators()
        {
            if (ParryReadyIndicator != null)
                ParryReadyIndicator.alpha = _viewModel.IsParryReady ? 1f : 0f;
            if (CounterReadyIndicator != null)
                CounterReadyIndicator.alpha = _viewModel.IsCounterAvailable ? 1f : 0f;
        }

        private void RefreshPhase()
        {
            if (PhaseText != null)
                PhaseText.text = _viewModel.CurrentPhase.ToString();
        }

        private void RefreshResult()
        {
            if (ResultPanel == null) return;

            if (_viewModel.Result.HasValue)
            {
                ResultPanel.alpha = 1f;
                ResultPanel.blocksRaycasts = true;
                if (ResultText != null)
                    ResultText.text = _viewModel.Result.Value.Outcome == BattleOutcome.Victory ? "胜利" : "失败";
            }
            else
            {
                ResultPanel.alpha = 0f;
                ResultPanel.blocksRaycasts = false;
            }
        }

        private void RefreshDebugOverlay()
        {
            if (DebugOverlay == null) return;

            bool show = _viewModel.IsDebugOverlayEnabled;
            DebugOverlay.alpha = show ? 1f : 0f;

            if (show)
            {
                if (DebugClockText != null)
                    DebugClockText.text = $"Clock: {_viewModel.DebugCombatClockMs}ms";
                if (DebugPhaseText != null)
                    DebugPhaseText.text = $"Phase: {_viewModel.CurrentPhase}";
                if (DebugSegmentIdText != null)
                    DebugSegmentIdText.text = $"Seg: {_viewModel.DebugAttackSegmentId}";
            }
        }

        #endregion
    }

}
