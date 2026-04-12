# 现有 UIForm 清单

## 代码侧

### Windows/UIForm/（client/Assets/Game/Scripts/Windows/UIForm/）

| 目录 | 类型 | 描述 |
|---|---|---|
| BattleMain/ | Window | 战斗主界面（战斗 HUD、操作面板） |
| Menu/ | Window | 主菜单界面（大厅入口） |

### Windows/Base/（client/Assets/Game/Scripts/Windows/Base/）

窗口基类与自定义组件：
- 窗口逻辑基类（继承 UIFrame 的 Controller）
- 扩展方法
- 自定义 UI 组件（CusomComponent 目录，注意拼写）

### Windows/Hud/（client/Assets/Game/Scripts/Windows/Hud/）

- TestHud.cs — HUD 测试用

### 启动流程 UI（client/Assets/Game/Launch/）

| 目录 | 描述 | 状态 |
|---|---|---|
| UpdateResourceForm/ | 资源更新表单（Slider + Text），由 ProcedureUpdateResources 使用 | ✅ 正在使用 |
| SimleResourceUI/Scripts/UI/ | UILoadMgr 系统（UILoadUpdate/UILoadTip/LoadStyle/LoadText 等 8 文件） | ⚠️ **死代码** — 仅被 unusedScripts/Yoo/ 引用 |

## 预制体资源

`client/Assets/GameRes/UI/UIForms/`

| 目录 | 预制体 |
|---|---|
| Battle/ | BattleMainForm.prefab — 战斗主界面 |
| Menu/ | 菜单界面预制体 |
| UpdateResource/ | UIUpdateResourceForm.prefab — 更新资源界面 |

## 数据表关联

UIForm 数据表（DRUIForm）在 `ProcedurePreload` 中加载，关联 screenId 到预制体资源路径。
数据文件: `client/Assets/GameRes/DataTables/UIForm.*`

## 当前状态

项目处于开发阶段，UI 界面数量较少：
- 战斗主界面和菜单是核心界面
- 大厅功能尚在早期（Menu 可以理解为当前的大厅入口）
- HUD 仍为测试阶段

后续大厅功能膨胀后，可从 bsk-ui 拆出独立的 bsk-lobby domain skill。
