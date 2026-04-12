---
name: bsk-launch
description: BSKGame 启动流程 domain。涉及 Unity 启动 Procedure 链（从 ProcedureLaunch 到 ProcedureGame）、版本检查与资源热更新、HybridCLR 热更 DLL 加载（AOT 元数据补充 + 程序集反射加载）、预加载数据表、微信 SDK 初始化等。当任务涉及启动顺序、Procedure 流程修改、资源更新 UI、热更程序集配置、BuiltinData、加载进度等关键词时触发本 skill。
---

# BSKGame 启动流程

## 概览

启动管线基于 **GameFramework Procedure（有限状态机）** 实现，位于 `client/Assets/Game/Launch/` 目录，由 `BSK.Game.Launch` 程序集定义。该程序集运行在 AOT 侧，负责完成所有准备工作后，通过反射创建热更侧的 `GameStarter` 组件进入主游戏逻辑。

## Procedure 链路

完整流程和分支条件见 `references/procedure-chain.md`。

简要路径（最常见的可更新模式）：

```
ProcedureLaunch → ProcedureSplash → ProcedureCheckVersion → ProcedureUpdateVersion
  → ProcedureVerifyResources → ProcedureUpdateResources → ProcedureInitResources
  → ProcedureLoadAssembly → ProcedurePreload → ProcedureGame
```

## 关键机制

### HybridCLR 热更

`ProcedureLoadAssembly` 是热更核心节点：
1. 先加载 AOT 元数据 DLL（`patchAOTAssemblies` 列表，优先级 10）
2. 再加载热更 DLL（`hotUpdateAssemblies` 列表，优先级 0）
3. 两者都从 `Assets/GameRes/DLL/` 加载 `.bytes` 格式的 TextAsset
4. 全部完成后进入 `ProcedurePreload`

热更程序集列表与 AOT 补丁列表配置在 `client/ProjectSettings/HybridCLRSettings.asset`。
详细机制见 `references/hybridclr-assembly.md`。

### 预加载阶段

`ProcedurePreload` 加载以下数据表：Entity, Scene, UIForm, Role, Property, VFX, Skill, Battle。
加载完成后设定目标场景为 `GameRes/Scenes/Menu`，进入 `ProcedureGame`。

### 进入游戏

`ProcedureGame` 通过反射从热更程序集中找到 `Game.Client.GameStarter` 类型，创建 GameObject 并挂载该组件，正式进入热更侧主逻辑。此后 Procedure 系统不再切换状态。

## 目录结构

```
client/Assets/Game/Launch/
├── BSK.Game.Launch.asmdef
├── Procedure/GF/           # 12 个 Procedure 文件
├── Component/BuiltinData/  # BuiltinDataComponent（构建信息/版本信息）
├── BuildExtension/Runtime/ # BuildInfo / VersionInfo 数据结构
├── Helper/                 # BuildInfoVersionHelper
├── SimleResourceUI/        # ⚠️ 死代码 — Yoo 管线遗留，当前 GF 流程未引用
└── UpdateResourceForm/     # ✅ 正在使用的资源更新 UI（Slider + Text）
```

> **注意**: `SimleResourceUI/`（UILoadMgr 等 8 个文件）仅被 `unusedScripts/Yoo/` 下的废弃流程引用，当前 GF Procedure 链不使用。`UpdateResourceForm/`（UIUpdateResourceForm）才是 `ProcedureUpdateResources` 实际使用的加载 UI。
```

## 修改指南

- 添加新 Procedure：在 `Procedure/GF/` 下创建，继承 `ProcedureBase`，在前后 Procedure 中用 `ChangeState<T>` 串接
- 修改热更程序集列表：编辑 `HybridCLRSettings.asset` 的 `hotUpdateAssemblies` 和 `patchAOTAssemblies`
- 修改预加载数据表：编辑 `ProcedurePreload.DataTableNames` 数组
- 修改目标场景：编辑 `ProcedurePreload.OnUpdate` 中的 `menuSceneAssetName`
