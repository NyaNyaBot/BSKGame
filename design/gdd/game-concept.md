# Game Concept: 回响之刃 (Echo of Blades)

*Created: 2026-04-12*
*Status: Draft*

---

## Elevator Pitch

> 一款短小精悍的回合制 RPG，你带领三人小队深入崩塌的钟楼遗迹。每场战斗中，当敌人攻击时你必须精准判断时机弹反——完美弹反触发华丽的全员反击。探索精心设计的箱庭关卡，在陪伴三位角色走完旅程的过程中，揭开钟楼的秘密。

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | 回合制 RPG + 实时弹反操作 |
| **Platform** | 微信小游戏（WebGL / 移动端浏览器） |
| **Target Audience** | 欣赏精品短流程 RPG 的中核玩家 |
| **Player Count** | 单人 |
| **Session Length** | 30-45 分钟（完整通关） |
| **Monetization** | 无（学习项目） |
| **Estimated Scope** | 小型（3-5 周，独立开发） |
| **Comparable Titles** | Clair Obscur: Expedition 33, Undertale, Ikenfell |

---

## Core Fantasy

你是钟楼深处最后的防线——当敌人的致命一击即将落下时，你精准地读取前摇、把握时机、完美弹反。金属碰撞声炸响，三位伙伴同时挥出反击，屏幕上绽放华丽的连携演出。

这不是数值碾压的快感，而是"操作精准度=团队命运"的纯粹技巧幻想。你每一次弹反都在保护同伴，每一次全员完美弹反都是你对这支小队的承诺。

---

## Unique Hook

像经典 JRPG 一样选择指令排兵布阵，**AND ALSO** 像动作游戏一样在敌人攻击瞬间做出反应弹反——弹反成功与否直接决定战斗走向，全员完美弹反触发独特的连携反击演出。

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Sensation** (感官快感) | 1 | 完美弹反的顿帧、金属音效、全屏特效、连携反击镜头演出 |
| **Challenge** (挑战精通) | 2 | 每种敌人有独特前摇节奏，Boss 有多阶段弹反模式，逐层难度递进 |
| **Narrative** (叙事弧光) | 3 | 三位角色各自的故事通过探索和战斗间的对话渐次展开 |
| **Discovery** (探索发现) | 4 | 箱庭钟楼中隐藏道具、环境叙事、可选分支路线 |
| **Fantasy** (角色扮演) | 5 | "保护同伴的守护者"身份认同，弹反=守护 |
| **Fellowship** (社交连接) | N/A | 纯单人体验 |
| **Expression** (自我表达) | N/A | 不含自定义/创造系统 |
| **Submission** (放松) | N/A | 本作追求挑战而非放松 |

### Key Dynamics

- 玩家会自发地观察和记忆每种敌人的前摇动画模式，形成"读取 → 预判 → 反应"的肌肉记忆
- 遭遇新敌人时先保守试探，逐渐掌握后追求全员完美弹反的极限操作
- 主动探索箱庭的每个角落以获取记忆碎片，提升角色能力和了解角色故事

### Core Mechanics

1. **回合指令系统** — 回合制策略层，选择攻击技能、辅助动作、道具
2. **实时弹反系统** — 敌方攻击时，判断前摇时机精准按下对应角色的弹反键；完美弹反触发反击，全员完美弹反触发回响连携
3. **回响值系统** — 连续完美弹反累积回响值，积满触发角色大招
4. **箱庭探索系统** — 线性-分支结构的钟楼关卡，环境交互、隐藏道具、角色对话触发
5. **记忆碎片成长** — 通过探索获得碎片，解锁角色被动能力（弹反窗口拓宽、反击增强等）

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** | 指令阶段自由选择策略；探索路线有分支；升级路线可选 | Supporting |
| **Competence** | 弹反 timing 从宽松→严格递进；Boss 有独特模式需要学习；全员完美弹反是技巧天花板 | Core |
| **Relatedness** | 三位角色性格鲜明，对话渐进展开，战斗中互相呼应——"为了他们而战" | Supporting |

