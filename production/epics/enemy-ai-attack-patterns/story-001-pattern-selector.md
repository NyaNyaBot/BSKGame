# Story 001: 攻击模式选择器（权重/冷却/HP 阶段）

> **Epic**: enemy-ai-attack-patterns
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/敌人 AI 与攻击模式.md`
**Requirement**: `TR-enemy-001`

**ADR Governing Implementation**: ADR-0008
**ADR Decision Summary**: `IEnemyAttackPlanner` + `IAttackPatternCatalog` 在纯 gameplay 层；按权重/冷却/HP 阶段/目标合法性确定性选择；注入确定性 RNG 或固定种子。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW（纯 C#）
**Engine Notes**: 不使用 Unity Random、wall-clock、frame count、ScriptableObject instance identity。

**Control Manifest Rules (this layer)**:

- **Required**: 确定性 RNG 或固定种子；pattern_score 公式按 GDD 实现。
- **Forbidden**: Unity `Random`、wall-clock、frame count 作为选择输入；GOAP/BT for MVP。
- **Guardrail**: 固定种子 + 固定快照下 100 次选择结果 deterministic。

---

## Acceptance Criteria

- [ ] 敌方阶段开始且有合法目标时，至少一个 pattern score > 0 则选择并返回 `EnemyActionPlan`。
- [ ] 处于冷却的 pattern score 为 0。
- [ ] HP 阶段不匹配的 pattern score 为 0。
- [ ] 无合法目标时不提交攻击。
- [ ] 固定种子和快照下，连续 100 次选择 deterministic。

---

## Implementation Notes

- `AttackPatternDefinition` 数据来自配置（MVP 可硬编码或数据表）。
- 目标合法性来自 `ICharacterRepository.GetSnapshot()`。
- 与 `turn-manager-combat-clock`：回合管理器请求 `IEnemyAttackPlanner.SelectAction(query)` 时提供当前回合数和上下文。

---

## Out of Scope

- **Story 002**: 段内容事实和 seed 提交。
- **Story 003**: 事件响应。

---

## QA Test Cases

- **AC-1**: 冷却排除
  - Given: pattern A 冷却中，pattern B 可用
  - When: 选择
  - Then: 只可能选 B

- **AC-2**: HP 阶段过滤
  - Given: 敌人 HP > 50%，pattern 要求 BelowHalf
  - When: 计算 score
  - Then: score = 0

- **AC-3**: 确定性
  - Given: 固定种子 42、固定快照
  - When: 连续 100 次 SelectAction
  - Then: 结果序列完全相同

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/enemy/pattern_selector_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `character-schema-snapshot` story-001（角色快照 API）
- Unlocks: story-002-segment-content
