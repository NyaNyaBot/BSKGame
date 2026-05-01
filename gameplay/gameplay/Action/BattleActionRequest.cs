namespace Game.Gameplay.BattleAction
{
    /// <summary>
    /// 战斗行动请求（ADR-0012）。
    /// </summary>
    public readonly struct BattleActionRequest
    {
        public string ActionRequestId { get; }
        public string ActorInstanceId { get; }
        public string TargetInstanceId { get; }
        public BattleActionType ActionType { get; }

        public BattleActionRequest(
            string actionRequestId,
            string actorInstanceId,
            string targetInstanceId,
            BattleActionType actionType = BattleActionType.BasicAttack)
        {
            ActionRequestId = actionRequestId;
            ActorInstanceId = actorInstanceId;
            TargetInstanceId = targetInstanceId;
            ActionType = actionType;
        }
    }

    public enum BattleActionType : byte
    {
        BasicAttack = 0,
        Counter = 1,
    }
}
