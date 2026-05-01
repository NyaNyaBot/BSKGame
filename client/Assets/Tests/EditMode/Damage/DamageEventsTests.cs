using System.Collections.Generic;
using Game.Gameplay.Character;
using Game.Gameplay.Combat;
using Game.Gameplay.Damage;
using Game.Gameplay.Events;
using Game.Gameplay.SceneLifecycle;
using NUnit.Framework;

namespace Game.Tests.EditMode.Damage
{
    /// <summary>
    /// ADR-0005/ADR-0002 / story-003：DamageApplied/DamageRejected/CharacterDefeated 事件发布。
    /// </summary>
    public sealed class DamageEventsTests
    {
        private SceneContextService _ctx;
        private BattleEventBus _bus;
        private CharacterRepository _repo;
        private DamageService _svc;
        private CombatClock _clock;
        private CharacterInstance _target;
        private SceneEventContext _eventCtx;

        [SetUp]
        public void SetUp()
        {
            _ctx = new SceneContextService();
            _ctx.EnterScene(SceneKind.Battle);
            _bus = new BattleEventBus(_ctx);
            _repo = new CharacterRepository();
            _svc = new DamageService(_repo);
            _clock = new CombatClock { MaxStepMs = 5000 };
            _clock.Advance(1000);

            var def = new CharacterDefinition("hero", "英雄", 500, 100, 40, 20);
            _target = _repo.CreateInstance(def);

            var battle = _ctx.CreateBattleContext(default);
            _eventCtx = battle.ToEventContext();

            _svc.OnDamageProcessed += result =>
            {
                if (result.Outcome == DamageOutcome.Applied)
                {
                    _bus.Publish(new DamageApplied(
                        _eventCtx, _clock.NowMs,
                        result.RequestId, result.TargetInstanceId,
                        result.ShieldAbsorbed, result.HpDamage,
                        result.FinalHp, result.FinalShield));

                    if (result.IsDefeated)
                    {
                        _bus.Publish(new CharacterDefeated(
                            _eventCtx, _clock.NowMs,
                            result.TargetInstanceId, result.RequestId));
                    }
                }
                else
                {
                    _bus.Publish(new DamageRejected(
                        _eventCtx, _clock.NowMs,
                        result.RequestId, result.TargetInstanceId,
                        result.RejectionReason ?? "unknown"));
                }
            };
        }

        [Test]
        public void ApplyDamage_PublishesDamageApplied()
        {
            var received = new List<DamageApplied>();
            _bus.Subscribe<DamageApplied>(e => received.Add(e));

            _svc.ApplyDamage(new DamageRequest("r1", _target.InstanceId, 100));

            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].HpDamage, Is.EqualTo(100));
            Assert.That(received[0].FinalHp, Is.EqualTo(400));
            Assert.That(received[0].OccurredAtCombatClockMs, Is.EqualTo(1000));
        }

        [Test]
        public void RejectedDamage_PublishesDamageRejected()
        {
            var received = new List<DamageRejected>();
            _bus.Subscribe<DamageRejected>(e => received.Add(e));

            _svc.ApplyDamage(new DamageRequest("r1", "nonexistent", 100));

            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].Reason, Does.Contain("not found"));
        }

        [Test]
        public void LethalDamage_PublishesBoth_Applied_And_Defeated()
        {
            var applied = new List<DamageApplied>();
            var defeated = new List<CharacterDefeated>();
            _bus.Subscribe<DamageApplied>(e => applied.Add(e));
            _bus.Subscribe<CharacterDefeated>(e => defeated.Add(e));

            _svc.ApplyDamage(new DamageRequest("r1", _target.InstanceId, 999));

            Assert.That(applied.Count, Is.EqualTo(1));
            Assert.That(applied[0].FinalHp, Is.EqualTo(0));

            Assert.That(defeated.Count, Is.EqualTo(1));
            Assert.That(defeated[0].InstanceId, Is.EqualTo(_target.InstanceId));
        }

        [Test]
        public void IdempotentRequest_OnlyOneEventPublished()
        {
            int count = 0;
            _bus.Subscribe<DamageApplied>(e => count++);

            _svc.ApplyDamage(new DamageRequest("r1", _target.InstanceId, 50));
            _svc.ApplyDamage(new DamageRequest("r1", _target.InstanceId, 50));

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void NoAppliedEvent_WhenRejected()
        {
            int appliedCount = 0;
            _bus.Subscribe<DamageApplied>(e => appliedCount++);

            _target.MarkDefeated();
            _svc.ApplyDamage(new DamageRequest("r1", _target.InstanceId, 100));

            Assert.That(appliedCount, Is.EqualTo(0));
        }
    }
}
