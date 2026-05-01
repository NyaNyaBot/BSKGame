using System.Collections.Generic;
using Game.Gameplay.BattleAction;
using Game.Gameplay.Character;
using Game.Gameplay.Damage;
using Game.Gameplay.Events;
using Game.Gameplay.Integration;
using Game.Gameplay.SceneLifecycle;
using Game.Gameplay.TurnManager;
using NUnit.Framework;

namespace Game.Tests.EditMode.Integration
{
    /// <summary>
    /// BattleOrchestrator 端到端集成测试。
    /// 验证完整战斗循环：开战 → 玩家攻击 → 推进回合 → 角色死亡 → 战斗结束。
    /// </summary>
    public sealed class BattleOrchestratorTests
    {
        private SceneContextService _sceneCtx;
        private BattleOrchestrator _orch;

        private static CharacterDefinition MakePlayer(int hp = 100, int atk = 30)
            => new CharacterDefinition("player_01", "Player", hp, 0, atk, 5);

        private static CharacterDefinition MakeEnemy(int hp = 100, int atk = 20)
            => new CharacterDefinition("enemy_01", "Enemy", hp, 0, atk, 3);

        [SetUp]
        public void SetUp()
        {
            _sceneCtx = new SceneContextService();
            _sceneCtx.EnterScene(SceneKind.Battle);
            _sceneCtx.CreateBattleContext(default);

            _orch = new BattleOrchestrator(_sceneCtx, new BattleStartConfig { BattleContextId = "test_battle" });
            _orch.Start(MakePlayer(), MakeEnemy());
        }

        [TearDown]
        public void TearDown()
        {
            _orch?.Dispose();
        }

        [Test]
        public void Start_SetsPhaseToPlayerCommand()
        {
            Assert.That(_orch.PhaseMachine.CurrentPhase, Is.EqualTo(BattleInputPhase.PlayerCommand));
            Assert.That(_orch.IsRunning, Is.True);
            Assert.That(_orch.TurnNumber, Is.EqualTo(1));
        }

        [Test]
        public void Start_CreatesCharacters()
        {
            var instances = _orch.CharacterRepo.GetAllInstances();
            Assert.That(instances.Count, Is.EqualTo(2));
            Assert.That(instances[0].MaxHp, Is.EqualTo(100));
            Assert.That(instances[1].MaxHp, Is.EqualTo(100));
        }

        [Test]
        public void SubmitPlayerAction_DealsDamage()
        {
            var instances = _orch.CharacterRepo.GetAllInstances();
            var player = instances[0];
            var enemy = instances[1];

            var result = _orch.SubmitPlayerAction(player.InstanceId, enemy.InstanceId);

            Assert.That(result.Outcome, Is.EqualTo(BattleActionOutcome.Accepted));
            Assert.That(enemy.CurrentHp, Is.LessThan(100));
        }

        [Test]
        public void SubmitPlayerAction_TransitionsToEnemyAction()
        {
            var instances = _orch.CharacterRepo.GetAllInstances();
            _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);

            Assert.That(_orch.PhaseMachine.CurrentPhase, Is.EqualTo(BattleInputPhase.EnemyAction));
        }

        [Test]
        public void AdvanceToResolution_ReturnsToPlayerCommand()
        {
            var instances = _orch.CharacterRepo.GetAllInstances();
            _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);
            _orch.AdvanceToResolution();

