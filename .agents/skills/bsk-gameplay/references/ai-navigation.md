# AI 与导航系统

## ReGoap AI（framework/game.core/AI/ReGoap/）

项目使用 **ReGoap**（Reactive Goal-Oriented Action Planning）作为 AI 框架，位于 game.core 中。

### 结构

```
AI/ReGoap/
├── Core/
│   ├── IReGoapAgent.cs        # Agent 接口
│   ├── IReGoapGoal.cs         # 目标接口
│   ├── IReGoapAction.cs       # 动作接口
│   ├── IReGoapAgentHelper.cs  # Agent 辅助接口
│   └── ReGoapState.cs         # 世界状态表示
└── Planner/
    ├── ReGoapPlannerSettings.cs  # 规划器配置
    └── A* 规划实现
```

### 工作原理

GOAP 是一种基于搜索的 AI 决策方案：
1. Agent 维护一个世界状态（ReGoapState）
2. 定义一组目标（Goal），每个目标有优先级和满足条件
3. 定义一组动作（Action），每个动作有前置条件和效果
4. Planner 通过 A* 搜索找到从当前状态到目标状态的动作序列

适配战棋游戏的 AI 决策：选择移动目标、选择攻击对象、使用技能等。

## 导航系统（gameplay/gameplay/AI/Navigation/）

```
AI/Navigation/
├── INavigation.cs             # 导航接口
└── GraphNavigationAgent.cs    # 基于图的导航 Agent
```

### GraphNavigationAgent

在 BoardGraph 上实现的导航，不依赖 Unity NavMesh：
- 基于棋盘图结构计算路径
- 考虑格子的通行性、占位等
- 与 game.core/Graph/ 下的通用图算法配合使用

## 与 ParadoxNotion 的关系

项目还引入了 ParadoxNotion 的 NodeCanvas（行为树/状态机）和 FlowCanvas（流图），位于 `client/Assets/3rdPart/ParadoxNotion/`。这些可视化工具可以和 ReGoap 配合使用，也可以独立驱动 AI 行为。
