namespace Game.Gameplay.Character
{
    /// <summary>
    /// 不可变角色战斗快照（ADR-0011 / TR-char-002）。
    /// 构造后所有字段只读；UI/反馈消费者使用此结构而非可变引用。
    /// </summary>
    public readonly struct CharacterBattleSnapshot
    {
        public string InstanceId { get; }
        public string CharacterId { get; }
        public int Version { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int CurrentMp { get; }
        public int MaxMp { get; }
        public int Shield { get; }
        public int Atk { get; }
        public int Def { get; }
        public CharacterStatus Status { get; }

        public CharacterBattleSnapshot(
            string instanceId,
            string characterId,
            int version,
            int currentHp,
            int maxHp,
            int currentMp,
            int maxMp,
            int shield,
            int atk,
            int def,
            CharacterStatus status)
        {
            InstanceId = instanceId;
            CharacterId = characterId;
            Version = version;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            CurrentMp = currentMp;
            MaxMp = maxMp;
            Shield = shield;
            Atk = atk;
            Def = def;
            Status = status;
        }
    }
}
