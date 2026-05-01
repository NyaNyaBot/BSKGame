# Story 002: AttackSegment 内容事实与 seed 提交

> **Epic**: enemy-ai-attack-patterns
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/敌人 AI 与攻击模式.md`
**Requirement**: `TR-enemy-002`

**ADR Governing Implementation**: ADR-0008
**ADR Decision Summary**: 攻击模式拥有内容事实（baseDamage、timing、profileId、comboInterruptRule）；回合管理器拥有调度身份（attackSegmentId、timelineSequenceId）；DTO 不含 Unity 对象引用。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW（纯 C#）
**Engine Notes**: 段时间使用相对延迟 + `CombatClockMs` 绝对化；DTO 只含 primitives 和 stable IDs。

**Control Manifest Rules (this layer)**:

- **Required**: `AttackSegmentDefinition` 含 GDD 定义的全部字段；时间绝对化公式 `absolute_time_ms = segment_start_ms + relative_delay_ms`。
- **Forbidden**: DTO 含 `GameObject`、`Transform`、`AnimationClip`、`ScriptableObject`、Unity instance IDs。
- **Guardrail**: seed 完整性校验（所有 required 字段非空且范围合法）。

---

## Acceptance Criteria

- [ ] 选中 pattern 后，为每个 segment 生成包含 GDD 定义全部字段的 `AttackSegmentTimelineSeed`。
- [ ] seed 内容只含 primitives、stable IDs 和 enums——无 Unity 对象引用。
- [ ] 多段连击的 comboIndex 从 0 递增，每段独立 profileId 和 timing。
- [ ] 提交的 seed 由回合管理器分配 `attackSegmentId` 和 `timelineSequenceId`。

---

## Out of Scope

- **Story 001**: 模式选择逻辑。
- **Story 003**: 弹反/counter/重叠响应。

---

## QA Test Cases

- **AC-1**: seed 完整性
  - Given: 选中 pattern 有 2 段
  - When: 生成 seed
  - Then: 两个 seed 各含全部 required 字段，comboIndex = 0 和 1

- **AC-2**: 无 Unity 类型
  - Given: 生成的 seed DTO
  - When: 反射检查字段类型
  - Then: 无 UnityEngine 命名空间类型

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/enemy/segment_content_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-pattern-selector
- Unlocks: story-003-event-response
