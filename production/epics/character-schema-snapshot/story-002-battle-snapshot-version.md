# Story 002: CharacterBattleSnapshot 不可变与版本

> **Epic**: character-schema-snapshot  
> **Status**: Complete  
> **Layer**: Core  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/角色数据模型.md`  
**Requirement**: `TR-char-002`  

**ADR Governing Implementation**: ADR-0011  
**ADR Decision Summary**: 快照不可变并带 `Version`；权威状态变更后递增版本；UI/只读消费者依赖快照而非可变引用。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW  
**Engine Notes**: 大列表快照策略（拷贝 vs 结构共享）以 MVP 可接受为准，需性能注释。

**Control Manifest Rules (this layer)**:

- **Required**: 快照 `Version` 单调；不可变语义对外可见。  
- **Forbidden**: 将可变集合引用泄漏给 UI。  
- **Guardrail**: 高频战斗下快照生成成本可测（基准可选）。

---

## Acceptance Criteria

- [ ] `CharacterBattleSnapshot`（或等价）构造后字段不可变。  
- [ ] 任意权威字段变更（经允许路径）后 `Version` 递增且旧快照不变。  
- [ ] 提供从 repository 获取当前快照的只读 API。

---

## Implementation Notes

- 与 `damage-hp-ownership`：伤害提交后触发快照刷新与版本 bump（单点）。  
- 与事件：`OccurredAt` 或版本可挂钩便于 UI diff（若 GDD 要求）。

---

## Out of Scope

- **Story 001**: schema 与实例模型。  
- **Story 003**: UI 绑定模式。

---

## QA Test Cases

- **AC-1**: 旧快照不随新伤害变化  
  - Given: 快照 V1  
  - When: `ApplyDamage` 成功  
  - Then: V1 数据不变；新快照 V2 反映变化  
  - Edge cases: 拒绝伤害时版本策略（应不 bump 或 bump「尝试」按 ADR）

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/character/character_battle_snapshot_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-authoritative-character-schema  
- Unlocks: story-003-ui-readonly-projection
