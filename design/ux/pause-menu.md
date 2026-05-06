# UX Spec: Pause Menu（暂停菜单）

> **Status**: Revised — `/ux-review` 复审通过（2026-05-06）；宿主暂停策略已闭合  
> **Author**: (contributor + ux-designer)  
> **Last Updated**: 2026-05-06  
> **Platform Target**: 微信小游戏 WebGL；**Touch Full**（见 `technical-preferences.md` § Input & Platform）  
> **Journey Phase(s)**: Unknown — 无 `design/player-journey.md`  
> **Template**: UX Spec  
> **MVP Scope**（AskQuestion）:**仅战斗内暂停** — 与 `Suspended`、`CombatClock` 停顿、战斗触区禁用一致  
> **Related GDD**: `design/gdd/战斗 UI.md`（§ Edge/UI：`Suspended` 注销战斗触区）；`design/gdd/回合管理器.md`（`Suspended`/Resume）；`design/gdd/输入系统.md`（`Suspended` 拒答战斗输入）；`design/gdd/场景管理.md`（必要时与场景切换 `Suspended` 对齐）  
> **Related UX**: `design/ux/main-menu.md`（返回标题后的上下文丢弃）；`design/ux/hud.md`（`Suspended`）；`design/ux/interaction-patterns.md`（**Pause Overlay** / Modal priority **250**）  
> **Return Title UX**（AskQuestion）:**总是阻断式二次确认**后才卸载战斗并回主菜单  

---

## Purpose & Player Need

玩家在**战斗进行中**主动打断节奏：在不丢失公平性与可操作审计的前提下，**冻结战斗时间与战斗相关输入**，并给出三条可控出路：**回到战斗（恢复）**、**次要选项（设置/关于）**、**退出本场返回主菜单**。

**玩家要完成的事**

| 目标 | 说明 |
|------|------|
| **安全喘息** | 暂停时不误判弹反、不囤积指针输入；恢复后继续看清局势；**宿主切入后台**与手动暂停共享同一 `Suspended` 语义，回到前台时须 **明示暂停 Overlay**，避免「时钟已停却看不见菜单」 |
| **配置 / 信息** | 可调设置或查看关于，且不偷偷解开战斗输入契约 |
| **体面退场** | 返回主菜单前明确知情确认（已定二次对话框），避免误触清空本场 |

**若缺失**

- 违反战斗 UI / 输入 / 回合 GDD 中 **`Suspended`** 契约（战斗中仍能写入 Parry 等）。
- `interaction-patterns` 中 **Pause Overlay** 与 battle hit-area 生命周期脱节。
- 与 `design/ux/main-menu.md` 中「返回标题丢弃战斗上下文」不对齐。

---

## Player Context on Arrival

- **触发**：玩家在战斗中点击 HUD **暂停**入口（或其它等价入口；以实现挂载为准）；亦可来自微信宿主生命周期触发「等价暂停」（须在 States 中单列）。  
- **前一动作**：正读取敌方前摇 / 等待指令 / Counter 窗口内均可暂停——暂停瞬间交互必须从「战斗操作」切换到「菜单操作」。  
- **情绪**：高压中求可控停顿——布局必须 **立刻可读**，主次分明；**功能优先**。  
- **自愿**：默认自愿暂停（玩家发起）。

---

## Navigation Position

`Gameplay Battle Layer → BattleHud（战斗中）→ Pause Overlay（模态壳）→ [可选] SettingForm / AboutForm（卫星 UI）→ [可选] ConfirmQuitDialog（阻断二次确认）→ Main Menu Flow`。

暂停 Overlay **不是**全局路由根；**低于系统宿主对话框**，高于战斗 HUD 与战斗 hit-area。**探索暂停不在 MVP 范围**，若在日后共用 PauseForm，须增补上下文小节（当前 Open Questions 钩子）。

---

## Entry & Exit Points

### Entry（到达）

| Entry Source | Trigger | Player carries this context |
|--------------|---------|------------------------------|
| 战斗 HUD — 暂停 | 玩家 Tap HUD「暂停」 | `battleContextId` / phase — **冻结**：不向 gameplay 投递未完成手势 |
| 宿主等价暂停 | 微信小游戏 **可见性丢失 / onHide**（或工程等价宿主暂停钩子） | **已定案**：立即进入与 HUD 暂停 **同源**的 `Suspended`（时钟停、战斗触区禁用）；**回到前台时**若仍处于 Suspended，则 **必须呈现与手动暂停相同的 Pause Overlay**（见 States），事件 `reason=host_pause` |

### Exit（离开）

