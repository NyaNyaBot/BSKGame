# Story 004: Counter 入口开启与关闭生命周期

> **Epic**: parry-system
> **Status**: Complete
> **Layer**: Feature
> **Type**: Integration
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/实时弹反系统.md`
**Requirement**: `TR-parry-004`

**ADR Governing Implementation**: ADR-0007, ADR-0006
**ADR Decision Summary**: `PerfectParry` 是唯一打开 counter 入口的结果；`CounterEntryOpened` 携带来源 `resolutionId`、`attackSegmentId`、`expiresAtMs`；counter 输入在 CounterEntryOpened drain 之后的下一 queue phase 才有效；counter 关闭由回合管理器拥有。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH（触区优先级、输入时序）
**Engine Notes**: Counter 触区 priority 300 > Parry 100；counter active 时 parry 不 block。

**Control Manifest Rules (this layer)**:

- **Required**: 只有 `PerfectParry` 开 counter；counter closure 由回合管理器广播 `CounterEntryClosed`；弹反系统只发 `CounterEntryOpened`。
- **Forbidden**: Normal/Failed 打开 counter；UI 按钮可见即自行创建 counter。
- **Guardrail**: 集成测试含 counter 消耗、过期、取消三种关闭路径。

---

## Acceptance Criteria

- [ ] `PerfectParry` 成立时发布 `CounterEntryOpened`（含 resolutionId、attackSegmentId、timelineSequenceId、expiresAtMs）。
- [ ] Normal/Failed 不发布 `CounterEntryOpened`。
- [ ] counter 被消耗、过期或取消时由回合管理器发布 `CounterEntryClosed`（只广播一次）。
- [ ] counter 输入在 `CounterEntryOpened` drain 后的下一 queue phase 才有效——同 tick 的触摸不回溯为 counter。

---

## Implementation Notes

- 弹反系统只负责发布 `CounterEntryOpened`；counter 关闭和 `CounterEntryClosed` 由回合管理器和行动系统协作。
- 与 `battle-ui`：UI 在收到 `CounterEntryOpened` 后注册 priority 300 counter 触区。
- 与 `action-system`：`CounterAction` 必须引用 `CounterEntryOpened` 来源。

---

## Out of Scope

- **Story 001–003**: timeline、结算、事件发布。
- `action-system` story-002 负责 counter 行动执行。

---

## QA Test Cases

- **AC-1**: Perfect 打开 counter
  - Given: ParryResolved(PerfectParry)
  - When: 事件发布
  - Then: 同 tick 发布 CounterEntryOpened

- **AC-2**: Non-perfect 不打开
  - Given: ParryResolved(NormalParry)
  - When: 事件发布
  - Then: 无 CounterEntryOpened

- **AC-3**: Counter 关闭幂等
  - Given: counter 已消耗
  - When: 过期触发器再到达
  - Then: 不重复发布 CounterEntryClosed

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `tests/integration/parry/counter_lifecycle_integration_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-003-parry-events, `turn-manager-combat-clock` story-002
- Unlocks: `action-system` story-002, `battle-ui` story-002
