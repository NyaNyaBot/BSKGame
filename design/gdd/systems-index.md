# Systems Index: 回响之刃 (Echo of Blades)

> **Status**: Draft
> **Created**: 2026-04-12
> **Last Updated**: 2026-04-27
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

《回响之刃》是一款回合制 RPG + 实时弹反操作的微信小游戏。核心循环是"指令选择 → 我方攻击 → 敌方攻击时弹反 → 结算"，围绕 4 根支柱展开：精准即快感、小而精致、角色即动力、层层递进。系统设计需要在极度有限的体量（30-45 分钟通关）内支撑完整的战斗手感、箱庭探索和角色叙事。平台约束（微信 WebGL、触屏、严格内存/包体限制）要求所有系统从设计阶段就考虑性能预算。

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|---|---|---|---|---|---|
| 1 | 角色数据模型 | Core | MVP | Approved | [角色数据模型](角色数据模型.md) | — |
| 2 | 输入系统 | Core | MVP | Approved | [输入系统](输入系统.md) | 角色数据模型 |
| 3 | 场景管理 | Core | MVP | Approved | [场景管理](场景管理.md) | — |
| 4 | 回合管理器 | Gameplay | MVP | Approved | [回合管理器](回合管理器.md) | 输入系统, 角色数据模型 |
| 5 | 伤害与生命系统 | Gameplay | MVP | Approved | [伤害与生命系统](伤害与生命系统.md) | 角色数据模型 |
| 6 | 实时弹反系统 | Gameplay | MVP | Approved | [实时弹反系统](实时弹反系统.md) | 输入系统, 回合管理器, 伤害与生命系统 |
| 7 | 敌人 AI 与攻击模式 | Gameplay | MVP | Approved | [敌人 AI 与攻击模式](敌人 AI 与攻击模式.md) | 回合管理器, 角色数据模型, 伤害与生命系统 |
| 8 | 技能与行动系统 | Gameplay | MVP | Approved | [技能与行动系统](技能与行动系统.md) | 回合管理器, 角色数据模型, 伤害与生命系统 |
| 9 | 战斗反馈系统 | Presentation | MVP | Approved | [战斗反馈系统](战斗反馈系统.md) | 实时弹反系统, 伤害与生命系统 |
| 10 | 战斗 UI | UI | MVP | Approved | [战斗 UI](战斗 UI.md) | 回合管理器, 实时弹反系统, 技能与行动系统 |
| 11 | 箱庭探索 | Gameplay | Vertical Slice | Not Started | — | 输入系统, 场景管理 |
| 12 | 回响值系统 | Gameplay | Vertical Slice | Not Started | — | 实时弹反系统 |
| 13 | 记忆碎片系统 | Progression | Vertical Slice | Not Started | — | 角色数据模型, 箱庭探索 |
| 14 | 对话与叙事 | Narrative | Vertical Slice | Not Started | — | 箱庭探索, 角色数据模型 |
| 15 | 探索 UI | UI | Vertical Slice | Not Started | — | 箱庭探索, 记忆碎片系统 |
| 16 | 音频管理 | Audio | Vertical Slice | Not Started | — | 场景管理, 回合管理器, 实时弹反系统 |
| 17 | 存档系统 | Persistence | Alpha | Not Started | — | 角色数据模型, 记忆碎片系统, 场景管理 |
| 18 | 教程引导 | Meta | Alpha | Not Started | — | 回合管理器, 实时弹反系统, 战斗 UI, 探索 UI |

---

## Categories

| Category | Description |
|---|---|
| **Core** | 基础设施系统，所有其他系统的地基 |
| **Gameplay** | 构成核心循环的玩法系统 |
| **Progression** | 玩家成长与解锁 |
| **Narrative** | 故事与对话交付 |
| **UI** | 玩家界面信息展示 |
| **Presentation** | 视听反馈与表现力 |
| **Audio** | 声音与音乐系统 |
| **Persistence** | 存档与状态持久化 |
| **Meta** | 核心循环之外的辅助系统 |

---

## Priority Tiers

| Tier | Definition | Target Milestone | Systems Count |
|---|---|---|---|
| **MVP** | 验证核心假设"弹反在触屏上爽不爽"所必需的系统 | 1-1.5 周 | 10 |
| **Vertical Slice** | 完整体验一层钟楼：探索→战斗→对话→成长闭环 | 2-3 周 | 6 |
| **Alpha** | 全功能完整，多层通关，持久化 | 3-4 周 | 2 |

---

## Dependency Map

### Foundation 层（零依赖）

1. **角色数据模型** — 所有战斗数值与能力定义的容器，被 7 个系统依赖（最高瓶颈）
2. **输入系统** — 触屏输入采集与弹反时机精度处理，被 4 个系统依赖
3. **场景管理** — 楼层加载/卸载与场景切换基础设施

