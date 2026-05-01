# Story 003: 行动幂等与场景/阶段过期拒绝

> **Epic**: action-system
> **Status**: Complete
> **Layer**: Feature
> **Type**: Integration
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/技能与行动系统.md`
**Requirement**: `TR-action-002`

**ADR Governing Implementation**: ADR-0012
**ADR Decision Summary**: `BattleActionRequestId` 全局唯一；重复 ID 返回原结果不重复 DamageRequest；场景/战斗上下文过期的行动直接拒绝。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW
**Engine Notes**: 幂等缓存 battle-scoped；场景卸载时清理。

**Control Manifest Rules (this layer)**:

- **Required**: 重复 actionRequestId 返回原始结果；stale SceneEventContext 拒绝。
- **Forbidden**: 跳过 context 校验。
- **Guardrail**: 集成测试含 BasicAttack → DamageApplied → 重复 → 无新 DamageRequest。

---

## Acceptance Criteria

- [ ] 同一 `actionRequestId` 重复提交时，返回原始结果且不生成新 `DamageRequest`。
- [ ] 场景上下文过期（`sceneVersion` 不匹配）时，行动直接拒绝。
- [ ] 战斗上下文过期（`battleVersion` 不匹配）时，行动直接拒绝。
- [ ] 场景卸载后幂等缓存被清理，新战斗不受旧 ID 影响。

---

## Out of Scope

- **Story 001–002**: 具体行动逻辑。

---

## QA Test Cases

- **AC-1**: 幂等
  - Given: BasicAttack with id="act-001" 已成功
  - When: 再次 Submit id="act-001"
  - Then: 返回原结果，DamageRequest 总数不变

- **AC-2**: context 过期
  - Given: sceneVersion = 2, 行动携带 sceneVersion = 1
  - When: Submit
  - Then: rejected, reason = StaleContext

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/action/action_idempotency_integration_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-basic-attack, story-002-counter-action
- Unlocks: Feature 层联调完成
