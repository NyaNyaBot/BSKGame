# Story 003: DamageApplied / DamageRejected / CharacterDefeated 事件

> **Epic**: damage-hp-ownership  
> **Status**: Ready  
> **Layer**: Core  
> **Type**: Integration  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/伤害与生命系统.md`  
**Requirement**: `TR-dmg-003`、`TR-dmg-004`  

**ADR Governing Implementation**: ADR-0005, ADR-0002  
**ADR Decision Summary**: 每个 `DamageRequestId` 恰好一个终端结果：`DamageApplied` 或 `DamageRejected`；满足击败转换时发 `CharacterDefeated`；事件为不可变 DTO 并携带 context 与逻辑时钟。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW  
**Engine Notes**: 击败事件每个击败转换一次；与 ADR-0011 `Defeated` 语义一致。

**Control Manifest Rules (this layer)**:

- **Required**: 终端结果唯一；DTO 符合 ADR-0002。  
- **Forbidden**: 以日志代替拒绝事件。  
- **Guardrail**: UI/反馈只消费事件不反向写 HP（与 character epic 对齐）。

---

## Acceptance Criteria

- [ ] 成功应用伤害发布 `DamageApplied`，字段含最终值、request id、`InstanceId` 等。  
- [ ] 预检/规则拒绝发布 `DamageRejected` 且不改 HP（与 story-002 一致）。  
- [ ] HP 降至 0 且满足击败条件时发布 `CharacterDefeated` 一次（重复伤害不重复击败，按 ADR）。

---

## Implementation Notes

- 发布点仅在 `IDamageService` 内部权威路径；与总线 `Publish` 泛型保留策略一致。  
- 与 `battle-event-bus-dto-versioning`：事件入队与 tick drain 顺序已在 turn epic 定义。

---

## Out of Scope

- **Story 001–002**: 管道与护盾顺序。  
- 弹反窗口与 `ParryResolved`（另系统 epic）。

---

## QA Test Cases

- **AC-1**: 拒绝无 Applied  
  - Given: 非法目标  
  - When: ApplyDamage  
  - Then: 仅 `DamageRejected`；HP 不变；无 Applied  
  - Edge cases: 0 伤害请求；重复 id

---

## Test Evidence

**Story Type**: Integration  
**Required evidence**: `tests/integration/damage/damage_events_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-shield-hp-order；`battle-event-bus-dto-versioning` story-001  
- Unlocks: `character-schema-snapshot` story-003（UI 刷新）
