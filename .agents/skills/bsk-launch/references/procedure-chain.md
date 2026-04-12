# Procedure 链路详解

所有 Procedure 位于 `client/Assets/Game/Launch/Procedure/GF/`，命名空间 `Game.Client`。

## 完整流程图

```
ProcedureLaunch
  │  OnEnter: 初始化语言/变体/声音设置
  │  OnUpdate: WX.InitSDK → 回调
  ▼
ProcedureSplash
  │  OnUpdate: 按资源模式分支
  ├─ EditorResourceMode ──────────→ ProcedureLoadAssembly
  ├─ ResourceMode.Package ────────→ ProcedureInitResources
  └─ Updatable (默认) ───────────→ ProcedureCheckVersion
                                      │
                                      │ OnEnter: 请求远端版本信息 (CheckVersionUrl)
                                      │ OnWebRequestSuccess: 解析 VersionInfo JSON
                                      │ 对比 InternalResourceVersion
                                      ▼
                                ┌─ 需要更新 ─→ ProcedureUpdateVersion
                                │                │ 下载版本列表
                                │                ▼
                                │           ProcedureVerifyResources
                                │                │ 校验资源完整性
                                │                ▼
                                │           ProcedureUpdateResources
                                │                │ 下载更新资源包
                                │                ▼
                                └─ 不需更新 ─→ ProcedureVerifyResources
                                                 │
                                                 ▼
                                          ProcedureInitResources
                                            │ OnEnter: GameEntry.Resource.InitResources()
                                            │ 完成回调
                                            ▼
                                          ProcedureLoadAssembly
                                            │ 1. LoadMetadataForAOTAssembly (非 Editor)
                                            │ 2. 加载 HotUpdateAssemblies DLL
                                            │ 两者都完成后
                                            ▼
                                          ProcedurePreload
                                            │ 加载数据表: Entity/Scene/UIForm/Role/
                                            │   Property/VFX/Skill/Battle
                                            │ 全部加载完毕
                                            ▼
                                          ProcedureGame
                                            │ 反射创建 GameStarter 组件
                                            │ 进入热更侧主逻辑
                                            ▼
                                          (Procedure 终态，不再切换)
```

## 各 Procedure 职责速查

| Procedure | 文件 | 核心职责 |
|---|---|---|
| ProcedureLaunch | ProcedureLaunch.cs | 微信 SDK 初始化，语言/声音/变体设置 |
| ProcedureSplash | ProcedureSplash.cs | Splash 展示 + 根据资源模式做三路分支 |
| ProcedureCheckVersion | ProcedureCheckVersion.cs | WebRequest 请求远端 VersionInfo，决定是否更新 |
| ProcedureUpdateVersion | ProcedureUpdateVersion.cs | 下载新的版本列表 |
| ProcedureVerifyResources | ProcedureVerifyRersources.cs | 校验本地资源完整性（注意文件名拼写） |
| ProcedureUpdateResources | ProcedureUpdateResources.cs | 下载资源增量包 |
| ProcedureInitResources | ProcedureInitResources.cs | 初始化资源管理器（单机模式直接进入此处） |
| ProcedureLoadAssembly | ProcedureLoadAssembly.cs | HybridCLR 核心：AOT 元数据 + 热更 DLL 加载 |
| ProcedurePreload | ProcedurePreload.cs | 预加载数据表（8 张） |
| ProcedureChangeScene | ProcedureChangeScene.cs | 当前被整体注释，未启用 |
| ProcedureGame | ProcedureGame.cs | 流程终态：反射创建 GameStarter 进入热更主逻辑 |

## 注意事项

- `ProcedureCheckVersion` 中的版本检查 URL 由 `BuiltinDataComponent.BuildInfo.CheckVersionUrl` 提供
- `ProcedureChangeScene` 当前全文注释，场景切换逻辑已移至 `ProcedureGame`/`ProcedurePreload` 直接跳转
- `ProcedureVerifyRersources.cs` 文件名有拼写问题（Rersources），重命名时需同步更新 meta 文件
- `ProcedureCheckResources` 中有 `#if WEIXINMINIGAME` 分支：微信平台跳过资源下载直接进 LoadAssembly
- `ProcedureUpdateVersion` 完成后跳转到 `ProcedureVerifyResources`（跳过了 `ProcedureCheckResources`），但 Verify 完成后又跳回 `ProcedureCheckResources`

## 死代码/废弃部分

| 文件/目录 | 状态 | 说明 |
|---|---|---|
| `ProcedureChangeScene.cs` | 全文注释 | 场景切换已由 ProcedureGame 直接处理 |
| `SimleResourceUI/`（8 个 .cs） | 死代码 | UILoadMgr 仅被 `unusedScripts/Yoo/` 引用，当前 GF 流程不使用 |
| `unusedScripts/Yoo/` | 废弃 | 整套 YooAsset 资源管线流程，已弃用 |
