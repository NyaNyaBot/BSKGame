namespace Game.Gameplay.SceneLifecycle
{
    /// <summary>
    /// 可玩场景分类（MVP 子集，可随场景表扩展）。
    /// </summary>
    public enum SceneKind : byte
    {
        Unknown = 0,
        Menu = 1,
        Battle = 2,
    }
}
