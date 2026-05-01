# Story 003: UI 只读投影与快照消费

> **Epic**: character-schema-snapshot  
> **Status**: Ready  
> **Layer**: Core  
> **Type**: UI  
> **Manifest Version**: 2026-05-01

## Context

**GDD**: `design/gdd/角色数据模型.md`  
**Requirement**: `TR-char-003`、`TR-ui-001`（只读侧）  

**ADR Governing Implementation**: ADR-0011, ADR-0010  
**ADR Decision Summary**: UI 从快照与事件渲染；不得直接修改 gameplay 权威状态；绑定为只读投影。

**Engine**: Unity 2022.3.17f1 | **Risk**: LOW  
**Engine Notes**: MVP 可为最小 HUD 条或调试面板，但必须走投影 API。

**Control Manifest Rules (this layer)**:

- **Required**: UI 通过只读接口读取快照；用户操作走请求/命令路径（非本 story 全量实现时可桩）。  
- **Forbidden**: UI 侧 `repository.SetHp` 类公共 API。  
- **Guardrail**: 投影层无业务规则，仅格式化与排序。

---

## Acceptance Criteria

- [ ] 提供 `ICharacterReadModel` 或等价，从 `CharacterBattleSnapshot` + 事件驱动刷新。  
- [ ] 示例 UI（或现有战斗 HUD 最小挂钩）仅通过只读模型显示 HP/MP（MVP 字段）。  
- [ ] Code Review 可证明无 UI→domain 直接 HP 写入路径（静态分析或命名约定+测试）。

---

## Implementation Notes

- 与 `bsk-ui` 现有 Window/Panel 模式对齐；本 story 可只加 ViewModel 与一条绑定。  
- 与 `battle-event-bus`：`DamageApplied` 等触发刷新。

---

## Out of Scope

- **Story 001–002**: schema 与快照核心。  
- 完整战斗 HUD 动效（Visual/Feel 另 story）。

---

## QA Test Cases

- **AC-1**: 手动只读验证  
  - Setup: 进入带示例 HUD 的战斗桩场景  
  - Verify: 伤害事件后条变化；无 Inspector 直连改 HP  
  - Pass condition: 仅通过事件+快照更新；无 domain 公共 setter 调用栈

---

## Test Evidence

**Story Type**: UI  
**Required evidence**: `production/qa/evidence/character-ui-readonly-projection.md`  

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: story-002-battle-snapshot-version；`damage-hp-ownership` story-003（事件）建议同迭代  
- Unlocks: None（本 epic 内）
