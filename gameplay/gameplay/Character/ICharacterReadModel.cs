using System.Collections.Generic;

namespace Game.Gameplay.Character
{
    /// <summary>
    /// UI 只读角色投影接口（ADR-0011 / TR-char-003）。
    /// UI/反馈层通过此接口读取快照，不得直接修改 gameplay 权威状态。
    /// </summary>
    public interface ICharacterReadModel
    {
        CharacterBattleSnapshot GetSnapshot(string instanceId);
        IReadOnlyList<CharacterBattleSnapshot> GetAllSnapshots();
        bool TryGetSnapshot(string instanceId, out CharacterBattleSnapshot snapshot);
    }
}
