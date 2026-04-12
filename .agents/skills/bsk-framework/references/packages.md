# 自定义 UPM 包

## com.game.framework.bsk.core

路径: `client/Assets/Packages/com.game.framework.bsk.core/`
程序集: `BSK.Game.Core`

### 职责

作为 `game.core.dll` 和 Unity 之间的桥接层：
- 内嵌预编译 DLL：`game.core.dll` / `Game.Math.dll` / `Newtonsoft.Json.dll`
- 提供 Unity 侧的基础设施封装

### 运行时内容

| 目录 | 内容 |
|---|---|
| Base/ | Singleton<T> / MonoSingleton<T> / CacheMonoBehaviour / MathAdapter |
| ObjectPool/ | GameObjectPool / ManagedAsset / ObjectPoolUtility |
| Log/ | LogInstance / LogUtility |
| DebugTool/ | Unity 调试绘制与服务 |
| Time/ | UnityTimeUtility（实现 game.core 的 ITimeUtility） |
| Helper/ | WXHelper.cs（实现 game.core 的 IWXHelper，微信平台特定逻辑） |
| TestCore.cs | 测试入口 |

### 编辑器

`BSK.Game.Framework.CoreEditor` asmdef，编辑器侧工具。

## com.game.framework.bsk.uiframework

路径: `client/Assets/Packages/com.game.framework.bsk.uiframework/`
程序集: `BSK.Game.Framework.UI`
依赖: DOTween

### 职责

独立的 UI 框架，详见 `bsk-ui` skill。

### 运行时内容

```
UIFramework/
├── Core/           # 层抽象、控制器接口
├── Panel/          # Panel 层实现
├── Window/         # Window 层实现（栈管理）
├── ScreenTransitions/  # 转场效果
├── UIFrame.cs      # 中央入口
└── UISettings.cs   # 配置
Signals/
└── Signals.cs      # 轻量信号系统
```
