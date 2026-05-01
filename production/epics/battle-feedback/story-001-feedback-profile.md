# Story 001: 反馈事件消费与 profile 查表

> **Epic**: battle-feedback
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Logic
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/战斗反馈系统.md`
**Requirement**: `TR-fx-001`

**ADR Governing Implementation**: ADR-0009
**ADR Decision Summary**: 反馈服务在 Unity 热更层消费不可变战斗事件；按 `feedbackKind` 和 `intensity` 查表选择 profile；幂等键 `sourceEventId`；stale `SceneEventContext` 丢弃。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM（URP/VFX/Camera）
**Engine Notes**: profile 查表为纯逻辑；VFX/音频播放在 Unity 适配层。

**Control Manifest Rules (this layer)**:

- **Required**: 只消费事件不修改战斗事实；Critical readability 每 tier 至少一个视觉或音频信号。
- **Forbidden**: 各战斗系统直接触发 VFX/audio 作为主集成。
- **Guardrail**: PlayMode 测试含 Perfect/Normal/Failed 差异 profile 选择。

---

## Acceptance Criteria

- [ ] `ParryResolved(PerfectParry)` → `FeedbackRequest(feedbackKind=PerfectParry, intensity=Critical)`。
- [ ] `ParryResolved(NormalParry)` → 弱于 Perfect 的 profile（intensity=Medium 或 High）。
- [ ] `ParryResolved(FailedParry)` + `DamageApplied` → 受击反馈。
- [ ] `ParryCancelled` → 清理 telegraph cue，不播放失败反馈。
- [ ] 同一 `sourceEventId` 重复到达时不重复播放关键反馈。
- [ ] stale `SceneEventContext` 事件被丢弃。

---

## Out of Scope

- **Story 002**: HitStopRequest。
- **Story 003**: 性能降级。

---

## QA Test Cases

- **AC-1**: Perfect profile
  - Given: ParryResolved(PerfectParry)
  - When: 反馈服务处理
  - Then: FeedbackRequest.intensity = Critical, feedbackKind = PerfectParry

- **AC-2**: 幂等
  - Given: 同一 resolutionId 的 ParryResolved 到达两次
  - When: 第二次处理
  - Then: 不重复创建 FeedbackRequest

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `tests/unit/feedback/feedback_profile_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `parry-system` story-003, `damage-hp-ownership` story-003
- Unlocks: story-002-hitstop-request
