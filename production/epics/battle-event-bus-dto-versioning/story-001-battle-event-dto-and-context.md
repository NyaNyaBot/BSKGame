# Story 001: 战斗事件 DTO 与 SceneEventContext

> **Epic**: battle-event-bus-dto-versioning  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/回合管理器.md`（事件契约跨系统）  
**Requirement**: `TR-turn-002`（事件形态基础）、`TR-scene-002`（context 守卫协同）  

**ADR Governing Implementation**: ADR-0002  
**ADR Decision Summary**: 轻量同步进程内总线；`IBattleEvent` 不可变 DTO，携带 `SceneEventContext`、`EventId`、`SchemaVersion`、`OccurredAtCombatClockMs`；状态变更类事件带幂等键或源 ID。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW；MEDIUM（WebGL 域重载）  
**Engine Notes**: DTO 无 Unity 对象引用；MVP `SchemaVersion` 从 1 起。

**Control Manifest Rules (this layer)**:

- **Required**: 事件不可变；稳定 gameplay ID；拒绝/取消为一等事件；owner 系统发布权威事实。  
- **Forbidden**: 可变 payload 或 Unity 引用进入 DTO。  
- **Guardrail**: O(订阅者/类型)；可记录 enqueue/drain 轨迹（Debug）。

---

## Acceptance Criteria

- [ ] 定义 `IBattleEvent` 及基元/context 字段，与 control-manifest 列名一致。  
- [ ] 发布路径在构造后冻结 DTO（readonly struct 或不可变类）。  
- [ ] Context 不匹配时订阅回调不被调用或收到明确拒绝语义（与 scene epic 对齐）。

---

## Implementation Notes

- Owner/consumers 分离：消费者不直接写他系统拥有的权威状态。  
- `SchemaVersion` 与序列化/演进策略在 ADR-0002 指引下最小实现。

---

## Out of Scope

- **Story 002**: 入队与同 tick drain 顺序。  
- **Story 003**: HybridCLR 泛型桥接细节。

---

## QA Test Cases

- **AC-1**: DTO 不可变  
  - Given: 已构造事件实例  
  - When: 尝试变更字段（反射或 API）  
  - Then: 编译期或运行期阻止；对外只读视图  
  - Edge cases: 大结构体拷贝成本（若可测）

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/battle_events/battle_event_dto_test.cs`  

**Status**: [x] `client/Assets/Tests/EditMode/Events/BattleEventDtoTests.cs` — 8 test functions, 14/14 passed (2026-05-01)

---

## Dependencies

- Depends on: `scene-lifecycle-context` story-001（context 字段语义）  
- Unlocks: story-002-enqueue-deterministic-drain
