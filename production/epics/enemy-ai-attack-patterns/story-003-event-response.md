# Story 003: 弹反/counter/重叠事件响应

> **Epic**: enemy-ai-attack-patterns
> **Status**: Complete
> **Layer**: Feature
> **Type**: Integration
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/敌人 AI 与攻击模式.md`
**Requirement**: `TR-enemy-003`

**ADR Governing Implementation**: ADR-0008, ADR-0007
**ADR Decision Summary**: AI 响应 `ParryResolved(PerfectParry)` 按 `comboInterruptRule` 中断/插入/继续后续段；`OverlapRejected` 后重排或取消——不得转为必中。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW
**Engine Notes**: 纯 C# 事件响应逻辑。

**Control Manifest Rules (this layer)**:

- **Required**: `InterruptOnPerfect` 中断后续段；`OverlapRejected` → 重排（新 timelineSequenceId）或取消。
- **Forbidden**: 被拒绝攻击翻转 `isParryable=false` 强制命中。
- **Guardrail**: 集成测试含 PerfectParry + interrupt、OverlapRejected + 重排、OverlapRejected + 取消三条路径。

---

## Acceptance Criteria

- [ ] 收到 `ParryResolved(PerfectParry)` 且 `comboInterruptRule=InterruptOnPerfect` 时，取消后续段。
- [ ] 收到 `OverlapRejected` 且可重排时，用新 `timelineSequenceId` 重排。
- [ ] 收到 `OverlapRejected` 且重排仍重叠时，取消该段（不得必中）。
- [ ] 目标在 windup 中变为不可受击时，取消当前段。

---

## Out of Scope

- **Story 001–002**: 选择和内容事实。
- counter 行动执行由 `action-system` 负责。

---

## QA Test Cases

- **AC-1**: PerfectParry 中断连击
  - Given: 3 段连击，第 1 段 Perfect
  - When: comboInterruptRule = InterruptOnPerfect
  - Then: 段 2 和 3 被取消

- **AC-2**: 重叠重排
  - Given: 段 B 与 active 段 A 重叠
  - When: OverlapRejected
  - Then: 段 B 用新 timelineSequenceId 延后 reschedule_delay_ms

- **AC-3**: 不可必中
  - Given: 重排后仍重叠
  - When: 再次 OverlapRejected
  - Then: 段取消，不翻转为不可弹反

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/enemy/event_response_integration_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-segment-content, `parry-system` story-003
- Unlocks: Feature 层联调