            Assert.That(_orch.PhaseMachine.CurrentPhase, Is.EqualTo(BattleInputPhase.PlayerCommand));
            Assert.That(_orch.TurnNumber, Is.EqualTo(2));
        }

        [Test]
        public void FullBattleLoop_EnemyDefeated_BattleEnds()
        {
            var player = MakePlayer(hp: 100, atk: 60);
            var enemy = MakeEnemy(hp: 50, atk: 10);

            _orch.Dispose();
            _sceneCtx.EnterScene(SceneKind.Battle);
            _sceneCtx.CreateBattleContext(default);
            _orch = new BattleOrchestrator(_sceneCtx, new BattleStartConfig { BattleContextId = "battle_2" });
            _orch.Start(player, enemy);

            var instances = _orch.CharacterRepo.GetAllInstances();

            _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);
            _orch.AdvanceToResolution();

            Assert.That(instances[1].Status, Is.EqualTo(CharacterStatus.Defeated));
            Assert.That(_orch.PhaseMachine.CurrentPhase, Is.EqualTo(BattleInputPhase.BattleEnd));
            Assert.That(_orch.IsRunning, Is.False);
        }

        [Test]
        public void Tick_AdvancesCombatClock()
        {
            _orch.Tick(100);
            Assert.That(_orch.Clock.NowMs, Is.EqualTo(100));
        }

        [Test]
        public void DamageEvents_PublishedViaBus()
        {
            var received = new List<DamageApplied>();
            _orch.Bus.Subscribe<DamageApplied>(evt => received.Add(evt));

            var instances = _orch.CharacterRepo.GetAllInstances();
            _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);

            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].TargetInstanceId, Is.EqualTo(instances[1].InstanceId));
            Assert.That(received[0].HpDamage, Is.GreaterThan(0));
        }

        [Test]
        public void PhaseChangedEvents_Published()
        {
            var phases = new List<BattleInputPhase>();
            _orch.Bus.Subscribe<BattleInputPhaseChanged>(evt => phases.Add(evt.CurrentPhase));

            var instances = _orch.CharacterRepo.GetAllInstances();
            _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);

            Assert.That(phases, Does.Contain(BattleInputPhase.EnemyAction));
        }

        [Test]
        public void CharacterDefeatedEvent_Published()
        {
            var defeated = new List<CharacterDefeated>();
            var player = MakePlayer(hp: 100, atk: 200);
            var enemy = MakeEnemy(hp: 10, atk: 1);

            _orch.Dispose();
            _sceneCtx.EnterScene(SceneKind.Battle);
            _sceneCtx.CreateBattleContext(default);
            _orch = new BattleOrchestrator(_sceneCtx, new BattleStartConfig { BattleContextId = "battle_3" });
            _orch.Start(player, enemy);
            _orch.Bus.Subscribe<CharacterDefeated>(evt => defeated.Add(evt));

            var instances = _orch.CharacterRepo.GetAllInstances();
            _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);

            Assert.That(defeated.Count, Is.EqualTo(1));
            Assert.That(defeated[0].InstanceId, Is.EqualTo(instances[1].InstanceId));
        }

        [Test]
        public void ReadModel_ReturnsSnapshots()
        {
            var snapshots = _orch.ReadModel.GetAllSnapshots();
            Assert.That(snapshots.Count, Is.EqualTo(2));
            Assert.That(snapshots[0].MaxHp, Is.EqualTo(100));
        }

        [Test]
        public void Dispose_CleansUpAllSystems()
        {
            _orch.Dispose();

            Assert.That(_orch.IsRunning, Is.False);
            Assert.That(_orch.CharacterRepo.GetAllInstances().Count, Is.EqualTo(0));
            Assert.That(_orch.PhaseMachine.CurrentPhase, Is.EqualTo(BattleInputPhase.None));

            _orch = null;
        }

        [Test]
        public void MultiTurnBattle_TurnNumberIncrements()
        {
            var instances = _orch.CharacterRepo.GetAllInstances();

            for (int i = 0; i < 3; i++)
            {
                _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);
                if (_orch.IsRunning)
                    _orch.AdvanceToResolution();
            }

            Assert.That(_orch.TurnNumber, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void Session_HasCorrectBattleContextId()
        {
            Assert.That(_orch.Session.BattleContextId, Is.EqualTo("test_battle"));
        }

        [Test]
        public void BattleEventContext_MatchesSceneContext()
        {
            var received = new List<DamageApplied>();
            _orch.Bus.Subscribe<DamageApplied>(evt => received.Add(evt));

            var instances = _orch.CharacterRepo.GetAllInstances();
            _orch.SubmitPlayerAction(instances[0].InstanceId, instances[1].InstanceId);

            Assert.That(received.Count, Is.EqualTo(1));
            Assert.That(received[0].Context.SceneContextId, Is.Not.Null.And.Not.Empty);
        }
    }
}
