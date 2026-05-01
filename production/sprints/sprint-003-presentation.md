# Sprint 003 — Presentation Layer

> **Status**: Partial — 纯 C# 逻辑完成，Unity UI 待人工
> **Start**: 2026-05-01
> **Layer**: Presentation (battle-feedback + battle-ui)

## Scope

| # | Epic | Story | Priority | Status |
|---|------|-------|----------|--------|
| 1 | battle-feedback | story-001: 反馈 profile 查表 | P0 | ✅ Complete |
| 2 | battle-feedback | story-002: HitStop 裁决 | P0 | ✅ Complete |
| 3 | battle-feedback | story-003: 质量降级 | P0 | ✅ Complete |
| 4 | battle-ui | story-001: HUD 生命周期 | P0 | ⏸️ 需人工 |
| 5 | battle-ui | story-002: 触区注册 | P0 | ⏸️ 需人工 |
| 6 | battle-ui | story-003: 状态渲染 | P0 | ⏸️ 需人工 |

## 已完成

### battle-feedback (纯 C# 逻辑)

- **FeedbackRequest / HitStopRequest** DTO — 不可变值对象
- **FeedbackProfileLookup** — 弹反→反馈映射、HitStop 生成、幂等、质量降级
- **FeedbackBundle** — 聚合反馈 + 可选 HitStop
- **HitStopController** — CombatClock 暂停/恢复裁决、优先级覆盖

### battle-ui (纯 C# ViewModel)

- **BattleHudViewModel** — 事件驱动视图模型、无倒计时约束、debug overlay
- **BattleResultInfo / BattleOutcome** — 胜负结果 DTO

## 测试

- 225 EditMode 测试全绿
- FeedbackProfileTests (10)、HitStopControllerTests (12)、QualityDegradationTests (9)、BattleHudViewModelTests (9)

## 需要人工干预

battle-ui 的三个 story 需要以下人工操作：

1. **BattleHudWindow prefab** — 基于 UIFrame Window 架构创建 UGUI 预制体
2. **Panel 组件** — PlayerStatusPanel、EnemyStatusPanel、ActionCommandPanel
3. **触区 UI** — Parry/Counter 触区的 RectTransform 和安全区适配
4. **PlayMode 测试** — HUD 打开/关闭/场景卸载清理
5. **设备测试** — WebGL 安全区、触区最小 dp

## 测试结果

| 类别 | 数量 | 状态 |
|------|------|------|
| 总计 | 225 | ✅ 全部通过 |

## Next Sprint

Sprint 003 的 battle-feedback 纯逻辑已完成。
battle-ui 需要 Unity UI 制作和 PlayMode 测试，需人工介入。
