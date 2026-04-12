# BattleAction 战斗动作系统

## 概述

位于 `client/Assets/Game/Scripts/Gameplay/BattleAction/`，实现回合制战斗的动作链机制。

## 目录结构

```
BattleAction/
├── BattleAction.cs          # 动作基类/接口
├── EmptyAction.cs           # 空动作（占位/跳过）
├── Model/                   # 动作相关数据模型
└── 其他具体动作实现...
```

## 设计模式

基于命令模式（Command Pattern），每个战斗动作封装为独立对象：
- 支持执行（Execute）
- 支持撤销（Undo）
- 支持批量执行（Execute All）

这种设计天然适配回合制战棋游戏的需求：玩家可以规划多步操作后一次性执行，也可以撤回之前的决策。

## 与实体系统的关系

BattleAction 操作 GameplayEntity：
- 移动动作 → 修改 PositionComponent
- 技能动作 → 调用 GameplayEntity.ReceiveDamage / Kill
- 动作的目标通过 EntityId 索引

## 扩展方式

新增战斗动作：
1. 在 `BattleAction/` 下创建新类
2. 继承动作基类
3. 实现 Execute / Undo 逻辑
4. 在战斗流程中注册和调度
