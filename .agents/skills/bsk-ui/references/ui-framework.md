# UIFrame 框架详解

## 包信息

包名: `com.game.framework.bsk.uiframework`
路径: `client/Assets/Packages/com.game.framework.bsk.uiframework/`
程序集: `BSK.Game.Framework.UI`
依赖: DOTween（动效）

## 核心类

### UIFrame（中央访问点）

`UIFramework/UIFrame.cs` — MonoBehaviour，挂载在场景 MainCanvas 上。

关键字段：
- `panelLayer` — PanelUILayer，管理所有 Panel
- `windowLayer` — WindowUILayer，管理所有 Window（含历史栈）
- `mainCanvas` / `graphicRaycaster` — 主画布及射线检测
- `cacheTrans` — 存放关闭但未销毁的 UI
- `preloadTrans` — 存放预加载中的 UI
- `childPanelsTrans` — clone 到每个 UI 下的子面板容器模板
- `uiCamera` — UI 相机

核心 API：
```
OpenWindow(screenId) / CloseWindow(screenId)
OpenPanel(screenId, parentIds?) / ClosePanel(screenId)
OpenScreen<T>(screenId, param, parentIds?, isImmediate?)
CloseScreen(screenId)
RegisterScreen(screenId, controller)
IsScreenOpen(screenId) / IsScreenRegistered(screenId)
HideAllScreen() / ShowAllScreen()
CloseAll() / ClearAllCache()
```

### 层级结构

```
UIFramework/
├── Core/
│   ├── AUILayer.cs              # UI 层抽象基类
│   ├── AUIScreenController.cs   # 屏幕控制器抽象基类
│   ├── IUIScreenController.cs   # 控制器接口（ScreenId/IsVisible/Show/Hide）
│   ├── IParameterData.cs        # 界面参数接口
│   └── IWindowController.cs / IPanelController.cs  # 分类接口
├── Panel/
│   ├── PanelUILayer.cs          # Panel 层实现
│   └── IPanelController 相关
├── Window/
│   ├── WindowUILayer.cs         # Window 层实现（含栈管理）
│   ├── WindowPriority.cs        # 窗口优先级
│   └── WindowProperties.cs      # 窗口属性
├── ScreenTransitions/
│   ├── SimpleFadeTransition.cs  # 简单淡入淡出
│   └── LegacyAnimationTransition.cs  # Legacy 动画转场
├── UIFrame.cs                   # 中央入口
└── UISettings.cs                # UI 配置
```

### Tick 机制

UIFrame 支持为每个 ScreenController 注册独立的 Update 驱动：
- `AddUpdate(ctl, tick)` — 每 tick 帧调用一次 UIUpdate
- `AddLateUpdate(ctl, tick)` / `AddFixedUpdate(ctl, tick)`
- tick = 0 表示每帧都调用

这允许不同界面以不同频率刷新，节省性能。

### 转场管理

打开/关闭界面时如果有转场动画，UIFrame 会：
1. `AddTransition(screen)` — 将界面加入转场中集合
2. 禁用 `GraphicRaycaster` 防止误触
3. 转场完成后 `RemoveTransition(screen)` — 恢复输入

### 信号系统

`Signals/Signals.cs` — 轻量的事件/信号机制，用于 UI 间解耦通信。
