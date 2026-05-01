# Story 003: 战斗状态渲染与无倒计时约束

> **Epic**: battle-ui
> **Status**: Ready
> **Layer**: Presentation
> **Type**: UI
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/战斗 UI.md`
**Requirement**: `TR-ui-003`

**ADR Governing Implementation**: ADR-0010
**ADR Decision Summary**: 正式 HUD 不显示精确弹反倒计时、QTE 圈或"现在按"提示；debug overlay 可显示 CombatClockMs/window 边界/ID 等信息但正式构建默认关闭。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM
**Engine Notes**: debug overlay 通过 `BattleHudViewModel.debug` 字段控制。

**Control Manifest Rules (this layer)**:

- **Required**: 正式 HUD 不显示 ms 倒计时/QTE ring/press-now 提示。
- **Forbidden**: UI 推断弹反成功来自动画/按钮状态。
- **Guardrail**: PlayMode 测试含 debug overlay 关闭时无精确窗口信息显示。

---

## Acceptance Criteria

- [ ] 进入敌方攻击阶段时，HUD 只显示弹反触区 ready 态，不显示 ms 倒计时。
- [ ] `CounterEntryOpened` 时显示 counter 可用但不显示 counter 到期倒计时。
- [ ] HP 变化通过事件驱动刷新，不轮询可变对象。
- [ ] debug overlay 默认关闭；开启时显示 CombatClockMs、phase、attackSegmentId。
- [ ] 胜负结果正确显示并提供重试/退出入口。

---

## Out of Scope

- **Story 001–002**: HUD 生命周期和触区。

---

## QA Test Cases

- **AC-1**: 无倒计时
  - Given: 敌方攻击阶段，debug overlay 关闭
  - When: 弹反窗口开启
  - Then: HUD 无 ms 数字、无 QTE 圈、无"现在按"文字

- **AC-2**: HP 事件驱动
  - Given: DamageApplied(finalDamage=15)
  - When: HUD 处理事件
  - Then: HP 条更新，无每帧轮询

---

## Test Evidence

**Story Type**: UI
**Required evidence**: PlayMode test + evidence doc `production/qa/evidence/battle-ui-state-rendering.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-001-hud-lifecycle, story-002-hit-area-registration
- Unlocks: Presentation 层联调完成
