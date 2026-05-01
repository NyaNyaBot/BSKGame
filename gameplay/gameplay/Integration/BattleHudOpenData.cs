using Game.Gameplay.Character;
using Game.Gameplay.Combat;
using Game.Gameplay.Events;

namespace Game.Gameplay.Integration
{
    /// <summary>
    /// BattleHudForm 打开时传入的数据。
    /// 放在 gameplay 层以避免 Gameplay → Windows 的跨层依赖。
    /// </summary>
    public class BattleHudOpenData
    {
        public BattleEventBus Bus { get; set; }
        public ICharacterReadModel ReadModel { get; set; }
        public ICombatClock Clock { get; set; }
    }
}
