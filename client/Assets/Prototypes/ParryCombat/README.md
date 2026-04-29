# Parry Combat Prototype

> PROTOTYPE - NOT FOR PRODUCTION

## 核心问题

在微信小游戏 WebGL 触屏环境下，实时弹反操作能否实现足够精准和"爽"的手感？

## 快速开始

### 方法 A：自动搭建（推荐）

1. 在 Unity 中创建新场景：`File > New Scene > Basic (Built-in)`
2. 删除场景中的默认对象（保留 Camera 和 Light 即可，脚本会自动创建缺失对象）
3. 创建一个空 GameObject，命名为 `ParryPrototype`
4. 挂载以下 3 个脚本：
  - `ParryPrototypeController`
  - `ParryFeedback`
  - `ParryDebugPanel`
5. Play — 场景会自动生成 Enemy Cube、Player Cube、地板和灯光

### 方法 B：手动搭建

1. 创建新场景
2. 创建两个 Cube：
  - `Enemy` 放在 (0, 0.5, 3)
  - `Player` 放在 (0, 0.5, -2)
3. 创建空 GameObject `ParryPrototype`，挂载 3 个脚本
4. 将 Enemy 和 Player 的 Transform 拖入 Controller 的对应槽位
5. Play

## 操作方式

- **Editor**：鼠标左键点击 = 弹反
- **WebGL/移动端**：触屏点击 = 弹反
- 在敌人方块变黄（Parry Window）时点击
- 目标是在窗口正中央点击以获得 Perfect

## 视觉信号


| 敌人颜色  | 阶段           | 操作        |
| ----- | ------------ | --------- |
| 灰色    | Idle         | 等待        |
| 灰→红渐变 | Wind Up（前摇）  | 准备弹反      |
| 亮黄色   | Parry Window | **现在点击!** |
| 红色    | Strike       | 太晚了       |
| 灰色渐回  | Cooldown     | 等待下一轮     |


## 反馈效果


| 结果      | 顿帧    | 震屏  | 闪光  | 粒子  | 缩放   |
| ------- | ----- | --- | --- | --- | ---- |
| Perfect | 120ms | 强   | 金色  | 是   | 1.5x |
| Good    | 50ms  | 中   | 白色  | 否   | 1.2x |
| Miss    | 无     | 中   | 红色  | 否   | 无    |


## Debug 面板

左上角显示实时数据：

- FPS、TimeScale
- 弹反统计（Perfect/Good/Miss 次数和比率）
- 输入延迟测量（平均/最小/最大）
- 弹反偏移量（正=偏晚，负=偏早）
- **可调参数滑块**：弹反窗口、完美窗口、前摇时长、顿帧时长、震屏强度

## WebGL 测试

1. `File > Build Settings > WebGL`
2. Build
3. 部署到微信小游戏或本地 HTTP 服务器
4. 在真机上测试触屏弹反手感
5. 记录 Debug 面板中的延迟数据

## 关键参数（默认值）


| 参数                 | 默认值   | 说明          |
| ------------------ | ----- | ----------- |
| Parry Window       | 300ms | 可弹反的总窗口时长   |
| Perfect Window     | 100ms | 窗口中央的完美判定范围 |
| Wind Up            | 800ms | 前摇动画时长      |
| Hit Stop (Perfect) | 120ms | 完美弹反顿帧      |
| Shake Intensity    | 0.25  | 震屏强度        |


## 文件结构

```
client/Assets/Prototypes/ParryCombat/
├── README.md
└── Scripts/
    ├── ParryPrototypeController.cs  — 主循环、弹反判定、状态机
    ├── ParryFeedback.cs             — 视觉反馈（顿帧/震屏/闪光/粒子/缩放）
    └── ParryDebugPanel.cs           — IMGUI 调试面板 + 参数调整
```

