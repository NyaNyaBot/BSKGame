namespace Game.Gameplay.Character
{
    /// <summary>
    /// 角色静态定义（ADR-0011 / TR-char-001）。
    /// 来自数据表配置，战斗运行时不可变。
    /// CharacterId 用于标识角色模板（如 "warrior_01"）。
    /// </summary>
    public sealed class CharacterDefinition
    {
        public string CharacterId { get; }
        public string DisplayName { get; }
        public int BaseMaxHp { get; }
        public int BaseMaxMp { get; }
        public int BaseAtk { get; }
        public int BaseDef { get; }

        public CharacterDefinition(
            string characterId,
            string displayName,
            int baseMaxHp,
            int baseMaxMp,
            int baseAtk,
            int baseDef)
        {
            CharacterId = characterId;
            DisplayName = displayName;
            BaseMaxHp = baseMaxHp;
            BaseMaxMp = baseMaxMp;
            BaseAtk = baseAtk;
            BaseDef = baseDef;
        }
    }
}
