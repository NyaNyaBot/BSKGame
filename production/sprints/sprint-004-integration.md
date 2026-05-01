# Sprint 004 — Integration Layer

> **Status**: Complete
> **Start**: 2026-05-02
> **Layer**: Integration (全系统串联)
> **Prerequisite**: Sprint 000-003 共 38 stories 已完成, 225 EditMode 测试全绿

## Goal

将 Foundation/Core/Feature/Presentation 四层独立系统串联为可运行的战斗循环。
创建 BattleOrchestrator（纯 C# 聚合根）和 BattleFlowController（Unity 侧 tick 驱动），
并编写端到端集成测试验证完整战斗流程。

## Scope

| # | Story | Priority | Status |
|---|-------|----------|--------|
| 1 | BattleOrchestrator — 纯 C# 系统联调器 | P0 | Done |
| 2 | BattleFlowController — Unity 侧 tick 驱动 + HUD | P0 | Done |
| 3 | Integration EditMode 测试 — 全流程验证 | P0 | Done |
| 4 | TempGameState 对接 — 场景加载后启动 | P1 | Done |

## System Wiring Order

```
SceneContextService (已有)
  └→ BattleEventBus(sceneContext)
     ├→ CombatClock()
     ├→ InputHitAreaRegistry()
     └→ BattleSession(id, ctx, bus, clock, registry)

CharacterRepository()
  ├→ CharacterReadModel(repo)
  ├→ DamageService(repo)
  ├→ BattleActionService(phaseMachine, repo)
  ├→ CounterActionService(repo, clock)
  └→ EnemyAttackPlanner(repo)

InputPhaseStateMachine()
CombatTickRunner(clock, bus)
ParryResolver()
FeedbackProfileLookup()
HitStopController(clock)
```

## Acceptance Criteria

- BattleOrchestrator.Start() 创建所有系统并注册 CombatTickRunner 步骤
- BattleOrchestrator.Tick(deltaMs) 驱动一帧完整战斗逻辑
- BattleOrchestrator.Dispose() 按序拆解所有系统
- 集成测试覆盖：开战 → 玩家攻击 → 敌人回合 → 弹反 → 伤害结算 → 角色死亡 → 战斗结束
- TempGameState.OnLoadSceneSuccess 创建 BattleFlowController
