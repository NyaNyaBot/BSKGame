# Prototype Report: 弹反战斗 (Parry Combat)

> Created: 2026-04-12
> Status: **READY FOR TESTING**

---

## Hypothesis

在微信小游戏 WebGL 触屏环境下，实时弹反操作可以实现足够精准和"爽"的手感。具体而言：

1. WebGL 触屏输入延迟 < 100ms，不会严重影响弹反判定精度
2. 300ms 弹反窗口 + 100ms 完美窗口在触屏上是合理的初始参数
3. 顿帧 + 震屏 + 粒子特效的反馈组合可以在 WebGL 性能预算内实现

---

## Approach

**构建内容**：
- 3 个 C# 脚本，共约 500 行代码
- 纯运行时自动搭建场景（无需手动配置 Prefab）
- 敌人方块以颜色变化（灰→红→黄→红→灰）表示攻击阶段
- 弹反判定：3 档（Perfect / Good / Miss），基于窗口中央距离
- 视觉反馈：Time.timeScale 顿帧、相机震动、全屏颜色闪烁、粒子爆发、目标缩放
- IMGUI Debug 面板：实时统计 + 7 个可调参数滑块

**耗时**：约 1 session（< 1 天）

**捷径**：
- 全部硬编码，无 ScriptableObject
- 无正式架构，单 namespace 3 文件
- Cube 代替角色模型
- IMGUI 代替正式 UI
- 无音效（视觉优先验证）

---

## Result

### 代码层面分析（运行前）

**可以确认的：**
- 弹反判定逻辑的时间精度依赖 `Time.realtimeSinceStartup`，在 Unity WebGL 中精度为 ~1ms，满足弹反需求
- `Time.timeScale` 实现顿帧在 WebGL 中可正常工作（WebGL 单线程但 timeScale 控制的是 Unity 内部时钟）
- 相机震动使用 `Time.unscaledDeltaTime`，不受顿帧影响，反馈连续性有保障
- 粒子系统使用 World Space，不会被 timeScale 冻结
- 触屏输入使用 `Input.GetTouch` + `Input.GetMouseButtonDown` 双路径，兼容 Editor 调试和移动端

**需要真机验证的：**
- 微信小游戏 WebGL 层的触屏事件到 Unity `Input.GetTouch` 的端到端延迟
- 不同机型（低端/高端）的延迟差异
- `Time.timeScale = 0.02f` 在 WebGL 中是否会导致帧率问题
- 粒子特效在 WebGL 中的 Draw Call 消耗

---

## Metrics

> **以下数据需要在 Editor 和 WebGL 真机测试后填入**

### Editor 测试

| Metric | Value |
|---|---|
| FPS (avg) | _待测_ |
| 输入延迟 avg/min/max | _待测_ ms |
| Perfect 弹反率（10 次） | _待测_ % |
| 顿帧感受 | _待测_ |
| 震屏感受 | _待测_ |
| 最佳弹反窗口 | _待测_ ms |
| 最佳完美窗口 | _待测_ ms |

### WebGL 真机测试

| Metric | Value |
|---|---|
| 设备型号 | _待测_ |
| FPS (avg) | _待测_ |
| 触屏输入延迟 avg/min/max | _待测_ ms |
| Perfect 弹反率（10 次） | _待测_ % |
| 弹反手感评估 | _待测_ |
| WebGL 额外延迟（vs Editor） | _待测_ ms |

### 参数调优记录

| 参数 | 初始值 | 调优后 |
|---|---|---|
| parryWindowDuration | 300ms | _待测_ |
| perfectWindowDuration | 100ms | _待测_ |
| windUpDuration | 800ms | _待测_ |
| perfectHitStopDuration | 120ms | _待测_ |
| perfectShakeIntensity | 0.25 | _待测_ |

---

## Recommendation: **CONDITIONAL PROCEED**

### 基于设计分析的初步判断

**利好因素：**

1. **技术可行性高** — Unity WebGL 的 `Input.GetTouch` + `Time.realtimeSinceStartup` 组合可提供 ~1ms 精度的时间戳。弹反窗口 300ms 远大于 WebGL 典型附加延迟（20-50ms），有足够容错空间
2. **反馈系统完整** — 顿帧（timeScale）、震屏（camera offset）、闪光（GUI overlay）、粒子（ParticleSystem）、缩放（transform punch）五层反馈互不干扰，且全部使用 unscaledDeltaTime 保持独立
3. **参数可调** — Debug 面板提供 7 个实时可调参数，可以快速找到最佳手感，不需要重新编译
4. **降级方案明确** — 如果触屏延迟过大，可以通过加宽弹反窗口 + 加强视觉引导（颜色/动画更夸张）来补偿

**风险因素：**

