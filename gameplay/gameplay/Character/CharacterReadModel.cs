using System.Collections.Generic;

namespace Game.Gameplay.Character
{
    /// <summary>
    /// ICharacterReadModel 的默认实现——对 ICharacterRepository 的只读包装。
    /// UI 层只持有此接口引用，无法访问任何写入 API。
    /// </summary>
    public sealed class CharacterReadModel : ICharacterReadModel
    {
        private readonly ICharacterRepository _repo;

        public CharacterReadModel(ICharacterRepository repo)
        {
            _repo = repo;
        }

        public CharacterBattleSnapshot GetSnapshot(string instanceId)
        {
            return _repo.GetSnapshot(instanceId);
        }

        public IReadOnlyList<CharacterBattleSnapshot> GetAllSnapshots()
        {
            var instances = _repo.GetAllInstances();
            var snapshots = new List<CharacterBattleSnapshot>(instances.Count);
            for (int i = 0; i < instances.Count; i++)
            {
                snapshots.Add(instances[i].TakeSnapshot());
            }
            return snapshots;
        }

        public bool TryGetSnapshot(string instanceId, out CharacterBattleSnapshot snapshot)
        {
            var instance = _repo.GetInstance(instanceId);
            if (instance == null)
            {
                snapshot = default;
                return false;
            }
            snapshot = instance.TakeSnapshot();
            return true;
        }
    }
}
