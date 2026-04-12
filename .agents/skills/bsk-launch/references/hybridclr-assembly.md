# HybridCLR 热更机制

## 概述

项目使用 HybridCLR (com.code-philosophy.hybridclr) 实现 C# 热更新。核心思路：将部分程序集标记为热更新程序集，构建时编译为 DLL，运行时以 TextAsset (.bytes) 形式从资源系统加载，通过 `Assembly.Load` 加载到运行时。

## 配置

**配置文件**: `client/ProjectSettings/HybridCLRSettings.asset`

### 热更新程序集

```yaml
hotUpdateAssemblies:
  - game.gameplay        # 玩法逻辑库
  - Assembly-CSharp      # 主客户端逻辑
```

对应 asmdef：
- `game.gameplay` → `gameplay/gameplay/gameplay.csproj` 编译产物
- `Assembly-CSharp` → `client/Assets/Game/Scripts/` 下各 asmdef 的合集

### AOT 元数据补充列表

```yaml
patchAOTAssemblies:
  - Newtonsoft.Json
  - System.Core / System / mscorlib
  - Unity.RenderPipelines.Core.Runtime
  - UnityEngine.AnimationModule / UnityEngine.CoreModule
  - game.core
  - ReGoap
  - BSK.Game.Core / BSK.Game.Common / BSK.Game.Launch
  - GameFramework / UnityGameFramework.Runtime
  - BSK.Game.Framework.UI
```

这些 DLL 的裁剪后版本在构建时自动复制到 `HybridCLRData/AssembliesPostIl2CppStrip/`，然后通过 `CopyDLL.bat` 或构建流程放入 `Assets/GameRes/DLL/`。

## DLL 加载流程（ProcedureLoadAssembly）

```
1. 判断 HybridCLR 是否启用 (SettingsUtils.HybridCLRCustomGlobalSettings.Enable)
   ├─ 未启用 → 直接从 AppDomain 找 MainLogicAssembly
   └─ 启用
      ├─ Editor 模式 → 跳过 AOT 元数据加载，直接用已加载的程序集
      └─ 非 Editor
         ├─ Step 1: 加载 AOT 元数据 (LoadMetadataForAOTAssembly)
         │   遍历 AOTMetaAssemblies 列表
         │   路径: Assets/GameRes/DLL/{dllName}.bytes
         │   优先级: 10（高于热更 DLL，确保先加载完）
         │   调用 HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, SuperSet)
         │
         └─ Step 2: 加载热更 DLL
             遍历 HotUpdateAssemblies 列表
             路径: Assets/GameRes/DLL/{dllName}.bytes
             优先级: 0
             调用 Assembly.Load(bytes)
             识别 LogicMainDllName 作为主逻辑程序集

2. 两个加载全部完成 → ChangeState<ProcedurePreload>
```

## DLL 文件位置

运行时加载路径: `Assets/GameRes/DLL/{assemblyName}.bytes`

当前 DLL 列表（含热更 + AOT 补丁）：
- Assembly-CSharp.dll.bytes
- BSK.Game.Client.dll.bytes / BSK.Game.Common.dll.bytes / BSK.Game.Core.dll.bytes
- BSK.Game.Framework.UI.dll.bytes / BSK.Game.Gameplay.dll.bytes
- BSK.Game.Hud.dll.bytes / BSK.Game.Launch.dll.bytes
- BSK.Game.UIBase.dll.bytes / BSK.Game.Windows.dll.bytes
- GameFramework.dll.bytes / UnityGameFramework.Runtime.dll.bytes
- game.core.dll.bytes / game.gameplay.dll.bytes
- Newtonsoft.Json.dll.bytes / System.Core.dll.bytes / System.dll.bytes / mscorlib.dll.bytes
- Unity.RenderPipelines.Core.Runtime.dll.bytes
- UnityEngine.AnimationModule.dll.bytes / UnityEngine.CoreModule.dll.bytes

## 外部工程编译

`framework/game.core/` 和 `gameplay/gameplay/` 是独立 .NET 工程：
- 目标框架: `netstandard2.1`
- 构建配置: Debug / Release / ReleaseNPublish
- 条件编译宏: `WEIXINMINIGAME` 始终定义
- PostBuild: 执行 `CopyDLL.bat` 将产物拷贝到 Unity 工程

修改这两个工程后需要重新编译并运行 CopyDLL.bat 以更新 `Assets/GameRes/DLL/` 下的 bytes 文件。