### Player Type Appeal (Bartle Taxonomy)

- [x] **Achievers** — 追求全员完美弹反、零失误通关、隐藏结局解锁
- [x] **Explorers** — 箱庭中的隐藏路线、环境叙事、记忆碎片收集
- [ ] **Socializers** — 不涉及
- [ ] **Killers/Competitors** — 不涉及

### Flow State Design

- **新手引导**：第一层前 2 场战斗的弹反窗口较宽，敌人前摇明显且缓慢，让玩家建立"看→按→爽"的基础循环
- **难度递进**：每层敌人前摇缩短、增加欺骗性动作、出现多方向同时攻击，要求更高的注意力分配
- **反馈清晰度**：完美弹反 → 金属音效 + 顿帧 + 粒子爆发；普通弹反 → 较弱反馈；失败 → 角色受伤动画 + 屏幕震动。玩家始终清楚自己的表现
- **失败恢复**：遭遇战失败可立即重试，Boss 战从当前阶段重新开始，惩罚轻但不免费

---

## Core Loop

### Moment-to-Moment (30 seconds)

**指令阶段**：选择角色的进攻技能、辅助动作、道具使用——纯策略思考，不赶时间。

**执行阶段**：我方攻击自动播放。轮到敌方攻击时，敌人展示前摇动画，玩家在攻击命中前精准按下对应角色的弹反键：
- **完美弹反** → 屏幕微顿、金属音效爆响、自动施加反击
- **全员完美弹反** → 触发"回响连携"——三人同时反击的华丽演出，伤害大幅增加
- **弹反失败** → 受到完整伤害 + 短暂硬直，下回合该角色行动延迟

### Short-Term (5-15 minutes)

每场遭遇战 3-5 回合。敌人组合多样化：快攻型（前摇短）、重击型（前摇长但连击）、施法型（欺骗性前摇）。连续完美弹反提升回响值，积满触发角色大招。战斗结束获得记忆碎片用于升级。

### Session-Level (30-45 minutes)

一座 3-4 层的箱庭钟楼。每层：探索环境 → 1-2 场遭遇战 → 精英怪 / Boss → 角色对话休息点。自然停止点在每层清完后。最终 Boss 在塔顶，击败后故事收束。

### Long-Term Progression

- 记忆碎片解锁角色被动能力（拓宽弹反窗口 / 增加反击效果）
- 探索解锁角色对话，逐步揭示完整故事
- "完成"标志：击败最终 Boss，揭示完整叙事
- 可选挑战：全程"回响不断"（不失败弹反）解锁隐藏结局

### Retention Hooks

- **Curiosity**：下一层有什么？角色的故事接下来会怎样？
- **Investment**：对三位角色产生情感连接后想看到结局
- **Mastery**：追求全员完美弹反的零失误通关，挑战更严格的隐藏结局条件

---

## Game Pillars

### Pillar 1: 精准即快感 (Precision is Pleasure)
每一次精准操作都必须获得与之匹配的感官回报。视听反馈是游戏的灵魂。

*Design test*: 如果在"增加策略深度"和"让弹反手感更爽"之间二选一，我们选**手感更爽**。

### Pillar 2: 小而精致 (Small but Exquisite)
30 分钟的高密度体验胜过 30 小时的稀释内容。每一层、每一场战斗、每一段对话都必须是精心设计的。

*Design test*: 如果在"多做一层"和"打磨现有层的细节"之间二选一，我们选**打磨细节**。

### Pillar 3: 角色即动力 (Characters Drive the Journey)
玩家不只是"通关一座塔"——他们是在陪三个人走完一段旅程。探索和战斗的动力来自对角色的关心。

*Design test*: 如果在"加一个酷炫新机制"和"加一段揭示角色过去的对话"之间二选一，我们选**角色对话**。

