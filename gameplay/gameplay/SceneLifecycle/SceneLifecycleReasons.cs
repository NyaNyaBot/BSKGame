namespace Game.Gameplay.SceneLifecycle
{
    public enum SceneTransitionReason : byte
    {
        Unknown = 0,
        UnloadStarting = 1,
        Retry = 2,
        HotReload = 3,
    }

    public enum BattleDisposeReason : byte
    {
        Unknown = 0,
        SceneUnload = 1,
        BattleRestart = 2,
        VictoryOrDefeat = 3,
        UserAbort = 4,
        NormalEnd = 5,
    }
}
