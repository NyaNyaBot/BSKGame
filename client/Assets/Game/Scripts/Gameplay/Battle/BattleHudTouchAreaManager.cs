using System;
using System.Collections.Generic;
using Game.Gameplay.Events;
using Game.Gameplay.Input;
using Game.Gameplay.Parry;
using Game.Gameplay.TurnManager;

namespace Game.Client
{
    /// <summary>
    /// 战斗 HUD 触区生命周期管理器（ADR-0010, ADR-0006 / TR-ui-002）。
    /// 负责在合适的阶段注册/注销 Parry 和 Counter 触区。
    /// Counter priority=300 > Parry priority=100。
    /// </summary>
    public sealed class BattleHudTouchAreaManager : IDisposable
    {
        public const string ParryAreaId = "battle_parry";
        public const string CounterAreaId = "battle_counter";

        private readonly InputHitAreaRegistry _registry;
        private readonly BattleEventBus _bus;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private bool _parryRegistered;
        private bool _counterRegistered;
        private int _stateVersion = 1;

        public bool IsParryRegistered => _parryRegistered;
        public bool IsCounterRegistered => _counterRegistered;
        public int StateVersion => _stateVersion;

        public BattleHudTouchAreaManager(InputHitAreaRegistry registry, BattleEventBus bus)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            _subscriptions.Add(_bus.Subscribe<BattleInputPhaseChanged>(OnPhaseChanged));
            _subscriptions.Add(_bus.Subscribe<CounterEntryOpened>(OnCounterOpened));
            _subscriptions.Add(_bus.Subscribe<CounterEntryClosed>(OnCounterClosed));
        }

        private void OnPhaseChanged(BattleInputPhaseChanged evt)
        {
            if (evt.CurrentPhase == BattleInputPhase.EnemyAction)
            {
                RegisterParry();
            }
            else
            {
                UnregisterParry();
                UnregisterCounter();
            }
        }

        private void OnCounterOpened(CounterEntryOpened evt)
        {
            RegisterCounter();
        }

        private void OnCounterClosed(CounterEntryClosed evt)
        {
            UnregisterCounter();
        }

        private void RegisterParry()
        {
            if (_parryRegistered) return;

            _registry.Register(new HitAreaRegistration(
                ParryAreaId,
                InputHitAreaRegistry.PriorityParry,
                blocksUnderlying: true));

            _parryRegistered = true;
            BumpStateVersion();
        }

        private void UnregisterParry()
        {
            if (!_parryRegistered) return;

            _registry.Unregister(ParryAreaId);
            _parryRegistered = false;
            BumpStateVersion();
        }

        private void RegisterCounter()
        {
            if (_counterRegistered) return;

            _registry.Register(new HitAreaRegistration(
                CounterAreaId,
                InputHitAreaRegistry.PriorityCounter,
                blocksUnderlying: false));

            if (_parryRegistered)
            {
                // ADR-0006: counter active 时 parry BlocksUnderlying=false
                // InputHitAreaRegistry 当前不支持修改 BlocksUnderlying，
                // 但 counter priority=300 自然覆盖 parry priority=100。
            }

            _counterRegistered = true;
            BumpStateVersion();
        }

        private void UnregisterCounter()
        {
            if (!_counterRegistered) return;

            _registry.Unregister(CounterAreaId);
            _counterRegistered = false;
            BumpStateVersion();
        }

        private void BumpStateVersion()
        {
            _stateVersion++;
            if (_parryRegistered)
                _registry.UpdateStateVersion(ParryAreaId, _stateVersion);
            if (_counterRegistered)
                _registry.UpdateStateVersion(CounterAreaId, _stateVersion);
        }

        /// <summary>
        /// 暂停时禁用所有战斗触区。
        /// </summary>
        public void DisableAll()
        {
            if (_parryRegistered)
                _registry.SetEnabled(ParryAreaId, false);
            if (_counterRegistered)
                _registry.SetEnabled(CounterAreaId, false);
        }

        /// <summary>
        /// 恢复时启用所有战斗触区。
        /// </summary>
        public void EnableAll()
        {
            if (_parryRegistered)
                _registry.SetEnabled(ParryAreaId, true);
            if (_counterRegistered)
                _registry.SetEnabled(CounterAreaId, true);
        }

        public void Dispose()
        {
            for (int i = 0; i < _subscriptions.Count; i++)
                _subscriptions[i].Dispose();
            _subscriptions.Clear();

            UnregisterCounter();
            UnregisterParry();
        }
    }
}