### Pillar 4: 层层递进 (Escalating Mastery)
难度、叙事张力、视听表现力同步递进。第一层教会你弹反，最后一层让你觉得自己是大师。

*Design test*: 如果在"让每层体验一致"和"让后期明显更难更华丽"之间二选一，我们选**后期更强**。

### Anti-Pillars (What This Game Is NOT)

- **NOT 开放世界探索**：箱庭是精心布局的线性-分支结构，因为开放世界会稀释"小而精致"支柱
- **NOT 数值成长膨胀**：不做装备系统/等级系统/属性加点，成长体现在操作技巧提升和少量被动解锁，因为数值膨胀会削弱"精准即快感"
- **NOT 多结局分支叙事**：叙事是线性但有深度的角色弧光（仅一个隐藏结局作为技巧挑战奖励），因为分支叙事会炸开内容体量
- **NOT 联机/社交功能**：这是一个纯粹的单人体验，因为联机延迟与弹反精度不兼容

---

## Visual Identity Anchor

由于评审模式为 Lean，视觉锚点未经 Art Director 评审。以下为初步方向：

- **参考方向**：Expedition 33 的印象派油画风 × 微信小游戏性能限制 → 建议 **风格化 2.5D / 手绘低面数**
- **视觉规则**：弹反瞬间是视觉最高潮——所有视觉预算优先服务战斗反馈
- **色彩哲学**：钟楼内部以冷色调（深蓝、灰紫）为基底，弹反成功时暖色（金、橙）爆发，形成冷暖对比的视觉节奏

*此部分将在 `/art-bible` 中详细展开。*

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently | Why It Matters |
| ---- | ---- | ---- | ---- |
| Clair Obscur: Expedition 33 | 弹反驱动的回合制战斗，全员反击的爽感，精致角色塑造 | 极度压缩体量为 30-45 分钟精品体验；箱庭探索而非半开放 | 验证了"回合制+实时操作"组合的市场接受度 |
| Undertale | 短小精悍的 RPG，独特战斗机制，强角色情感 | 弹反机制替代躲弹幕；3D 表现替代像素风 | 证明短流程 RPG 可以产生强烈情感共鸣 |
| Dark Souls 3 | 读取敌人动作前摇→精准反应的"学习-精通"循环 | 回合制框架降低操作门槛，但保留弹反的判断深度 | 验证了挑战驱动的重复游玩动力 |
| Hi-Fi Rush | 音效节奏与战斗操作深度融合的视听爽感 | 不做全程节奏绑定，弹反是局部实时操作 | 证明了视听反馈可以将普通操作提升为"爽"的体验 |

**Non-game inspirations**：钟楼建筑的哥特式美学——向上攀登的垂直空间感，齿轮与钟摆的机械节奏暗合弹反的时机主题。

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Age range** | 18-30 |
| **Gaming experience** | 中核至硬核，熟悉回合制 RPG 和动作游戏 |
| **Time availability** | 微信小游戏的碎片场景，但能投入 30-45 分钟完整通关 |
| **Platform preference** | 手机微信小游戏 |
| **Current games they play** | Expedition 33, 原神, 各类微信小游戏 |
| **What they're looking for** | 在碎片时间体验高品质、有深度的回合制战斗和叙事 |
| **What would turn them away** | 低操作反馈（按了没感觉）、过长的流程/重复磨损、纯数值堆砌 |

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Engine** | Unity 2022.3（已有微信小游戏导出管线：HybridCLR + WebGL WASM） |
| **Key Technical Challenges** | ① 微信小游戏 WebGL 环境下的实时弹反输入延迟控制 ② 触屏弹反操作的精度与手感设计 ③ 小游戏包体限制下的资源管理 |
| **Art Style** | 风格化 2.5D / 手绘低面数（兼顾表现力与小游戏性能） |
| **Art Pipeline Complexity** | 中等（需要角色模型/立绘、关卡环境、特效） |
| **Audio Needs** | 中等偏高——弹反音效设计是核心体验，需要高品质打击/金属音效 |
| **Networking** | 无（纯单人） |
| **Content Volume** | 3-4 层关卡，6-8 场遭遇战，3 个精英怪，1 个 Boss（2 阶段），15-20 段对话 |
| **Procedural Systems** | 无——所有内容手工设计，服务"小而精致"支柱 |