### Core 层（仅依赖 Foundation）

4. **回合管理器** — 依赖：输入系统, 角色数据模型。回合制阶段流转骨架，被 6 个系统依赖
5. **伤害与生命系统** — 依赖：角色数据模型。弹反成功/失败的数值后果
6. **箱庭探索** — 依赖：输入系统, 场景管理。钟楼内移动与环境交互

### Feature 层（依赖 Core）

7. **实时弹反系统** — 依赖：输入系统, 回合管理器, 伤害与生命系统。核心假设本体
8. **技能与行动系统** — 依赖：回合管理器, 角色数据模型, 伤害与生命系统
9. **敌人 AI 与攻击模式** — 依赖：回合管理器, 角色数据模型, 伤害与生命系统
10. **回响值系统** — 依赖：实时弹反系统。连续完美弹反的累积奖励
11. **记忆碎片系统** — 依赖：角色数据模型, 箱庭探索
12. **对话与叙事** — 依赖：箱庭探索, 角色数据模型
13. **存档系统** — 依赖：角色数据模型, 记忆碎片系统, 场景管理

### Presentation 层（包装 Feature）

14. **战斗 UI** — 依赖：回合管理器, 实时弹反系统, 回响值系统, 技能与行动系统
15. **探索 UI** — 依赖：箱庭探索, 记忆碎片系统
16. **战斗反馈系统** — 依赖：实时弹反系统, 伤害与生命系统, 回响值系统
17. **音频管理** — 依赖：场景管理, 回合管理器, 实时弹反系统

### Polish 层

18. **教程引导** — 依赖：回合管理器, 实时弹反系统, 战斗 UI, 探索 UI

---

## Recommended Design Order

| Order | System | Priority | Layer | Est. Effort |
|---|---|---|---|---|
| 1 | 角色数据模型 | MVP | Foundation | S |
| 2 | 输入系统 | MVP | Foundation | S |
| 3 | 场景管理 | MVP | Foundation | S |
| 4 | 回合管理器 | MVP | Core | M |
| 5 | 伤害与生命系统 | MVP | Core | S |
| 6 | 实时弹反系统 | MVP | Feature | L |
| 7 | 敌人 AI 与攻击模式 | MVP | Feature | M |
| 8 | 技能与行动系统 | MVP | Feature | M |
| 9 | 战斗反馈系统 | MVP | Presentation | M |
| 10 | 战斗 UI | MVP | Presentation | M |
| 11 | 箱庭探索 | VS | Core | M |
| 12 | 回响值系统 | VS | Feature | S |
| 13 | 记忆碎片系统 | VS | Feature | S |
| 14 | 对话与叙事 | VS | Feature | M |
| 15 | 探索 UI | VS | Presentation | S |
| 16 | 音频管理 | VS | Presentation | M |
| 17 | 存档系统 | Alpha | Feature | S |
| 18 | 教程引导 | Alpha | Polish | S |

Effort: S = 1 session, M = 2-3 sessions, L = 4+ sessions

---

## Circular Dependencies

无循环依赖。所有依赖方向一致：Foundation → Core → Feature → Presentation → Polish。

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|---|---|---|---|
| 实时弹反系统 | Technical + Design | 微信 WebGL 触屏输入延迟可能导致弹反手感不精准；timing window 参数需要大量 playtest | MVP 最优先原型验证；准备降级方案（更宽窗口、视觉辅助） |
| 输入系统 | Technical | 微信小游戏 WebGL 层与浏览器之间的输入延迟不可控；不同机型触控响应差异大 | 早期在真机上测量延迟基线；设计延迟补偿机制 |
| 角色数据模型 | Design | 被 7 个系统依赖——数据结构设计错误会波及全局 | 先定义最小接口契约；MVP 阶段用硬编码数据，确认后再数据驱动 |
| 战斗反馈系统 | Technical | WebGL 上的顿帧/震屏/全屏VFX可能有性能问题；Art Bible 的"完美弹反爆灯"需要在 DC≤70 内实现 | 按 Art Bible §8 的 Draw Call 预算做技术验证 |

---

## Progress Tracker

| Metric | Count |
|---|---|
| Total systems identified | 18 |
| Design docs started | 10 |
| Design docs reviewed | 11 |
| Design docs approved | 10 |
| MVP systems designed | 10/10 |
| Vertical Slice systems designed | 0/6 |

---

## Next Steps

- [x] MVP 10 个核心系统 GDD 完成并 review
- [ ] 运行 `/gate-check pre-production` 验证是否可进入实现前阶段
- [ ] 最高风险系统（实时弹反 + 输入 + 反馈）尽早原型验证 (`/prototype 弹反战斗`)
- [ ] 继续补 Vertical Slice 系统：箱庭探索、回响值系统、记忆碎片、叙事、探索 UI、音频管理
