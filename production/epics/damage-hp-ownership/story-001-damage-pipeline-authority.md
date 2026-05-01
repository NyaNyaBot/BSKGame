# Story 001: 权威 DamagePipeline 与 DamageRequestId

> **Epic**: damage-hp-ownership  
> **Status**: Ready  
> **Layer**: Core  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/伤害与生命系统.md`  
**Requirement**: `TR-dmg-001`  

**ADR Governing Implementation**: ADR-0005  
**ADR Decision Summary**: 所有 HP 变更经 `IDamageService.ApplyDamage(DamageRequest)`；`DamageRequestId` 全局唯一；重复 ID 返回原结果不双扣；伤害倍数用整数 basis points。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW  
**Engine Notes**: `DamageRequest` 与 GDD 公式字段对齐；float 倍数在边界转换。

**Control Manifest Rules (this layer)**:

- **Required**: 单一伤害入口；终端结果事件唯一（见 story-003）。  
- **Forbidden**: Feature/UI 直接改 HP。  
- **Guardrail**: 预检失败快速路径低成本。

---

## Acceptance Criteria

- [ ] 实现 `IDamageService` 与 `DamageRequest`/`DamageRequestId` 模型。  
- [ ] 同一 `DamageRequestId` 第二次调用返回与第一次相同终端结果且不重复应用。  
- [ ] 伤害最终值 clamp `>= 0`（与 ADR 一致）。

---

## Implementation Notes

- 与 `character-schema-snapshot`：通过 repository 受控钩子写 HP。  
- 与 ADR-0012：`IBattleActionService` 将来只产 `DamageRequest`，本 story 可不实现 action 层。

---

## Out of Scope

- **Story 002**: 护盾吸收顺序。  
- **Story 003**: 事件广播形态。

---

## QA Test Cases

- **AC-1**: 幂等 ID  
  - Given: 有效请求 id=X  
  - When: ApplyDamage 两次  
  - Then: HP 只变一次；两次返回值一致  
  - Edge cases: 并发（若单线程则文档化）

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/damage/damage_pipeline_idempotency_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `character-schema-snapshot` story-001–002  
- Unlocks: story-002-shield-hp-order
