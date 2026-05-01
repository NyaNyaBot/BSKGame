namespace Game.Gameplay.SceneLifecycle
{
    /// <summary>
    /// 开局请求占位；后续可承载 seed、关卡 id 等（ADR-0001 CreateBattleContext 参数）。
    /// </summary>
    public readonly struct StartBattleRequest
    {
        public StartBattleRequest(string? correlationId = null)
        {
            CorrelationId = correlationId;
        }

        public string? CorrelationId { get; }
    }
}
