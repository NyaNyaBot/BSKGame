# Story 001: BattleHudWindow 生命周期与事件订阅

> **Epic**: battle-ui
> **Status**: Ready
> **Layer**: Presentation
> **Type**: UI
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/战斗 UI.md`
**Requirement**: `TR-ui-001`

**ADR Governing Implementation**: ADR-0010
**ADR Decision Summary**: UIFrame Window + Panels 组合；UI 订阅事件和快照驱动显示；场景卸载清理全部订阅和触区。

**Engine**: Unity 2022.3.17f1 | **Risk**: MEDIUM（UIFrame/UGUI）
**Engine Notes**: 使用项目已有 UIFrame Window/Panel 架构。

**Control Manifest Rules (this layer)**:

- **Required**: 事件驱动不轮询；battle-scoped 订阅；dispose 清理全部。
- **Forbidden**: 每帧轮询可变 gameplay 对象。
- **Guardrail**: PlayMode 测试含 HUD 打开/关闭/场景卸载清理。

---

## Acceptance Criteria

- [ ] `BattleStarted` 时 HUD 初始化，读取初始角色/敌人快照填充 `BattleHudViewModel`。
- [ ] 订阅阶段、HP、行动、counter、弹反结果事件，事件到达时更新 ViewModel。
- [ ] 场景卸载时清理所有事件订阅，不残留回调。
- [ ] HUD Panel 组合：PlayerStatusPanel、EnemyStatusPanel、ActionCommandPanel 按 UIFrame 规范分层。

---

## Out of Scope

- **Story 002**: 触区注册。
- **Story 003**: 状态渲染细节。

---

## QA Test Cases

- **AC-1**: 初始化
  - Given: BattleStarted
  - When: HUD 打开
  - Then: playerHp、enemySummaries 正确

- **AC-2**: 清理
  - Given: 场景卸载
  - When: HUD dispose
  - Then: 事件订阅数 = 0

---

## Test Evidence

**Story Type**: UI
**Required evidence**: PlayMode test + evidence doc `production/qa/evidence/battle-hud-lifecycle.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `scene-lifecycle-context` story-001, `turn-manager-combat-clock` story-002
- Unlocks: story-002-hit-area-registration
