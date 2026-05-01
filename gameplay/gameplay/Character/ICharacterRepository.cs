using System.Collections.Generic;

namespace Game.Gameplay.Character
{
    /// <summary>
    /// 角色仓储接口（ADR-0011）。
    /// 拥有角色定义、运行时实例和快照的读写权威。
    /// </summary>
    public interface ICharacterRepository
    {
        CharacterInstance CreateInstance(CharacterDefinition definition);
        CharacterInstance? GetInstance(string instanceId);
        IReadOnlyList<CharacterInstance> GetAllInstances();
        CharacterBattleSnapshot GetSnapshot(string instanceId);
        void RemoveInstance(string instanceId);
        void Clear();
    }
}
