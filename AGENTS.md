# BSKGame — AI Agent 入口

BSKGame 是一款基于 Unity 2022.3 的战棋类微信小游戏，采用 GameFramework (Starforce) + HybridCLR 热更 + 微信 WASM 小游戏导出方案。

## 快速导航

开始任何涉及本项目的任务前，先阅读 **项目总导航 skill**：

```
.agents/skills/bsk-project-map/SKILL.md
```

它包含仓库结构、程序集依赖关系、以及按任务类型跳转到对应 domain skill 的索引表。

## 可用 Skills

所有 skill 遵循 [Agent Skills 标准](https://agentskills.io/specification)，位于 `.agents/skills/` 目录。

| Skill | 路径 | 职责 |
|---|---|---|
| bsk-project-map | `.agents/skills/bsk-project-map/` | 项目总导航，仓库结构与 skill 索引 |
| bsk-framework | `.agents/skills/bsk-framework/` | 框架核心：game.core / UGF 运行时 / 自定义 UPM 包 |
| bsk-gameplay | `.agents/skills/bsk-gameplay/` | 玩法层：战斗 / 实体 / 棋盘 / AI / 数据表 |
| bsk-launch | `.agents/skills/bsk-launch/` | 启动流程：Procedure 链 / 热更 / 资源更新 |
| bsk-ui | `.agents/skills/bsk-ui/` | UI 层：UIFrame / Window-Panel / HUD |
| bsk-wx-minigame | `.agents/skills/bsk-wx-minigame/` | 微信小游戏：导出 / JS 桥接 / 分包 / CDN |
| skill-creator | `.agents/skills/skill-creator/` | 工具：创建和迭代 skill |

每个 skill 文件夹包含 `SKILL.md`（主指令）和可选的 `references/`（按需加载的参考文档）。

## 编码约定

- **命名空间**：`Game.Client`（客户端）、`Game.Gameplay`（玩法库）、`Game.Core`（core 包）、`Game.UIFramework`（UI 框架包）
- **运行时代码**：`client/Assets/Game/`
- **编辑器扩展**：`client/Assets/Editor/`
- **外部 C# 工程**（framework / gameplay）修改后需重新编译并通过 `CopyDLL.bat` 同步 DLL

## 禁止修改区域

- `client/Assets/GameRes/DLL/*.bytes` — HybridCLR 编译产物，由 CopyDLL.bat 生成
- `client/Assets/3rdPart/` — 第三方库源码，除非用户明确要求
- `wxProject/minigame/webgl.wasm.framework.unityweb.js` — Unity 导出产物（2w+ 行）

## Cursor 特有配置

- **全局规则**：`.cursor/rules/bsk-global.md`（语言、禁止修改区域、编码风格、提交约定）
