# Epics Index

Last Updated: 2026-05-01
Engine: Unity 2022.3.17f1
Control Manifest: 2026-05-01

## Foundation Layer

| Epic | Layer | System / Scope | Primary GDD | Stories | Status |
|------|-------|----------------|-------------|---------|--------|
| [scene-lifecycle-context](scene-lifecycle-context/EPIC.md) | Foundation | 场景管理 | `design/gdd/场景管理.md` | 3 | Ready |
| [battle-event-bus-dto-versioning](battle-event-bus-dto-versioning/EPIC.md) | Foundation | 战斗事件总线（跨系统契约） | `design/gdd/回合管理器.md` | 3 | Ready |
| [gameplay-unity-hybridclr-layering](gameplay-unity-hybridclr-layering/EPIC.md) | Foundation | 程序集分层 / HybridCLR 边界 | `design/gdd/game-concept.md` | 3 | Ready |
| [battle-input-touch-and-hit-areas](battle-input-touch-and-hit-areas/EPIC.md) | Foundation | 输入系统 | `design/gdd/输入系统.md` | 3 | Ready |

## Core Layer

| Epic | Layer | System / Scope | Primary GDD | Stories | Status |
|------|-------|----------------|-------------|---------|--------|
| [character-schema-snapshot](character-schema-snapshot/EPIC.md) | Core | 角色数据模型 | `design/gdd/角色数据模型.md` | 3 | Ready |
| [turn-manager-combat-clock](turn-manager-combat-clock/EPIC.md) | Core | 回合管理器 + 战斗时钟 | `design/gdd/回合管理器.md` | 4 | Ready |
| [damage-hp-ownership](damage-hp-ownership/EPIC.md) | Core | 伤害与生命系统 | `design/gdd/伤害与生命系统.md` | 3 | Ready |

## Feature Layer

| Epic | Layer | System / Scope | Primary GDD | Stories | Status |
|------|-------|----------------|-------------|---------|--------|
| [parry-system](parry-system/EPIC.md) | Feature | 实时弹反系统 | `design/gdd/实时弹反系统.md` | 4 | Ready |
| [enemy-ai-attack-patterns](enemy-ai-attack-patterns/EPIC.md) | Feature | 敌人 AI 与攻击模式 | `design/gdd/敌人 AI 与攻击模式.md` | 3 | Ready |
| [action-system](action-system/EPIC.md) | Feature | 技能与行动系统 | `design/gdd/技能与行动系统.md` | 3 | Ready |

## Presentation Layer

| Epic | Layer | System / Scope | Primary GDD | Stories | Status |
|------|-------|----------------|-------------|---------|--------|
| [battle-feedback](battle-feedback/EPIC.md) | Presentation | 战斗反馈系统 | `design/gdd/战斗反馈系统.md` | 3 | Ready |
| [battle-ui](battle-ui/EPIC.md) | Presentation | 战斗 UI | `design/gdd/战斗 UI.md` | 3 | Ready |

## Summary

| Layer | Epics | Stories |
|-------|-------|---------|
| Foundation | 4 | 12 |
| Core | 3 | 10 |
| Feature | 3 | 10 |
| Presentation | 2 | 6 |
| **Total** | **12** | **38** |

**Next:** `/ccgs-story-readiness` per story, then `/ccgs-dev-story` when implementing.
