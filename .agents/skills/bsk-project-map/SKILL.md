---
name: bsk-project-map
description: BSKGame 项目总导航。任何涉及本项目的任务——无论是 Unity 客户端、C# 框架、gameplay 逻辑、微信小游戏导出、UI 开发、资源管线、HybridCLR 热更、构建发布——都应先读取本 skill 了解仓库结构，再按需跳转到对应的 domain skill。凡提到 BSKGame、BSK、战棋、小游戏、HybridCLR、GameFramework、UGF 等关键词时，优先触发本 skill。
---

# BSKGame 项目导航

BSKGame 是一款基于 Unity 2022.3 的**战棋类微信小游戏**，采用 GameFramework (Starforce) + HybridCLR 热更 + 微信 WASM 小游戏导出方案。

## 仓库顶层结构

```
BSKGame/
├── client/              # Unity 工程根目录
│   ├── Assets/
│   │   ├── Game/        # 核心业务代码（Launch + Scripts）
│   │   ├── GameRes/     # 运行时资源（UI/场景/数据表/DLL/特效等）
│   │   ├── Packages/    # 自定义 UPM 包（bsk.core / bsk.uiframework）
│   │   ├── 3rdPart/     # 第三方源码（GameFramework/ParadoxNotion/QFramework）
│   │   ├── Editor/      # 编辑器工具（数据表生成/构建扩展/HybridCLR）
│   │   └── WX-WASM-SDK-V2/  # 微信 WASM SDK 编辑器配置
│   └── ProjectSettings/ # Unity 项目设置（含 HybridCLRSettings）
├── framework/           # game.core C# 类库（netstandard2.1，与 Unity 解耦）
├── gameplay/            # game.gameplay C# 类库（依赖 game.core）
├── wxProject/           # 微信小游戏 + WebGL 导出目录
├── build/               # 标准 WebGL 构建输出
├── Library/             # 外部 DLL 引用（Game.Math / Newtonsoft.Json）
└── cachePackages/       # 已弃用的 GF 扩展归档
```

## 程序集依赖关系

```
[Unity 原生]
     ↓
BSK.Game.Launch (client/Assets/Game/Launch/)  ← 启动流程，AOT 侧
     ↓ 反射加载热更 DLL
BSK.Game.Client (client/Assets/Game/Scripts/) ← Assembly-CSharp 热更主程序集
     ↓ 引用
BSK.Game.Core   (Packages/com.game.framework.bsk.core/)  ← Unity 侧薄封装
     ↓ 内嵌
game.core.dll   (framework/game.core/)        ← 纯 C# 框架核心
     ↑ ProjectReference
game.gameplay.dll (gameplay/gameplay/)         ← 纯 C# 玩法逻辑

BSK.Game.Framework.UI (Packages/com.game.framework.bsk.uiframework/) ← UI 框架
BSK.Game.Windows / BSK.Game.Hud (client/Assets/Game/Scripts/Windows/) ← UI 表现
```

## 按任务选择 Domain Skill

| 你要做什么 | 读哪个 skill |
|---|---|
| 启动流程 / Procedure 链 / 版本检查 / 资源热更 / HybridCLR 加载 | `bsk-launch` |
| 战斗 / 实体 / 棋盘 / 回合动作 / AI / 数据表 / 玩法逻辑 | `bsk-gameplay` |
| UI 架构 / Window-Panel 层级 / UIForm / HUD | `bsk-ui` |
| game.core 框架 / GameFramework 模块 / 自定义 UPM 包 / 对象池 / 图结构 | `bsk-framework` |
| 微信小游戏导出 / JS 桥接 / 分包 / CDN / MiniGameConfig | `bsk-wx-minigame` |

读取方式：阅读 `.agents/skills/<skill-name>/SKILL.md`，如需更深入的模块细节，按 SKILL.md 中的指引读取对应的 `references/*.md`。

## 关键配置文件速查

| 配置 | 路径 |
|---|---|
| HybridCLR 设置 | `client/ProjectSettings/HybridCLRSettings.asset` |
| 微信导出配置 | `client/Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset` |
| 小游戏工程配置 | `wxProject/minigame/game.json` + `project.config.json` |
| 资源构建 | `client/Assets/GameRes/Configs/ResourceBuilder.xml` |
| game.core 工程 | `framework/game.core/game.core.csproj` |
| game.gameplay 工程 | `gameplay/gameplay/gameplay.csproj` |
