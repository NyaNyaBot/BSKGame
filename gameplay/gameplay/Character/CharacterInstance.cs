using System;
using SysMath = System.Math;

namespace Game.Gameplay.Character
{
    /// <summary>
    /// 运行时角色实例（ADR-0011 / TR-char-001）。
    /// InstanceId 全局唯一标识单场战斗中的一个角色实例。
    /// 所有可变字段由 IDamageService 等受控 API 修改，不暴露公共 setter。
    /// </summary>
    public sealed class CharacterInstance
    {
        private int _currentHp;
        private int _currentMp;
        private int _shield;
        private int _snapshotVersion;
        private CharacterStatus _status;

        public string InstanceId { get; }
        public CharacterDefinition Definition { get; }

        public int MaxHp { get; }
        public int MaxMp { get; }
        public int CurrentHp => _currentHp;
        public int CurrentMp => _currentMp;
        public int Shield => _shield;
        public int Atk { get; }
        public int Def { get; }
        public CharacterStatus Status => _status;
        public int SnapshotVersion => _snapshotVersion;

        public CharacterInstance(CharacterDefinition definition, string instanceId)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));

            MaxHp = definition.BaseMaxHp;
            MaxMp = definition.BaseMaxMp;
            Atk = definition.BaseAtk;
            Def = definition.BaseDef;

            _currentHp = MaxHp;
            _currentMp = MaxMp;
            _shield = 0;
            _snapshotVersion = 1;
            _status = CharacterStatus.Active;
        }

        internal void SetHp(int value)
        {
            _currentHp = SysMath.Max(0, SysMath.Min(value, MaxHp));
            _snapshotVersion++;
        }

        internal void SetShield(int value)
        {
            _shield = SysMath.Max(0, value);
            _snapshotVersion++;
        }

        internal void SetMp(int value)
        {
            _currentMp = SysMath.Max(0, SysMath.Min(value, MaxMp));
            _snapshotVersion++;
        }

        internal void MarkDefeated()
        {
            if (_status == CharacterStatus.Active)
            {
                _status = CharacterStatus.Defeated;
                _snapshotVersion++;
            }
        }

        internal void MarkRemoved()
        {
            _status = CharacterStatus.Removed;
            _snapshotVersion++;
        }

        public CharacterBattleSnapshot TakeSnapshot()
        {
            return new CharacterBattleSnapshot(
                InstanceId,
                Definition.CharacterId,
                _snapshotVersion,
                _currentHp,
                MaxHp,
                _currentMp,
                MaxMp,
                _shield,
                Atk,
                Def,
                _status);
        }
    }

    public enum CharacterStatus : byte
    {
        Active = 0,
        Defeated = 1,
        Removed = 2,
    }
}
