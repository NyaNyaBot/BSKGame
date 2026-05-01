namespace Game.Gameplay.Damage
{
    /// <summary>
    /// 权威伤害服务接口（ADR-0005）。
    /// 所有 HP 变更的唯一入口。
    /// </summary>
    public interface IDamageService
    {
        DamageResult ApplyDamage(in DamageRequest request);
    }
}
