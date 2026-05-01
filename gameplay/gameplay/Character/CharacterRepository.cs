using System;
using System.Collections.Generic;

namespace Game.Gameplay.Character
{
    /// <summary>
    /// 内存角色仓储实现（ADR-0011）。
    /// InstanceId 使用递增计数器保证战斗内唯一。
    /// </summary>
    public sealed class CharacterRepository : ICharacterRepository
    {
        private readonly List<CharacterInstance> _instances = new List<CharacterInstance>();
        private readonly Dictionary<string, CharacterInstance> _lookup = new Dictionary<string, CharacterInstance>(StringComparer.Ordinal);
        private int _nextId;

        public CharacterInstance CreateInstance(CharacterDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            _nextId++;
            var instanceId = $"{definition.CharacterId}_{_nextId}";

            if (_lookup.ContainsKey(instanceId))
                throw new InvalidOperationException($"InstanceId collision: {instanceId}");

            var instance = new CharacterInstance(definition, instanceId);
            _instances.Add(instance);
            _lookup[instanceId] = instance;
            return instance;
        }

        public CharacterInstance? GetInstance(string instanceId)
        {
            _lookup.TryGetValue(instanceId, out var instance);
            return instance;
        }

        public IReadOnlyList<CharacterInstance> GetAllInstances() => _instances;

        public CharacterBattleSnapshot GetSnapshot(string instanceId)
        {
            if (!_lookup.TryGetValue(instanceId, out var instance))
                throw new KeyNotFoundException($"No character instance with id '{instanceId}'.");
            return instance.TakeSnapshot();
        }

        public void RemoveInstance(string instanceId)
        {
            if (_lookup.TryGetValue(instanceId, out var instance))
            {
                instance.MarkRemoved();
                _lookup.Remove(instanceId);
                _instances.Remove(instance);
            }
        }

        public void Clear()
        {
            _instances.Clear();
            _lookup.Clear();
            _nextId = 0;
        }
    }
}
