# Story 002: 护盾吸收与 HP 扣减顺序

> **Epic**: damage-hp-ownership  
> **Status**: Complete  
> **Layer**: Core  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/伤害与生命系统.md`  
**Requirement**: `TR-dmg-002`  

**ADR Governing Implementation**: ADR-0005  
**ADR Decision Summary**: 预检 → 护盾吸收 → HP 扣减 → 死亡判定；HP clamp `[0, MaxHp]`；护盾与多段伤害边界按 GDD。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW  
**Engine Notes**: 整数/定点策略若 GDD 指定则遵守；否则与 ADR basis points 一致。

**Control Manifest Rules (this layer)**:

- **Required**: 顺序与 GDD Acceptance 一致；拒绝路径不修改状态。  
- **Forbidden**: 在护盾逻辑中偷偷写非 HP 的无关权威字段。  
- **Guardrail**: 极端大伤害与满盾交互单测。

---

## Acceptance Criteria

- [ ] 护盾先于 HP 吸收；溢出伤害正确传递到 HP（按 GDD）。  
- [ ] HP/护盾变更后快照版本策略与 `character-schema-snapshot` story-002 一致。  
- [ ] 预检失败产生 `DamageRejected`（事件形态在 story-003，本 story 可先内部枚举）。

---

## Implementation Notes

- 管道内步骤应可单测注入（纯函数子模块）。  
- 与 `turn-manager-combat-clock` story-004：伤害子步仅在编排内调用。

---

## Out of Scope

- **Story 001**: ID 与入口幂等。  
- **Story 003**: 事件 DTO 与总线发布。

---

## QA Test Cases

- **AC-1**: 部分护盾击穿  
  - Given: 护盾 S，伤害 D>S  
  - When: ApplyDamage  
  - Then: 护盾归零；HP 扣 (D-S)；无负护盾  
  - Edge cases: S=0；D=0；满 HP 满盾

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/damage/shield_hp_order_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-damage-pipeline-authority  
- Unlocks: story-003-damage-events
