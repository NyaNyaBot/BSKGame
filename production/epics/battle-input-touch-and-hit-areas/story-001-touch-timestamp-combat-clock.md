# Story 001: 触摸时间戳映射到 CombatClockMs

> **Epic**: battle-input-touch-and-hit-areas  
> **Status**: Complete  
> **Layer**: Foundation  
> **Type**: Logic  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/输入系统.md`  
**Requirement**: `TR-input-001`、`TR-input-003`  

**ADR Governing Implementation**: ADR-0006, ADR-0004  
**ADR Decision Summary**: `TouchTimestampAdapter` 将平台触摸/PointerDown 映射到 `battleTimestampMs`，计算 `bridgeDelayMs`，与 `CombatClockMs` 对齐；拒绝非单调/不可信值；`latency_compensation_ms` 不由输入系统应用。

**Engine**: Unity 2022.3.17f1 | **Risk**: HIGH（微信 WebGL 触摸时序）  
**Engine Notes**: 单位与 epoch 归一后再算 `bridgeDelayMs`；与只读 `ICombatClock` 读数一致。

**Control Manifest Rules (this layer)**:

- **Required**: 公式与诊断字段与 control-manifest 一致；fallback 可观测。  
- **Forbidden**: 用 UGUI `onClick` 作为弹反/反击判定时序源。  
- **Guardrail**: Release 避免无界输入诊断日志。

---

## Acceptance Criteria

- [ ] 实现 `TouchTimestampAdapter`（或等价）输出 `battleTimestampMs`、`bridgeDelayMs`、fallback 标记。  
- [ ] 与注入的 `ICombatClock`/fake clock 的单元测试覆盖单调性、负值钳制、过大 bridge 延迟拒绝。  
- [ ] 不将 GDD 中的 `latency_compensation_ms` 在输入层二次补偿。

---

## Implementation Notes

- `battleTimestampMs = max(0, combatClock.NowMs - bridgeDelayMs)` 等公式严格按 ADR-0006。  
- 与 `turn-manager-combat-clock` 联调：时钟暂停（hit stop）时输入时间语义在 ADR-0004 指引下文档化。

---

## Out of Scope

- **Story 002**: `ParryAttempt` 结构化与诊断扩展。  
- **Story 003**: 触区注册表与优先级。

---

## QA Test Cases

- **AC-1**: bridge 延迟钳制  
  - Given: 伪造极大 `receivedRealtimeMs` 差  
  - When: 适配器归一  
  - Then: `bridgeDelayMs` 在配置上限内；标记异常路径  
  - Edge cases: 时钟回拨；零 delta

---

## Test Evidence

**Story Type**: Logic  
**Required evidence**: `tests/unit/input/touch_timestamp_adapter_test.cs`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `turn-manager-combat-clock` story-003（只读时钟）建议并行就绪  
- Unlocks: story-002-parry-attempt-structure