| Exit Destination | Trigger | Notes |
|------------------|---------|-------|
| **恢复战斗** | Tap「继续」/ Resume | 关闭 Pause Overlay；**按最新 battle/input phase 重建战斗触区**（对齐 HUD AC）；**不补发**暂停期内缓冲的战斗手势 |
| 设置 | Tap「设置」 | `OpenUIForm(SettingForm)`；底层仍为 Suspended |
| 关于 | Tap「关于」 | 同上 |
| **返回主菜单** | Tap「返回主菜单」→ **确认对话框 → 确认** | **卸载本场战斗上下文**（对齐 main-menu Entry）；走后接入 `EnterMenu` 或以现有 Procedure 为准 |
| 取消退出 | Confirm 对话框 **取消** | 留在 Pause Overlay，不改变暂停状态 |

---

## Layout Specification

### Information Hierarchy

| 优先级 | 元素 | 说明 |
|--------|------|------|
| **P0** | 「继续」/ Resume | 最显眼、误触成本最低的返程 |
| **P1** | 「返回主菜单」 | 次显眼但必须经由二次确认才生效 |
| **P2** | 「设置」「关于」 | 与 main-menu 一致卫星 UX |
| **P3** | **静音**：半透明遮挡战场（仍可辨认明暗以防眩晕过载）；禁用 HUD 战斗控件可读外形 |

### Layout Zones

| Zone | 职责 |
|------|------|
| **Backdrop（半透明遮罩）** | 阻断对战场的直接交互；不遮挡系统安全区外的可读边界提示 |
| **暂停卡片 / Panel（居中或底部抽屉）** | 容纳按钮栈；适配全面屏手势条 |
| **二次确认对话框** | `QuitConfirmationDialog`，居中 modal；优先级高于 Pause Panel |

### Component Inventory

| Zone | 组件 | Pattern | 交互 |
|------|------|---------|------|
| Backdrop | 半透明 Image（可选 Tap-to-dismiss：**默认 OFF**，防止误触恢复 — MVP **必须通过按钮恢复**，避免手势争端） | Pause Overlay | 无穿透命中战斗层 |
| Panel | Primary Button「继续」 | Basic UI tap | Resume |
| Panel | Secondary Button「设置」「关于」 | 卫星菜单（参照 main-menu） | OpenUIForm |
| Panel | Destructive/Text Secondary「返回主菜单」 | — | 打开 Quit Confirmation |
| Dialog | 「确认退出本场？」+ Cancel / Confirm | Modal blocker priority **≥250** 精神 | Confirm → Route Title |

### ASCII Wireframe

```
        [半透明战场可见]

┌─────────────────────────────┐
│       Paused（可选标签）       │
│  ┌───────────────────────┐   │
│  │      继续 / Resume      │   │
│  └───────────────────────┘   │
│  ┌───────────────────────┐   │
│  │       设置             │   │
│  └───────────────────────┘   │
│  ┌───────────────────────┐   │
│  │       关于             │   │
│  └───────────────────────┘   │
│  ┌───────────────────────┐   │
│  │    返回主菜单           │   │
│  └───────────────────────┘   │
└─────────────────────────────┘

Quit Confirm（第二层）:
┌─────────────────────────────┐
│ 确认退出本场并返回主菜单？      │
│   [取消]      [确认]          │
└─────────────────────────────┘
```

---

## States & Variants

| State / Variant | Trigger | UI / System Behavior |
|-----------------|---------|----------------------|
| **Hidden** | 战斗未暂停 | 无 Pause Overlay |
| **Paused — Overlay Visible** | 暂停入口 | `Suspended`：**CombatClock 停止推进**；**战斗 hit-area 注销或禁用**（HUD AC）；**只允许 Pause/modal UI Button / Dialog** |
| **Paused — Satellite Open** | 打开设置/关于 | Pause Overlay **保留或半透明占位**（以实现为准）；返回卫星 UI 后仍Suspensed |
| **Quit Confirm Visible** | 点返回主菜单 | Modal **阻断** Pause Panel 重复触发退出 |
| **Resuming** | Resume Tap → Overlay 关闭中 | 极短（≤150ms）淡出可选；完成后触发命中区域重建 |
| **宿主 Suspended — Background** | `onHide`/宿主暂停 | **仅系统语义**：立刻 `Suspended`；后台期间 **不强行叠第二层 Overlay**（宿主可能不绘制）；禁止在此时新建第二战斗上下文 |
| **宿主 Suspended — Foreground** | `onShow`/回到前台 | 若战斗仍有效且仍为 Suspended：**单例** Pause Overlay **必须在首帧可交互前可见**（与 HUD 按钮打开的 Overlay **同一 prefab/同一实例策略**）；若 Overlay 已在后台前打开则 **复用同一实例**，禁止重复 `OpenUIForm` 造成双层遮挡 |

---

## Interaction Map