---

## Risks and Open Questions

### Design Risks
- 弹反 timing window 的参数调优需要大量 playtest 迭代，几周内可能无法打磨到"爽"的程度
- 触屏弹反操作可能比手柄/键盘精度低，需要针对移动端特别设计（如更宽的完美窗口、视觉引导）
- 30-45 分钟流程中的叙事密度需要把控——太少无感，太多打断战斗节奏

### Technical Risks
- 微信小游戏 WebGL 环境可能存在输入延迟，影响弹反手感的精确度
- 小游戏包体限制可能制约美术和音效资源质量
- HybridCLR 热更环境下实时弹反系统的性能表现需要验证

### Market Risks
- 作为学习项目不涉及商业化，此风险可忽略
- 微信小游戏平台用户对"硬核弹反操作"的接受度待验证

### Scope Risks
- 角色叙事（15-20 段对话）的文本质量依赖 AI 辅助写作，质量上限待验证
- 美术资产制作（3D 角色、环境、特效）可能是最大时间瓶颈

### Open Questions
- 微信小游戏触屏环境下，弹反操作的最佳交互方案是什么？（点击 / 滑动 / 多指？）— 需通过 MVP 原型验证
- 弹反完美窗口应设为多少帧？移动端是否需要比 PC 端更宽的窗口？— 需 playtest 数据
- AI 辅助写作能否产出有足够情感密度的角色对话？— 在 Tier 2 阶段验证

---

## MVP Definition

**Core hypothesis**: 在微信小游戏环境下，弹反驱动的回合制战斗在触屏上是有足够爽感的。

**Required for MVP**:
1. 1 层钟楼箱庭（可探索，有道具和环境元素）
2. 1 个可控角色（弹反系统完整运作）
3. 2 场遭遇战（不同敌人类型，不同前摇模式）
4. 1 场 Boss 战（有独特弹反模式）
5. 完整的弹反视听反馈链（音效 + 顿帧 + 特效）

**Explicitly NOT in MVP**:
- 3 人队伍系统（MVP 只需 1 人验证弹反手感）
- 角色对话/叙事系统
- 记忆碎片成长系统
- 回响连携演出

### Scope Tiers

| Tier | Content | Features | Timeline |
| ---- | ---- | ---- | ---- |
| **MVP** | 1 层 + 3 种敌人 + 1 Boss | 弹反核心 + 基础 UI | 1-1.5 周 |
| **Vertical Slice** | 2 层 + 3 角色 + 对话 | 回响连携 + 记忆碎片 + 角色切换 | 2-3 周 |
| **Alpha** | 3-4 层 + 完整叙事 | 全系统功能完整 | 3-4 周 |
| **Full Vision** | 4 层 + 隐藏结局 + 音效打磨 | 全系统打磨 + 挑战模式 | 4-5 周 |

---

## Next Steps

- [ ] 配置引擎技术栈 (`/setup-engine`)
- [ ] 建立视觉身份规范 (`/art-bible`)
- [ ] 验证概念完整性 (`/design-review design/gdd/game-concept.md`)
- [ ] 拆解系统结构与依赖 (`/map-systems`)
- [ ] 逐系统撰写 GDD (`/design-system`)
- [ ] 创建架构蓝图 (`/create-architecture`)
- [ ] 记录关键架构决策 (`/architecture-decision`)
- [ ] 阶段门控验证 (`/gate-check`)
- [ ] 原型验证核心循环 (`/prototype 弹反战斗`)
- [ ] 原型试玩报告 (`/playtest-report`)
- [ ] 规划第一个冲刺 (`/sprint-plan new`)
