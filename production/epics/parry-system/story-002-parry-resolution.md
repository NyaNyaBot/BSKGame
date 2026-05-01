# Story 002: ParryAttempt 结算与 grade 判定

> **Epic**: parry-system
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/实时弹反系统.md`
**Requirement**: `TR-parry-002`

**ADR Governing Implementation**: ADR-0007, ADR-0004
**ADR Decision Summary**: 弹反结算使用 `CombatClockMs`（不使用 Unity 帧时间）；`IParryResolver.ResolveAttempt` 计算 `timingOffsetMs` 和 grade（Perfect/Normal/Failed）；每个 `attackSegmentId + timelineSequenceId` 最多一个终结结果。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH（低 FPS / 触摸时序）
**Engine Notes**: 判定逻辑为纯 C#；hit stop 期间已结算结果不重算。

**Control Manifest Rules (this layer)**:

- **Required**: `effective_input_timestamp_ms` 按公式计算（含 buffer 和 latency_compensation）；grade 按 `timingOffsetMs` vs `perfect_half_window_ms` 判定。
- **Forbidden**: 用 `Unity Time.time` 作为弹反判定权威时间。
- **Guardrail**: 窗口边界（early/perfect/late/none）全覆盖单元测试。

---

## Acceptance Criteria

- [ ] 输入命中 perfect window 时产生 `PerfectParry`（`damageMultiplier=0.0`，`echoIntent=High`）。
- [ ] 输入命中 normal window 但非 perfect 时产生 `NormalParry`（`damageMultiplier=0.5`，`echoIntent=None`）。
- [ ] 窗口关闭且无输入时产生 `FailedParry`（`damageMultiplier=1.0`，`failureReason=NoInput`）。
- [ ] 缓冲输入按 `window_open_timestamp_ms` 参与结算。
- [ ] 同一 `attackSegmentId + timelineSequenceId` 不产生两个终结结果。

---

## Implementation Notes

- 与 `battle-input-touch-and-hit-areas`：`ParryAttempt.battleTimestampMs` 由输入系统提供。
- `ResolveNoInput` 用于窗口关闭且无输入的场景。
- `resolutionId` 保持幂等——重复调用返回已有结果。

---

## Out of Scope

- **Story 001**: timeline 派生。
- **Story 003**: 事件发布。
- **Story 004**: counter 生命周期。

---

## QA Test Cases

- **AC-1**: Perfect 判定
  - Given: `battleTimestampMs` 在 `[perfectStartMs, perfectEndMs]` 内
  - When: `ResolveAttempt(attempt)`
  - Then: grade = PerfectParry, damageMultiplier = 0.0, echoIntent = High

- **AC-2**: Normal 判定
  - Given: `battleTimestampMs` 在 `[windowOpenMs, perfectStartMs)` 或 `(perfectEndMs, normalEndMs]`
  - When: `ResolveAttempt(attempt)`
  - Then: grade = NormalParry, damageMultiplier = 0.5

- **AC-3**: Failed（无输入）
  - Given: `CombatClockMs > normalEndMs` 且无 `ParryAttempt`
  - When: `ResolveNoInput(attackSegmentId, timelineSequenceId)`
  - Then: grade = FailedParry, failureReason = NoInput

- **AC-4**: 缓冲输入
  - Given: 输入在 `windowOpenMs - input_buffer_ms` 到 `windowOpenMs` 之间
  - When: 窗口开启后结算
  - Then: 使用 `window_open_timestamp_ms` 参与判定

- **AC-5**: 幂等
  - Given: 已为某段产生结果
  - When: 同段再次调用 `ResolveAttempt`
  - Then: 返回原始结果，不产生新终结事件

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/parry/parry_resolution_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-frozen-timeline-derivation
- Unlocks: story-003-parry-events
