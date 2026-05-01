using System;
using System.Collections.Generic;
using Game.Gameplay.Character;

namespace Game.Gameplay.EnemyAI
{
    /// <summary>
    /// 攻击模式选择器（ADR-0008 / TR-enemy-001）。
    /// 确定性选择：权重/冷却/HP 阶段过滤，注入确定性 RNG。
    /// </summary>
    public sealed class EnemyAttackPlanner
    {
        private readonly ICharacterRepository _charRepo;
        private readonly Dictionary<string, int> _cooldowns = new Dictionary<string, int>(StringComparer.Ordinal);

        public EnemyAttackPlanner(ICharacterRepository charRepo)
        {
            _charRepo = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
        }

        public EnemyActionPlan? SelectAction(
            EnemyActionQuery query,
            IReadOnlyList<AttackPatternDefinition> patterns,
            int deterministicSeed)
        {
            if (patterns == null || patterns.Count == 0)
                return null;

            var target = _charRepo.GetInstance(query.TargetInstanceId);
            if (target == null || target.Status != CharacterStatus.Active)
                return null;

            var scores = new List<PatternScore>();
            int totalWeight = 0;

            for (int i = 0; i < patterns.Count; i++)
            {
                var p = patterns[i];
                int score = ComputeScore(p, query);
                if (score > 0)
                {
                    scores.Add(new PatternScore(p, score));
                    totalWeight += score;
                }
            }

            if (scores.Count == 0 || totalWeight <= 0)
                return null;

            int roll = DeterministicRoll(deterministicSeed, query.TurnNumber, totalWeight);
            int cumulative = 0;
            for (int i = 0; i < scores.Count; i++)
            {
                cumulative += scores[i].Score;
                if (roll < cumulative)
                {
                    var selected = scores[i].Pattern;
                    return new EnemyActionPlan(
                        selected.PatternId,
                        query.ActorInstanceId,
                        query.TargetInstanceId,
                        selected.BaseDamage);
                }
            }

            var fallback = scores[scores.Count - 1].Pattern;
            return new EnemyActionPlan(
                fallback.PatternId,
                query.ActorInstanceId,
                query.TargetInstanceId,
                fallback.BaseDamage);
        }

        private int ComputeScore(AttackPatternDefinition pattern, EnemyActionQuery query)
        {
            if (_cooldowns.TryGetValue(pattern.PatternId, out int remainingCd) && remainingCd > 0)
                return 0;

            if (pattern.HpPhase != HpPhaseRequirement.Any)
            {
                var actor = _charRepo.GetInstance(query.ActorInstanceId);
                if (actor == null) return 0;

                bool aboveHalf = actor.CurrentHp * 2 > actor.MaxHp;
                if (pattern.HpPhase == HpPhaseRequirement.AboveHalf && !aboveHalf)
                    return 0;
                if (pattern.HpPhase == HpPhaseRequirement.BelowHalf && aboveHalf)
                    return 0;
            }

            return pattern.BaseWeight;
        }

        public void ApplyCooldown(string patternId, int cooldownTurns)
        {
            _cooldowns[patternId] = cooldownTurns;
        }

        public void TickCooldowns()
        {
            var keys = new List<string>(_cooldowns.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                int val = _cooldowns[key] - 1;
                if (val <= 0)
                    _cooldowns.Remove(key);
                else
                    _cooldowns[key] = val;
            }
        }

        public void ClearCooldowns() => _cooldowns.Clear();

        private static int DeterministicRoll(int seed, int turnNumber, int max)
        {
            unchecked
            {
                long h = (long)seed * 31 + turnNumber;
                h = h * 2654435761L;
                h = h < 0 ? -h : h;
                return (int)(h % max);
            }
        }

        private readonly struct PatternScore
        {
            public AttackPatternDefinition Pattern { get; }
            public int Score { get; }

            public PatternScore(AttackPatternDefinition p, int s)
            {
                Pattern = p;
                Score = s;
            }
        }
    }

    public readonly struct EnemyActionQuery
    {
        public string ActorInstanceId { get; }
        public string TargetInstanceId { get; }
        public int TurnNumber { get; }

        public EnemyActionQuery(string actorInstanceId, string targetInstanceId, int turnNumber)
        {
            ActorInstanceId = actorInstanceId;
            TargetInstanceId = targetInstanceId;
            TurnNumber = turnNumber;
        }
    }

    public readonly struct EnemyActionPlan
    {
        public string PatternId { get; }
        public string ActorInstanceId { get; }
        public string TargetInstanceId { get; }
        public int BaseDamage { get; }

        public EnemyActionPlan(string patternId, string actorInstanceId, string targetInstanceId, int baseDamage)
        {
            PatternId = patternId;
            ActorInstanceId = actorInstanceId;
            TargetInstanceId = targetInstanceId;
            BaseDamage = baseDamage;
        }
    }
}
