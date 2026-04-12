---
name: bsk-gameplay
description: BSKGame 玩法层 domain。覆盖战棋战斗系统、实体（Entity）架构、棋盘/格子图数据结构、回合制战斗动作（BattleAction）、AI 规划（ReGoap）、数据表（DR*）、GameplayTag、导航、伤害系统等。代码分布在两层：game.gameplay 纯 C# 类库（域逻辑/模型）和 client/Assets/Game/Scripts/Gameplay/（Unity 侧表现/控制）。当涉及战斗、实体、回合、棋盘、地图、技能、角色、AI、数据表配置等关键词时触发。
---

# BSKGame 玩法层

## 架构分层

玩法代码分布在两个层次，保持"逻辑与表现分离"：

### 1. game.gameplay（纯 C# 类库，无 Unity 依赖）
路径: `gameplay/gameplay/`
程序集: `game.gameplay.dll` → 热更程序集

职责：领域模型、数据结构、规则计算
- `Entity/` — GameplayEntity 基类及各子类（Board/Lattice/Role/TileNode/Player）
- `Graph/` — BoardGraph 棋盘图、LatticeNode 格子节点
- `DataTable/` — DR* 数据行类型（DRBattle/DRRole/DRSkill/DREntity/DRProperty/DRVFX）
- `AI/Navigation/` — 图导航 Agent
- `Damage/` — IDamageable / ApplyDamageInfo
- `Team/` — 队伍接口与扩展
- `Utility/Global/` — 黑板/输入/物理/随机/队伍/VFX 的全局工具门面

### 2. client/Assets/Game/Scripts/Gameplay/（Unity 侧）
路径: `client/Assets/Game/Scripts/Gameplay/`
程序集: `Assembly-CSharp`（热更主程序集的一部分）

职责：Unity 侧表现控制
- `Entity/EntityLogic/` — Unity MonoBehaviour 实体逻辑（Base/Role/RoleController/ProceduralGraph/TileMapGraph）
- `Entity/EntityLogicSocket/` — Mesh/Collider 等实体插槽扩展
- `BattleAction/` — 战斗动作系统（移动/技能/撤销/全部执行等）
- `Camera/` — 战斗相机
- `Tilemap/` — TileGraph 瓦片图 Unity 侧封装
- `Procedure/` — 玩法内子流程（区别于 Launch 的启动 Procedure）
- `System/` — 系统基类
- `Utility/` — 图算法/输入/物理/VFX 的 Unity 侧工具

## 模块详细文档

按需阅读以下 `references/` 文件获取模块级细节：

| 需要了解的内容 | 文件 |
|---|---|
| Entity 分层架构（GameplayEntity → EntityLogic → IView） | `references/entity-system.md` |
| BattleAction 回合制动作链 | `references/battle-action.md` |
| 棋盘图 / Tilemap / Lattice 数据结构 | `references/board-graph.md` |
| ReGoap AI + 导航 | `references/ai-navigation.md` |
| 数据表类型及生成工作流 | `references/data-table.md` |

## 关键入口点

- `GameplayEntity` 基类: `gameplay/gameplay/Entity/GameplayEntity/Base/GameplayEntity.cs`
- `BoardGraph`: `gameplay/gameplay/Graph/BoardGraph.cs`
- `BattleAction` 目录: `client/Assets/Game/Scripts/Gameplay/BattleAction/`
- 玩法 Procedure: `client/Assets/Game/Scripts/Gameplay/Procedure/`
- 数据表定义: `gameplay/gameplay/DataTable/`
- 数据表资源: `client/Assets/GameRes/DataTables/`

## 代码状态标注

以下模块仍在使用，但属于早期遗留实现，后续可能重构：

| 模块 | 引用情况 | 状态 |
|---|---|---|
| BattleAction（11 文件） | 被 BattleManager / BattleMainArchitecture / BattleMainForm 等 15+ 处引用 | **活跃但属于遗留代码**，命令模式骨架可保留但具体动作实现可能重写 |
| BoardGraph / LatticeGameplayEntity | 被 EntityLogic / EntityModel / RoleGameplayEntity / GameUtils 引用 | **活跃但属于遗留代码**，棋盘图结构可能整体重构 |
| TileGraph / TileNodeGameplayEntity | 被 ProcedureMain / SkillAction / TileMapGraphUtility 引用 | **活跃但属于遗留代码**，Tilemap 封装可能重构 |

修改这些代码时需特别注意影响范围，建议先在 `references/` 对应文档中查看引用链路。

## 修改指南

- 新增实体类型：在 `gameplay/Entity/GameplayEntity/` 下创建子类，同时在 `client/.../EntityLogic/` 下创建对应的 Unity 逻辑
- 新增数据表：在 `gameplay/DataTable/` 下创建 `DR{Name}.cs`，在 `client/Assets/GameRes/DataTables/` 放入源数据，在 `ProcedurePreload.DataTableNames` 中注册
- 新增战斗动作：在 `BattleAction/` 下创建，继承对应基类
- 修改 gameplay 库后需重新编译并 CopyDLL
