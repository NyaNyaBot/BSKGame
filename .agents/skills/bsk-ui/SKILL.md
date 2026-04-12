---
name: bsk-ui
description: BSKGame UI 层 domain。覆盖 UIFrame 架构（Panel / Window 双层）、屏幕控制器生命周期、转场动画、UIForm 预制体管理、HUD 系统，以及当前已实现的各个界面（战斗主界面/菜单/资源更新）。当涉及 UI 开发、界面打开关闭、Window/Panel 层级、UIForm 注册、HUD、UI 动画转场等关键词时触发。
---

# BSKGame UI 层

## 架构概览

UI 系统基于自定义 `UIFrame` 框架（`com.game.framework.bsk.uiframework` 包），采用 **Panel + Window 双层**设计：

- **Window 层**：全屏或接近全屏的界面，有历史栈管理（打开新 Window 时自动处理旧 Window）
- **Panel 层**：附着在某个父界面上的子面板，可独立显示隐藏

两层共享统一的 `UIFrame` MonoBehaviour 入口，挂载在场景的 MainCanvas 上。

## 关键代码位置

| 模块 | 路径 |
|---|---|
| UIFrame 核心 | `client/Assets/Packages/com.game.framework.bsk.uiframework/Runtime/UIFramework/` |
| 信号系统 | `client/Assets/Packages/com.game.framework.bsk.uiframework/Runtime/Signals/` |
| 业务窗口基类 | `client/Assets/Game/Scripts/Windows/Base/` |
| 具体 UIForm | `client/Assets/Game/Scripts/Windows/UIForm/` |
| HUD | `client/Assets/Game/Scripts/Windows/Hud/` |
| UI 预制体资源 | `client/Assets/GameRes/UI/UIForms/` |

## UIFrame 核心机制

详见 `references/ui-framework.md`。

简要：
- `UIFrame.cs` 是中央访问点，提供 `OpenWindow` / `CloseWindow` / `OpenPanel` / `ClosePanel` / `OpenScreen` 等统一 API
- 界面通过 `RegisterScreen(screenId, controller)` 注册后才能打开
- 支持可配置 tick 的 Update/LateUpdate/FixedUpdate 驱动
- 内置转场锁：转场期间屏蔽 GraphicRaycaster 防止误触
- 三个特殊 Transform：`cacheTrans`（缓存已关闭未销毁的 UI）、`preloadTrans`（预加载）、`childPanelsTrans`（子面板容器模板）

## 现有界面

详见 `references/ui-forms.md`。

## 修改指南

- 新增 Window：创建 Controller 类继承 IWindowController，在 `UIForm/` 下建子目录，制作 Prefab 放入 `GameRes/UI/UIForms/`
- 新增 Panel：创建 Controller 类继承 IPanelController，可挂靠到某个 Window 下
- UI 预制体注册：通过 UIForm 数据表（DRUIForm）关联 screenId 和资源路径