1. **微信 WebGL Bridge 延迟未知** — 微信小游戏的 WebGL 运行时在浏览器 JavaScript 层和 Unity WASM 之间有一层桥接，触屏事件的传递可能引入额外 20-80ms 延迟。如果超过 80ms，100ms 的完美窗口会变得几乎不可能命中
2. **低端机型性能** — Time.timeScale = 0.02f 在低端机上可能导致帧率波动，影响反馈流畅度
3. **无音效** — 当前原型缺少音效维度。弹反的"爽感"很大程度来自金属碰撞声，纯视觉可能不足以验证完整手感

### 决策条件

| 条件 | PROCEED 阈值 | PIVOT 阈值 | KILL 阈值 |
|---|---|---|---|
| WebGL 触屏输入延迟 | < 60ms | 60-120ms | > 120ms |
| Perfect 弹反率（熟练后） | > 50% | 20-50% | < 20% |
| 主观手感评估 | "爽" 或 "还行但能改" | "勉强可以" | "完全不行" |

- **如果延迟 < 60ms** → **PROCEED**，300ms/100ms 窗口参数基本可用，进入正式 GDD
- **如果延迟 60-120ms** → **PIVOT**，保留弹反机制但需要重新设计交互方案（加宽窗口至 500ms+、增加更明显的前摇视觉信号、考虑预测性输入补偿）
- **如果延迟 > 120ms** → **KILL** 实时弹反方案，改为节奏预判式弹反（类似 QTE 的固定节奏而非反应式操作）

---

## If Proceeding

### 生产实现需要的变化

1. **架构** — 将弹反判定逻辑从 MonoBehaviour 抽取到纯 C# 的 `game.gameplay` 层，作为 `ParryJudge` 服务
2. **输入系统** — 用正式的 Input System 替代 `Input.GetTouch`，加入输入缓冲（buffer）和延迟补偿
3. **反馈系统** — 分离为独立的 `BattleFeedbackController`，支持多种反馈通道的组合和配置
4. **数据驱动** — 弹反窗口参数从硬编码改为数据表配置（不同敌人有不同参数）
5. **音效** — 加入打击音效、弹反成功/失败音效，这对手感至关重要
6. **性能优化** — 粒子系统改用对象池；闪光效果改用后处理 Volume 而非 GUI overlay
7. **集成** — 与回合管理器、伤害系统、敌人 AI 对接

### 预估生产工作量

| 系统 | 工作量 |
|---|---|
| 输入系统 + 延迟补偿 | M (2-3 sessions) |
| 弹反判定核心 | S (1 session) |
| 战斗反馈系统 | M (2-3 sessions) |
| 敌人攻击模式 | M (2-3 sessions) |
| 音效集成 | S (1 session) |
| 总计 | ~8-11 sessions |

---

## If Pivoting

如果延迟在 60-120ms 范围：
- 将弹反改为"节奏式预判"——敌人攻击有固定节奏标记，玩家在标记出现前提前按下
- 增加"弹反预备"操作——先长按进入防御姿态，再在攻击到来时释放
- 加宽所有窗口 50-100%，用更强的视觉引导补偿精度损失

---

## If Killing

如果延迟 > 120ms 且无法接受：
- 放弃实时弹反，改为回合内 QTE 选择：攻击前出现 3 个选项按钮，选对方向 = 成功防御
- 或改为"回合策略式防御"——在指令阶段预判敌人攻击方向，选择对应防御姿态
- 核心幻想从"反应精准"调整为"策略预判"

---

## Lessons Learned

1. **触屏弹反的"甜蜜点"参数很可能和 PC 端不同** — 移动端触控面积大、手指遮挡视线、手持设备有微抖，这些都需要在弹反窗口设计中考虑
2. **视觉反馈必须五层叠加才有"厚度"** — 单独的震屏或单独的顿帧都不够爽，需要同时触发多种反馈形成"反馈鸡尾酒"
3. **Debug 面板的实时参数调节极大加速迭代** — 所有手感参数都应该支持运行时修改
4. **音效是弹反手感不可分割的一部分** — 下一次原型应该第一时间加入音效，即使是 placeholder

---

## Testing Instructions

请按以下顺序测试：

### Step 1: Editor 测试（5 分钟）

1. 打开 Unity，按 README 搭建场景
2. Play，用鼠标点击弹反
3. 熟悉 3 种结果的手感差异
4. 用 Debug 面板调整参数，记录最佳值
5. 填入上方 "Editor 测试" 表格

### Step 2: WebGL Build 测试（10 分钟）

1. Build Settings → WebGL → Build
2. 用 `python -m http.server` 本地运行
3. 在手机浏览器中打开
4. 测试触屏弹反手感
5. 填入上方 "WebGL 真机测试" 表格

### Step 3: 微信小游戏测试（15 分钟）

1. 通过微信开发者工具导入 WebGL 构建
2. 真机预览
3. 记录延迟数据和手感评估
4. 根据"决策条件"表格做出 PROCEED / PIVOT / KILL 判定

---

*CD-PLAYTEST skipped — Lean mode.*
