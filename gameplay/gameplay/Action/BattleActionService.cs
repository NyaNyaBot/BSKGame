using System;
using System.Collections.Generic;
using Game.Gameplay.Character;
using Game.Gameplay.Damage;
using Game.Gameplay.TurnManager;
using SysMath = System.Math;

namespace Game.Gameplay.BattleAction
{
    /// <summary>
    /// 战斗行动服务（ADR-0012 / TR-action-001）。
    /// 校验阶段 + actor/target 合法性，生成 DamageRequest。
    /// </summary>
    public sealed class BattleActionService
    {
        public const int DefaultPowerMultiplierBasisPoints = 10000;

        private readonly InputPhaseStateMachine _phaseMachine;
        private readonly ICharacterRepository _charRepo;
        private readonly Dictionary<string, BattleActionResult> _processedActions =
            new Dictionary<string, BattleActionResult>(StringComparer.Ordinal);

        public BattleActionService(InputPhaseStateMachine phaseMachine, ICharacterRepository charRepo)
        {
            _phaseMachine = phaseMachine ?? throw new ArgumentNullException(nameof(phaseMachine));
            _charRepo = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
        }

        public BattleActionResult Submit(in BattleActionRequest request, int powerMultiplierBasisPoints = DefaultPowerMultiplierBasisPoints)
        {
            if (string.IsNullOrEmpty(request.ActionRequestId))
                return BattleActionResult.Rejected(request.ActionRequestId, "ActionRequestId is null or empty");

            if (_processedActions.TryGetValue(request.ActionRequestId, out var cached))
                return cached;

            if (_phaseMachine.CurrentPhase != BattleInputPhase.PlayerCommand)
            {
                var rejected = BattleActionResult.Rejected(request.ActionRequestId,
                    $"PhaseMismatch: current={_phaseMachine.CurrentPhase}, required=PlayerCommand");
                Cache(rejected);
                return rejected;
            }

            var actor = _charRepo.GetInstance(request.ActorInstanceId);
            if (actor == null || actor.Status != CharacterStatus.Active)
            {
                var rejected = BattleActionResult.Rejected(request.ActionRequestId, "Actor cannot act");
                Cache(rejected);
                return rejected;
            }

            var target = _charRepo.GetInstance(request.TargetInstanceId);
            if (target == null || target.Status != CharacterStatus.Active)
            {
                var rejected = BattleActionResult.Rejected(request.ActionRequestId, "Target cannot be targeted");
                Cache(rejected);
                return rejected;
            }

            int baseDamage = RoundHalfUp(actor.Atk, powerMultiplierBasisPoints);

            var dmgReq = new DamageRequest(
                $"dmg_{request.ActionRequestId}",
                request.TargetInstanceId,
                baseDamage);

            var result = BattleActionResult.Accepted(request.ActionRequestId, dmgReq);
            Cache(result);
            return result;
        }

        private void Cache(BattleActionResult result)
        {
            _processedActions[result.ActionRequestId] = result;
        }

        /// <summary>
        /// action_base_damage = round_half_up(attack_power * multiplier / 10000)
        /// </summary>
        internal static int RoundHalfUp(int attackPower, int basisPoints)
        {
            long product = (long)attackPower * basisPoints;
            int result = (int)((product + 5000) / 10000);
            return SysMath.Max(0, result);
        }

        public void ClearCache()
        {
            _processedActions.Clear();
        }
    }
}
