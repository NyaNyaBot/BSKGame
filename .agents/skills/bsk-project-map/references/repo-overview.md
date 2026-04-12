# BSKGame 仓库全景

## 项目定位

BSKGame 是一款**战棋类微信小游戏**，基于 Unity 2022.3.17f1 开发，使用微信小游戏 WASM 方案发布。

## 技术栈

| 技术 | 版本/方案 | 用途 |
|---|---|---|
| Unity | 2022.3.17f1 | 游戏引擎 |
| GameFramework (UGF/Starforce) | 源码内嵌 | 应用框架 |
| HybridCLR | com.code-philosophy.hybridclr | C# 热更新 |
| WX-WASM-SDK-V2 | 编辑器插件 | 微信小游戏导出 |
| UnityPlugin | 1.2.91 | 微信侧 Unity 运行时插件 |
| ParadoxNotion NodeCanvas/FlowCanvas | 源码内嵌 | 行为树/流图可视化 AI |
| ReGoap | 内嵌在 game.core | GOAP AI 规划 |
| DOTween | Package 依赖 | UI 动效 |
| Newtonsoft.Json | DLL 引用 | JSON 序列化 |
| Game.Math | DLL 引用 | 数学库 |

## 程序集全景

### AOT 侧（不可热更）
- `BSK.Game.Launch` — 启动流程 Procedure
- `BSK.Game.Core` — game.core Unity 侧封装
- `BSK.Game.Framework.UI` — UI 框架
- `BSK.Game.Common` — 通用基础
- `GameFramework` / `UnityGameFramework.Runtime` — 框架运行时

### 热更侧
- `Assembly-CSharp` — 主客户端逻辑（含 Gameplay/Windows/Hud）
- `game.gameplay` — 纯 C# 玩法逻辑库

### 纯 C# 外部工程
- `game.core` → `framework/game.core/game.core.csproj`
- `game.gameplay` → `gameplay/gameplay/gameplay.csproj`

## 目录 → 程序集映射

| 目录 | asmdef / 程序集 | 热更 |
|---|---|---|
| client/Assets/Game/Launch/ | BSK.Game.Launch | 否（AOT 补丁） |
| client/Assets/Game/Scripts/Common/ | BSK.Game.Common | 否（AOT 补丁） |
| client/Assets/Game/Scripts/Gameplay/ | Assembly-CSharp 的一部分 | 是 |
| client/Assets/Game/Scripts/Windows/ | BSK.Game.Windows / BSK.Game.Hud | 是 |
| client/Assets/Packages/bsk.core/ | BSK.Game.Core | 否（AOT 补丁） |
| client/Assets/Packages/bsk.uiframework/ | BSK.Game.Framework.UI | 否（AOT 补丁） |
| framework/game.core/ | game.core | 否（AOT 补丁） |
| gameplay/gameplay/ | game.gameplay | 是 |
| client/Assets/3rdPart/GameFramework/ | GameFramework + UnityGameFramework.Runtime | 否 |

## 资源类型一览

`client/Assets/GameRes/` 下按类型组织：

| 子目录 | 内容 |
|---|---|
| DLL/ | 热更 + AOT 补丁 DLL（.bytes） |
| DataTables/ | 策划数据表（txt/bytes/xlsx） |
| UI/UIForms/ | UI 预制体 |
| Scenes/ | 场景（Launch.unity / BattleStorm/ / Menu） |
| Configs/ | 构建配置 / ResourceBuilder / GameplayTag / VersionInfo |
| Settings/ | 游戏/框架设置 ScriptableObject |
| Prefab/ | 角色/投影等预制体 |
| Tile/ | 瓦片与调色板 |
| Entities/ | 实体资源 |
| Sprite/ Material/ Textures/ Shader/ VFX/ | 美术资源 |

## 构建输出

| 输出 | 路径 | 说明 |
|---|---|---|
| 微信小游戏 | wxProject/minigame/ | 微信开发者工具打开此目录 |
| 标准 WebGL | wxProject/webgl/ 或 build/webglbuild/ | 浏览器直接运行 |

## 已弃用/归档/死代码

| 路径 | 状态 | 说明 |
|---|---|---|
| `cachePackages/UnusedGFExtension/` | 归档 | 未使用的 GF 扩展集，不参与构建 |
| `unusedScripts/Yoo/` | 废弃 | YooAsset 资源管线流程，已完全弃用 |
| `client/.../Launch/SimleResourceUI/` | 死代码 | UILoadMgr 加载 UI，仅被废弃的 Yoo 流程引用 |
| `client/.../Procedure/GF/ProcedureChangeScene.cs` | 注释 | 全文注释，场景切换已由 ProcedureGame 处理 |

## 遗留代码（仍在使用但可能重构）

以下代码仍是 Gameplay 核心引用链的一部分，但属于早期实现：
- `BattleAction/` — 回合制动作系统（11 文件，15+ 处引用）
- `BoardGraph` / `LatticeGameplayEntity` — 棋盘图结构
- `TileGraph` / `TileNodeGameplayEntity` — Tilemap 封装

修改或重构这些模块时需特别注意广泛的引用依赖。
