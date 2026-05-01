using System;
using System.Collections.Generic;
using Game.Gameplay.Character;
using Game.Gameplay.Combat;
using Game.Gameplay.Damage;
using SysMath = System.Math;

namespace Game.Gameplay.BattleAction
{
    /// <summary>
    /// Counter 行动服务（ADR-0012 / TR-action-002）。
    /// 校验 CounterEntryOpened 授权，生成 DamageRequest（1.5x 倍率）。
    /// </summary>
    public sealed class CounterActionService
    {
        public const int CounterPowerMultiplierBasisPoints = 15000;

        private readonly ICharacterRepository _charRepo;
        private readonly ICombatClock _clock;
        private readonly Dictionary<string, CounterEntry> _activeCounters =
            new Dictionary<string, CounterEntry>(StringComparer.Ordinal);
        private readonly Dictionary<string, BattleActionResult> _processedActions =
            new Dictionary<string, BattleActionResult>(StringComparer.Ordinal);

        public event System.Action<string>? OnCounterConsumed;

        public CounterActionService(ICharacterRepository charRepo, ICombatClock clock)
        {
            _charRepo = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void RegisterCounter(string resolutionId, string actorInstanceId, string targetInstanceId, long expiresAtMs)
        {
            _activeCounters[resolutionId] = new CounterEntry(resolutionId, actorInstanceId, targetInstanceId, expiresAtMs, false);
        }

        public BattleActionResult SubmitCounter(string actionRequestId, string sourceResolutionId)
        {
            if (string.IsNullOrEmpty(actionRequestId))
                return BattleActionResult.Rejected(actionRequestId, "ActionRequestId is null or empty");

            if (_processedActions.TryGetValue(actionRequestId, out var cached))
                return cached;

            if (!_activeCounters.TryGetValue(sourceResolutionId, out var counter))
            {
                var rejected = BattleActionResult.Rejected(actionRequestId, "CounterNotFound");
                _processedActions[actionRequestId] = rejected;
                return rejected;
            }

            if (counter.IsConsumed)
            {
                var rejected = BattleActionResult.Rejected(actionRequestId, "CounterAlreadyConsumed");
                _processedActions[actionRequestId] = rejected;
                return rejected;
            }

            if (_clock.NowMs > counter.ExpiresAtMs)
            {
                var rejected = BattleActionResult.Rejected(actionRequestId, "CounterExpired");
                _processedActions[actionRequestId] = rejected;
                return rejected;
            }

            var actor = _charRepo.GetInstance(counter.ActorInstanceId);
            if (actor == null || actor.Status != CharacterStatus.Active)
            {
                var rejected = BattleActionResult.Rejected(actionRequestId, "Actor cannot act");
                _processedActions[actionRequestId] = rejected;
                return rejected;
            }

            var target = _charRepo.GetInstance(counter.TargetInstanceId);
            if (target == null || target.Status != CharacterStatus.Active)
            {
                var rejected = BattleActionResult.Rejected(actionRequestId, "TargetInvalid");
                _processedActions[actionRequestId] = rejected;
                return rejected;
            }

            int baseDamage = BattleActionService.RoundHalfUp(actor.Atk, CounterPowerMultiplierBasisPoints);
            var dmgReq = new DamageRequest($"dmg_{actionRequestId}", counter.TargetInstanceId, baseDamage);

            _activeCounters[sourceResolutionId] = new CounterEntry(
                counter.ResolutionId, counter.ActorInstanceId, counter.TargetInstanceId,
                counter.ExpiresAtMs, true);

            OnCounterConsumed?.Invoke(sourceResolutionId);

            var result = BattleActionResult.Accepted(actionRequestId, dmgReq);
            _processedActions[actionRequestId] = result;
            return result;
        }

        public bool IsCounterActive(string resolutionId)
        {
            return _activeCounters.TryGetValue(resolutionId, out var c) && !c.IsConsumed;
        }

        public void Clear()
        {
            _activeCounters.Clear();
            _processedActions.Clear();
        }

        private readonly struct CounterEntry
        {
            public string ResolutionId { get; }
            public string ActorInstanceId { get; }
            public string TargetInstanceId { get; }
            public long ExpiresAtMs { get; }
            public bool IsConsumed { get; }

            public CounterEntry(string resolutionId, string actorId, string targetId, long expiresAt, bool consumed)
            {
                ResolutionId = resolutionId;
                ActorInstanceId = actorId;
                TargetInstanceId = targetId;
                ExpiresAtMs = expiresAt;
                IsConsumed = consumed;
            }
        }
    }
}
