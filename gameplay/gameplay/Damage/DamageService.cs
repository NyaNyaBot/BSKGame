using System;
using System.Collections.Generic;
using Game.Gameplay.Character;
using SysMath = System.Math;

namespace Game.Gameplay.Damage
{
    /// <summary>
    /// 权威伤害管道实现（ADR-0005 / TR-dmg-001, TR-dmg-002）。
    /// 预检 → 护盾吸收 → HP 扣减 → 死亡判定。
    /// 同一 DamageRequestId 幂等。
    /// </summary>
    public sealed class DamageService : IDamageService
    {
        private readonly ICharacterRepository _repo;
        private readonly Dictionary<string, DamageResult> _processedRequests =
            new Dictionary<string, DamageResult>(StringComparer.Ordinal);

        public event Action<DamageResult>? OnDamageProcessed;

        public DamageService(ICharacterRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            if (string.IsNullOrEmpty(request.RequestId))
                return DamageResult.Rejected(request.RequestId, request.TargetInstanceId, "RequestId is null or empty");

            if (_processedRequests.TryGetValue(request.RequestId, out var cached))
                return cached;

            var instance = _repo.GetInstance(request.TargetInstanceId);
            if (instance == null)
            {
                var rejected = DamageResult.Rejected(request.RequestId, request.TargetInstanceId, "Target not found");
                CacheAndNotify(rejected);
                return rejected;
            }

            if (instance.Status != CharacterStatus.Active)
            {
                var rejected = DamageResult.Rejected(request.RequestId, request.TargetInstanceId,
                    $"Target status is {instance.Status}");
                CacheAndNotify(rejected);
                return rejected;
            }

            int rawDamage = SysMath.Max(0, request.RawDamage);

            int shieldBefore = instance.Shield;
            int shieldAbsorbed = SysMath.Min(shieldBefore, rawDamage);
            int remainingDamage = rawDamage - shieldAbsorbed;

            instance.SetShield(shieldBefore - shieldAbsorbed);

            int hpBefore = instance.CurrentHp;
            int hpDamage = SysMath.Min(hpBefore, remainingDamage);
            instance.SetHp(hpBefore - hpDamage);

            bool isDefeated = instance.CurrentHp <= 0 && instance.Status == CharacterStatus.Active;
            if (isDefeated)
            {
                instance.MarkDefeated();
            }

            var result = new DamageResult(
                request.RequestId,
                request.TargetInstanceId,
                DamageOutcome.Applied,
                shieldAbsorbed,
                hpDamage,
                instance.CurrentHp,
                instance.Shield,
                isDefeated);

            CacheAndNotify(result);
            return result;
        }

        private void CacheAndNotify(DamageResult result)
        {
            _processedRequests[result.RequestId] = result;
            OnDamageProcessed?.Invoke(result);
        }

        public void ClearCache()
        {
            _processedRequests.Clear();
        }
    }
}
