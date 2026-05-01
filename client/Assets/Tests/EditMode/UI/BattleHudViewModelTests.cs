using Game.Gameplay.Character;
using Game.Gameplay.TurnManager;
using Game.Gameplay.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    /// <summary>
    /// ADR-0010 / TR-ui-001, TR-ui-003：BattleHudViewModel 单元测试。
    /// </summary>
    public sealed class BattleHudViewModelTests
    {
        private BattleHudViewModel _vm;
        private int _changedCount;

        [SetUp]
        public void SetUp()
        {
            _vm = new BattleHudViewModel();
            _changedCount = 0;
            _vm.OnChanged += () => _changedCount++;
        }

        [Test]
        public void UpdatePlayerSnapshot_RaisesChanged()
        {
            var def = new CharacterDefinition("hero", "Hero", 100, 50, 20, 10);
            var inst = new CharacterInstance(def, "i1");
            var snap = inst.TakeSnapshot();

            _vm.UpdatePlayerSnapshot(snap);

            Assert.That(_vm.PlayerSnapshot.InstanceId, Is.EqualTo("i1"));
            Assert.That(_changedCount, Is.EqualTo(1));
        }

        [Test]
        public void UpdatePhase_SetsParryReady_OnEnemyAction()
        {
            _vm.UpdatePhase(BattleInputPhase.EnemyAction);

            Assert.That(_vm.IsParryReady, Is.True);
            Assert.That(_vm.CurrentPhase, Is.EqualTo(BattleInputPhase.EnemyAction));
        }

        [Test]
        public void UpdatePhase_ClearsParryReady_OnPlayerCommand()
        {
            _vm.UpdatePhase(BattleInputPhase.EnemyAction);
            _vm.UpdatePhase(BattleInputPhase.PlayerCommand);

            Assert.That(_vm.IsParryReady, Is.False);
        }

        [Test]
        public void SetCounterAvailable_RaisesChanged()
        {
            _vm.SetCounterAvailable(true);

            Assert.That(_vm.IsCounterAvailable, Is.True);
            Assert.That(_changedCount, Is.EqualTo(1));
        }

        [Test]
        public void SetBattleResult_Victory()
        {
            _vm.SetBattleResult(new BattleResultInfo(BattleOutcome.Victory, "enemy1"));

            Assert.That(_vm.Result, Is.Not.Null);
            Assert.That(_vm.Result!.Value.Outcome, Is.EqualTo(BattleOutcome.Victory));
            Assert.That(_changedCount, Is.EqualTo(1));
        }

        [Test]
        public void DebugOverlay_DefaultOff()
        {
            Assert.That(_vm.IsDebugOverlayEnabled, Is.False);
        }

        [Test]
        public void DebugInfo_DoesNotRaiseChanged()
        {
            _vm.UpdateDebugInfo(1234, "seg_1");

            Assert.That(_vm.DebugCombatClockMs, Is.EqualTo(1234));
            Assert.That(_vm.DebugAttackSegmentId, Is.EqualTo("seg_1"));
            Assert.That(_changedCount, Is.EqualTo(0), "Debug info should not trigger UI refresh");
        }

        [Test]
        public void Reset_ClearsAll()
        {
            _vm.UpdatePhase(BattleInputPhase.EnemyAction);
            _vm.SetCounterAvailable(true);
            _vm.SetBattleResult(new BattleResultInfo(BattleOutcome.Victory, "e1"));
            _changedCount = 0;

            _vm.Reset();

            Assert.That(_vm.CurrentPhase, Is.EqualTo(BattleInputPhase.None));
            Assert.That(_vm.IsCounterAvailable, Is.False);
            Assert.That(_vm.IsParryReady, Is.False);
            Assert.That(_vm.Result, Is.Null);
        }

        [Test]
        public void NoCountdown_Constraint_NoTimerField()
        {
            var vmType = typeof(BattleHudViewModel);
            var countdownField = vmType.GetProperty("CountdownMs");
            Assert.That(countdownField, Is.Null,
                "ADR-0010: ViewModel must not expose countdown timer");
        }
    }
}
