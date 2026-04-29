# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 2022.3.17f1
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline)
- **Physics**: Unity Physics (基础)

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: 微信小游戏 (WebGL / 移动端浏览器)
- **Input Methods**: Touch
- **Primary Input**: Touch
- **Gamepad Support**: None
- **Touch Support**: Full
- **Platform Notes**: 微信小游戏环境运行，触屏为唯一输入方式。需注意 WebGL 下的输入延迟对弹反操作的影响。包体大小受小游戏平台限制。所有 UI 必须适配移动端触控，不依赖 hover 交互。

## Naming Conventions

- **Classes**: PascalCase (e.g., `PlayerController`)
- **Public fields/properties**: PascalCase (e.g., `MoveSpeed`)
- **Private fields**: _camelCase (e.g., `_moveSpeed`)
- **Methods**: PascalCase (e.g., `TakeDamage()`)
- **Signals/Events**: PascalCase + EventArgs 后缀 (e.g., `ParrySuccessEventArgs`)
- **Files**: PascalCase matching class (e.g., `PlayerController.cs`)
- **Scenes/Prefabs**: PascalCase (e.g., `BattleScene.unity`)
- **Constants**: PascalCase or UPPER_SNAKE_CASE

## Performance Budgets

- **Target Framerate**: 30 FPS minimum on target WeChat WebGL devices; 60 FPS stretch on high-end devices
- **Frame Budget**: 33.3 ms at 30 FPS; 16.7 ms stretch at 60 FPS
- **Draw Calls**: Combat target <= 70 draw calls during PerfectParry peak; normal gameplay target <= 50 draw calls
- **Memory Ceiling**: 256 MB runtime working set target for MVP WebGL; no unbounded combat event or feedback logs

## Testing

- **Framework**: NUnit (Unity Test Framework)
- **Minimum Coverage**: 80% line/branch coverage target for pure gameplay services once implementation begins; 100% contract coverage for timing, HP mutation, idempotency, and stale-context acceptance criteria
- **Required Tests**: Balance formulas, gameplay systems, combat clock, parry timing boundaries, HP/damage idempotency, input hit area priority, scene unload cleanup, HybridCLR/WebGL smoke evidence

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- [None configured yet — add as architectural decisions are made]

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- ADR-0001: `docs/architecture/adr-0001-scene-lifecycle-context-routing.md` — 场景生命周期与场景上下文路由
- ADR-0002: `docs/architecture/adr-0002-battle-event-bus-dto-versioning.md` — 战斗事件总线与 DTO 版本策略
- ADR-0003: `docs/architecture/adr-0003-gameplay-unity-hybridclr-layering.md` — 玩法逻辑、Unity 适配器与 HybridCLR 的运行时程序集分层
- ADR-0004: `docs/architecture/adr-0004-combat-clock-deterministic-ordering.md` — 战斗时钟与确定性战斗排序
- ADR-0005: `docs/architecture/adr-0005-damage-hp-ownership.md` — 伤害与 HP 所有权
- ADR-0006: `docs/architecture/adr-0006-mobile-touch-timestamp-hit-area-strategy.md` — 移动触摸时间戳与输入触区策略
- ADR-0007: `docs/architecture/adr-0007-parry-timeline-counter-handoff.md` — 弹反时间轴派生与 Counter Handoff
- ADR-0008: `docs/architecture/adr-0008-enemy-attack-pattern-ownership.md` — 敌方攻击模式数据与调度所有权
- ADR-0009: `docs/architecture/adr-0009-battle-feedback-quality-hitstop.md` — 战斗反馈质量分级与 WebGL 降级策略
- ADR-0010: `docs/architecture/adr-0010-uiframe-battle-hud-composition.md` — UIFrame 战斗 HUD 组成方式
- ADR-0011: `docs/architecture/adr-0011-character-schema-snapshot-ownership.md` — 角色数据模型 Schema 与快照所有权
- ADR-0012: `docs/architecture/adr-0012-action-service-boundary.md` — 技能与行动服务边界
- ADR-0013: `docs/architecture/adr-0013-deterministic-combat-test-strategy.md` — 确定性战斗逻辑测试策略

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (C# review — primary covers it)
- **Shader Specialist**: unity-shader-specialist (Shader Graph, HLSL, URP materials)
- **UI Specialist**: unity-ui-specialist (UI Toolkit UXML/USS, UGUI Canvas, runtime UI)
- **Additional Specialists**: unity-addressables-specialist (asset loading, memory management, content catalogs)
- **Routing Notes**: Invoke primary for architecture and general C# code review. Invoke shader specialist for rendering and visual effects. Invoke UI specialist for all interface implementation. Invoke Addressables specialist for asset management systems.

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (.cs files) | unity-specialist |
| Shader / material files (.shader, .shadergraph, .mat) | unity-shader-specialist |
| UI / screen files (.uxml, .uss, Canvas prefabs) | unity-ui-specialist |
| Scene / prefab / level files (.unity, .prefab) | unity-specialist |
| Native extension / plugin files (.dll, native plugins) | unity-specialist |
| General architecture review | unity-specialist |
