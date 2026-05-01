# Story 001: BasicAttackAction 授权与 DamageRequest 提交

> **Epic**: action-system
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/技能与行动系统.md`
**Requirement**: `TR-action-001`, `TR-action-002`

**ADR Governing Implementation**: ADR-0012, ADR-0005
**ADR Decision Summary**: `IBattleActionService.Submit` 校验 `BattleInputPhase.PlayerCommand`、actor `CanAct`、target `CanBeTargeted`，生成 `DamageRequest`（integer basis points multiplier），保持 `BattleActionRequestId` 幂等。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW（纯 C#）
**Engine Notes**: 行动逻辑为纯 C#；动画完成不回滚已 commit 伤害。

**Control Manifest Rules (this layer)**:

- **Required**: `BasicAttackAction` 只在 `PlayerCommand` 阶段接受；`action_base_damage = round_half_up(attack_power * power_multiplier)`；通过 `DamageRequest` 提交。
- **Forbidden**: 行动系统直接写 HP；UI 直接调 `IDamageService`。
- **Guardrail**: 单元测试含阶段匹配/不匹配、目标合法/不合法、幂等。

---

## Acceptance Criteria

- [ ] `PlayerCommand` 阶段且 actor/target 合法时，Submit 成功并生成一个 `DamageRequest`。
- [ ] 非 `PlayerCommand` 阶段提交时，请求被拒绝且不造成伤害。
- [ ] 目标 `CanBeTargeted=false` 时，请求被拒绝。
- [ ] `action_base_damage` 按 GDD 公式计算。
- [ ] 同一 `actionRequestId` 重复提交不重复生成 `DamageRequest`。

---

## Out of Scope

- **Story 002**: CounterAction。
- **Story 003**: 场景/阶段过期集成。

---

## QA Test Cases

- **AC-1**: 合法提交
  - Given: PlayerCommand, actor CanAct, target CanBeTargeted
  - When: Submit(BasicAttackAction)
  - Then: 一个 DamageRequest，damage = round_half_up(20 * 1.0) = 20

- **AC-2**: 阶段不匹配
  - Given: inputPhase = EnemyAttack
  - When: Submit(BasicAttackAction)
  - Then: rejected, reason = PhaseMismatch

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/action/basic_attack_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `character-schema-snapshot` story-001, `damage-hp-ownership` story-001
- Unlocks: story-002-counter-action
