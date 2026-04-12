---
name: bsk-framework
description: BSKGame 框架核心 domain。覆盖 game.core 纯 C# 类库（GameFramework 模块、对象池、图结构、GameplayTag、ReGoap、日志、时间工具等）、Unity 侧 3rdPart/GameFramework 运行时与编辑器、两个自定义 UPM 包（bsk.core 和 bsk.uiframework）的结构与职责。当涉及 GameFramework 模块扩展、引用池/对象池、GameplayTag、图算法、FSM、Procedure 基础设施、资源管理器底层、自定义 Package 等关键词时触发。
---

# BSKGame 框架核心

## 架构三件套

BSKGame 的框架基础设施分布在三个位置，形成"纯 C# 核心 → Unity 运行时封装 → 自定义 Package 桥接"的三层结构：

```
1. framework/game.core/           ← 纯 C# 类库（netstandard2.1）
   程序集: game.core.dll
   命名空间: GameFramework.* / bsk.game.*

2. client/Assets/3rdPart/GameFramework/ ← Unity 侧 UGF 运行时 + 编辑器
   程序集: UnityGameFramework.Runtime / Editor

3. client/Assets/Packages/        ← 自定义 UPM 包
   ├── com.game.framework.bsk.core/        程序集: BSK.Game.Core
   └── com.game.framework.bsk.uiframework/ 程序集: BSK.Game.Framework.UI
```

## 各层职责

详见 `references/game-core.md`、`references/ugf-runtime.md`、`references/packages.md`。

### game.core（框架心脏）

纯 C#，不依赖 Unity。提供：
- **GameFramework 模块系统**：Procedure / FSM / Entity / Resource / DataTable / UI / Event / Network / ObjectPool / Download / Scene / Sound / Config / Localization / WebRequest / Setting / Debugger / FileSystem
- **对象池与集合池**：`Pool/` 下的 ListPool / DictionaryPool 等
- **图数据结构**：`Graph/` 下的 ArrayGraph + 扩展方法
- **GameplayTag**：标签容器与匹配
- **AI (ReGoap)**：GOAP 规划器
- **项目常量**：`Defination/Constant/` 下的 Entity / UIGroup 等常量定义
- **平台抽象**：`Resource/IWXHelper.cs` 微信平台辅助接口

### UGF 运行时（Unity MonoBehaviour 封装）

将 game.core 的各个 Manager 包装成 Unity Component，由 `GameEntry` 统一挂载和访问。同时提供编辑器工具：资源构建、Inspector、分析器等。

### 自定义 UPM 包

- **bsk.core**：内嵌 `game.core.dll` + `Game.Math.dll` + `Newtonsoft.Json.dll`，外加 Unity 侧薄封装（Singleton / MonoSingleton / GameObjectPool / LogUtility / WXHelper / UnityTimeUtility / DebugTool）
- **bsk.uiframework**：UIFrame 双层架构（详见 `bsk-ui` skill）

## 修改指南

- 扩展 GameFramework 模块：在 `game.core/GameFramework/` 对应目录修改，编译后 CopyDLL
- 添加新的 Pool 类型：在 `game.core/Pool/` 下创建
- Unity 侧框架扩展：在 `client/Assets/Game/Scripts/Common/GameFrameworkExtensions/` 下添加
- 编辑器工具扩展：在 `client/Assets/Editor/GameFrameworkExtension/` 下添加
