# Story 001: 权威角色 Schema

> **Epic**: character-schema-snapshot  
> **Status**: Complete  
> **Layer**: Core  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/角色数据模型.md`  
**Requirement**: `TR-char-001`  

**ADR Governing Implementation**: ADR-0011  
**ADR Decision Summary**: `ICharacterRepository` 拥有定义、运行时实例、快照；`CharacterId` 与 `InstanceId` 分离；队伍为列表；事件使用 `InstanceId`。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW  
**Engine Notes**: 纯 C# 模型优先；与伤害服务的 HP 变更钩子衔接留 story-002/003。

**Control Manifest Rules (this layer)**:

- **Required**: 静态定义与运行时实例 id 分离；无公共任意 HP setter。  
- **Forbidden**: UI 直接写权威 HP/MP。  
- **Guardrail**: schema 字段变更需同步 TR/registry。

---

## Acceptance Criteria

- [ ] 文档化权威角色字段（HP/MP/护盾/状态/与 GDD 对齐的 MVP 子集）。  
- [ ] 运行时实例创建/查询 API 与 `InstanceId` 生成策略。  
- [ ] `Defeated` 与 `Removed` 语义不混淆（与 GDD/ADR 一致）。

---

## Implementation Notes

- 与 `damage-hp-ownership`：仅暴露 `IDamageService` 使用的受控变更钩子。  
- 与 `battle-event-bus`：事件载荷使用 `InstanceId`。

---

## Out of Scope

- **Story 002**: 快照不可变与版本。  
- **Story 003**: UI 投影。

---

## QA Test Cases

- **AC-1**: 创建多实例 id 唯一  
  - Given: 同 `CharacterId` 多实例加入战斗  
  - When: 分配 `InstanceId`  
  - Then: 全局唯一；查询正确  
  - Edge cases: 重复加入拒绝或幂等（按 GDD）

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/character/character_schema_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `gameplay-unity-hybridclr-layering` story-001（程序集归属）  
- Unlocks: story-002-battle-snapshot-version