**映射基数**：Touch Full；与战斗 Timing(PointerDown 弹反)**分离**：暂停菜单只用 **`Button`/对话框**，不走战斗 hit-area。

| 控件 | Tap | 反馈 | 结果 |
|------|-----|------|------|
| 继续 | Tap | 标准按下态 | `ResumeBattleUi()`（语义）；Overlay 关闭；hit-area 按 HUD AC **重建** |
| 设置/关于 | Tap | 按下态 | Open satellite UI |
| 返回主菜单 | Tap | 按下态 | 打开 Quit Confirm |
| Quit — 取消 | Tap | — | 关闭对话框 |
| Quit — 确认 | Tap | — | **Discard battle session context** → 路由 Main Menu |
| （不推荐 MVP）Backdrop Tap | — | — | **不映射**：除非日后 UX review 批准 |

---

## Events Fired

| Player Action | Event | Payload |
|---------------|-------|---------|
| 打开暂停 | `ui_pause_open` | `battle_context_id`, `reason`: hud_button \| host_pause |
| 恢复 | `ui_pause_resume` | `battle_context_id` |
| 打开卫星 | `ui_pause_open_satellite` | `target`: settings \| about |
| 请求退出（第一层） | `ui_pause_quit_prompt_open` | — |
| 退出取消 | `ui_pause_quit_cancel` | — |
| 退出确认 | `ui_pause_quit_confirm` | `battle_context_id` |

---

## Transitions & Animations

- Overlay **淡入 ≤160–200ms**（可与 `hud_fade_ms` 对齐量级）；**low-motion** 下允许Opacity跳变无缩放弹跳。
- Dialog **instant center fade** ≤150ms。
- Resume：**淡出 Overlay**，紧接着 gameplay-side **hit-area registration flush**（非 UX spec 细节但必须 QA）。

---

## Data Requirements

| Data | Source | R/W | 备注 |
|------|--------|-----|------|
| `Suspended` / Pause Flag | 回合管理器 / BattleFacade（以实现为准） | R/W（UI 触发 API） | UI **不得伪造**：必须由 gameplay 侧 ACK |
| `battle_context_id` | Battle Session | Read（analytics/debug） | 事件载荷 |

---

## Accessibility

- Basic：`Pause Panel` **Primary ≥88dp**；对话框按钮同理。
- 语义：**Destructive**（退出）与 Primary（继续）**不只靠色相**，可加字重/边框区分。
- 文本：**可读字号**，深色半透明 backdrop + **亮色卡片对比度足够**。
- **宿主 Pause**：音频 Duck — Audio Designer optional。

---

## Localization Considerations

| 元件 | CN MVP | +40% 预留 |
|------|--------|-----------|
| 「继续」 | ≤6 字 | ≤10 |
| 「返回主菜单」 | ≤10 | ≤14 |
| 对话框标题 / 正文 | 单行标题 ≤22 | 正文自动折行 |
| 「取消」「确认」 | ≤4 | ≤8 |

---

## Acceptance Criteria

- [ ] 战斗中开启 Pause：**CombatClockMs 不推进**直至 Resume（可由 QA harness / Debug overlay 观测）。
- [ ] Pause 可见期间：**任意战斗 Parry/Counter hit-area 不生效**，指向日志 `ignoredReason=Suspended` 或可观测等价断言。
- [ ] Tap Resume：**≤150–250ms** 内 Overlay 消失且 Phase/HUD **恢复到暂停前合法交互状态**，无需重启整场战斗。
- [ ] 「返回主菜单」：**必须先弹出对话框**，Cancel **不回菜单**，Confirm **丢上下文并入 main-menu spec Entry（丢弃战斗中未完成指令）**。
- [ ] 打开 SettingForm：**不改变 Suspended** — Confirm Resume 前 gameplay clock **仍未擅自走动**。
- [ ] Safe-area：**Pause Panel / Dialog** 不被刘海遮挡 Primary Buttons。
- [ ] **宿主 Pause**：`onHide` 起进入 `Suspended`；`onShow` 后 **≤ 首帧交互窗口** 内玩家可见 **唯一** Pause Overlay（与手动暂停 UI 一致），且 `ui_pause_open` 可观测 `reason=host_pause`（无双重 Overlay、无重复时钟停机调用）。

---

## Open Questions

| # | 话题 | Owner |
|---|------|--------|
| ~~1~~ | ~~宿主后台 Pause Overlay~~ — **Resolved**：后台 **Suspended 同源**；回前台 **强制可见单例 Overlay**（见 States / Acceptance） | — |
| 2 | 探索场景 Pause：**延后 / 共用 PauseForm** 拆分章节时机 | Game Designer |
| 3 | 音效：**Pause instant Duck vs Fade** | Audio |
