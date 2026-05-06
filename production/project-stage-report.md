# Project Stage Analysis Report

**Generated**: 2026-05-06  
**Stage**: Pre-Production  
**Analysis Scope**: Full project（CCGS `/project-stage-detect` 扫描；Unity 仓库路径已按 `client/`、`framework/`、`gameplay/` 适配，不等同于模板中的通用 `src/`）

---

## Executive Summary

Echo of Blades（BSKGame）在 **设计文档、架构决策（ADR）、Epic/Story 与 Sprint 规划** 上已达到较高的 Pre-Production 成熟度；**玩法与框架代码体量可观**，且 **Unity EditMode 测试覆盖面较广**。与此同时，`production/stage.txt` 明确为 **Pre-Production**，与 **`production/gate-checks/production-gate-2026-05-01.md`（Verdict: CONCERNS）** 一致：**Vertical Slice 竖切构建、正式 playtest 证据与部分 UX 规格仍为缺口**，尚未形成门禁意义上的「完整可玩竖切 + 多轮试玩闭环」。

**优先级建议**：先交付 **启动 → 战斗 → 结算** 的竖切可构建版本，并行补齐 **主菜单 / 暂停菜单 UX 规格**，完成 **≥3 次试玩与结构化报告**，再运行 **`/gate-check production`** 争取将 CONCERNS 升为 PASS，并按门禁 skill 约定更新阶段文件。

**Current Focus**: 竖切与试玩验证（对齐门禁 Recommended Next Steps）；工程侧可参考 `production/sprints/sprint-000-foundation.md` 与各 Epic Story 的持续实现。  
**Blocking Issues**: 非单一技术阻塞；主要为 **门禁缺失项（VS、playtest、部分 UX）** 阻碍 **Pre-Production → Production** 的形式化通过。  
**Estimated Time to Next Stage**: 视团队带宽而定；门禁曾对 VS 给出约 **1–2 天量级** 的人工估计（需结合实际 Scene/UI/打包范围复核）。

---

## Completeness Overview

### Design Documentation

- **Status**: ~85%（MVP 系统 GDD 齐备；叙事/关卡与部分规格仍弱）
- **Files Found**（抽样规则：`design/gdd/`）
  - **GDD（根目录 `.md`）**: **12** 个（含 `game-concept.md`、`systems-index.md` 与各系统 GDD）
  - **GDD 评审日志**: `design/gdd/reviews/` 约 **10** 个 `*.md`
  - **Narrative docs**: **0**（`design/narrative/` 无文件）
  - **Level designs**: **0**（`design/levels/` 无文件）
- **Key Gaps**:
  - [ ] **`design/character-profiles/`**：门禁记录为缺失（角色视觉档案）
  - [ ] **独立 `design/gdd/game-pillars.md`**：未发现；支柱内容可能在 `game-concept.md` 内——需产品侧确认是否仍需拆文件

### Source Code

- **Status**: ~高（不宜用单一百分比概括「玩法完成度」）
- **Major locations**（本仓库非通用 `src/` 布局）:
  - ✅ **Unity 客户端业务与启动** — `client/Assets/Game/**/*.cs` — **144** 个文件（近似）
  - ✅ **game.core（框架核心）** — `framework/game.core/**/*.cs` — **402** 个文件（近似）
  - ✅ **game.gameplay（玩法库）** — `gameplay/gameplay/**/*.cs` — **116** 个文件（近似）
- **Major Systems Identified**（按目录与文档交叉推断，非穷尽）:
  - ✅ 启动 / Procedure / HybridCLR 边界（见 `bsk-launch` skill 与 ADR-0003）
  - ✅ 战斗编排、事件总线、回合/时钟、伤害、弹反、敌人模式、UI/HUD 相关逻辑与测试
  - ⚠️ **呈现层与内容管线**：竖切所需的「菜单 → 战斗 → 结算」产品外壳与资产挂钩仍需对齐门禁
- **Key Gaps**:
  - [ ] **对外可演示的 Vertical Slice 构建产物路径与验收标准**（与门禁一致）
  - [ ] **战斗反馈与角色表现的工业化打磨**（Sprint 005 文档亦列出后续项）

### Architecture Documentation

- **Status**: ~高
- **ADRs Found**: **13** — `docs/architecture/adr-0001` … `adr-0013`（门禁曾记录为 Accepted）
- **Coverage**（摘要）:
  - ✅ 场景生命周期 / 上下文路由（ADR-0001）
  - ✅ 战斗事件总线与 DTO 版本策略（ADR-0002）
  - ✅ Gameplay / Unity / HybridCLR 分层（ADR-0003）
  - ✅ 战斗时钟与确定性顺序（ADR-0004）
  - ✅ 伤害与 HP 权责（ADR-0005）
  - ✅ 移动端触摸时间戳与命中区域策略（ADR-0006）
  - ✅ 弹反时间轴与 counter handoff（ADR-0007）
  - ✅ 敌方攻击模式数据与调度权责（ADR-0008）
  - ✅ 战斗反馈质量 / HitStop 等（ADR-0009）
  - ✅ UIFrame 战斗 HUD 组合（ADR-0010）
  - ✅ 角色 schema / snapshot 权责（ADR-0011）
  - ✅ Action 服务边界（ADR-0012）
  - ✅ 确定性战斗测试策略（ADR-0013）
- **Key Gaps**:
  - [ ] 若竖切引入新的跨边界契约（例如存档、账号、网络），可能需要 **新增 ADR** ——待竖切实装后做一次增量架构审计

### Production Management

- **Status**: ~中高
- **Found**:
  - **Explicit stage**: `production/stage.txt` → **Pre-Production**
  - **Sprint plans**: **6** 个于 `production/sprints/`（sprint-000 … sprint-005）
  - **Epics / Stories**: `production/epics/`（索引见 `production/epics/index.md`）
  - **Gate checks**: `production/gate-checks/`（含 `production-gate-2026-05-01.md`）
  - **Milestones**: `production/milestones/` **未发现里程碑文件**（目录可能为空或未使用）
  - **Playtests**: `production/playtests/` **当前为空**（与门禁一致）
- **Key Gaps**:
  - [ ] **结构化 playtest 报告归档位置与命名约定**
  - [ ] （可选）**里程碑文件**：若团队依赖门禁/sprint 即可，可明确声明「不使用 milestones 目录」

### Testing

- **Status**: 自动化基础 **强**（EditMode）；人工竖切/趣味验证仍缺证据
- **Test Files**:
  - `tests/`（仓库根）：约 **8** 个文件（README、证据与少量脚本）
  - **`client/Assets/Tests/EditMode/`**：**32** 个 `*.cs`（主力）
- **Coverage by System**（估计）:
  - Architecture / Scene / Events / Damage / Parry / Turn / Integration（`BattleOrchestratorTests` 等）：**覆盖面广**
  - **端到端玩家竖切**（菜单、构建包、真机）：**应以 playtest + 构建清单验证**，不单靠 EditMode
- **Key Gaps**:
  - [ ] **Playtest 与 VS 的人类可重复验收路径**（门禁明示）

### Prototypes

- **仓库根 `prototypes/`**：**1** 个文件 — `prototypes/parry-combat/REPORT.md`
- **Unity 内原型**：`client/Assets/Prototypes/ParryCombat/` — **有 README**（手感与 WebGL/微信试跑指引）
- **Key Gaps**:
  - [ ] 若竖切 **复用 ParryCombat**：建议在 `prototypes/` 或 `production/playtests/` 侧 **补一行索引链接**，避免「文档分裂在两棵树」

---

## Stage Classification Rationale

**Why Pre-Production?**

1. **`production/stage.txt` 显式写入 Pre-Production** — 按 `/gate-check` 协作约定，**优先于**「代码量级启发式」覆盖推断结果。  
2. **最新 Production 门禁为 CONCERNS**：文档与架构齐备，但 **VS / playtest** 等 Pre-Production 典型产出仍未闭环，**不符合「已进入 Production」的形式定义**。  
3. **代码深度已超过「仅有原型」**：这与「阶段标签」不矛盾——阶段由 **流程门禁 + stage.txt** 主导，而非仅由 LOC 决定。

**Indicators for this stage**:

- Engine / 技术栈已在 `CLAUDE.md` 固化（Unity 2022.3 + HybridCLR + 微信 WASM 路径）
- MVP GDD、ADR、Epic/Story、sprint 文档齐全
- 集成层与可交互战斗循环在 sprint 文档中有明确描述；竖切与试玩证据仍待补齐

**Next stage requirements**（对齐 `production-gate-2026-05-01.md`）:

- [ ] **Vertical Slice 构建**：完整 **启动 → 战斗 → 结算**
- [ ] **≥3 次 playtest** + **结构化报告**（skill：`/playtest-report`）
- [ ] **UX Spec**：**main-menu** + **pause-menu**（skill：`/ux-design`）
- [ ] **复检**：`/gate-check production` → 目标 **PASS**（或消除关键 CONCERNS）

---

## Gaps Identified (with Clarifying Questions)

### Critical Gaps（阻碍门禁通过）

1. **Vertical Slice 未按门禁定义为「可交付竖切」**
   - **Impact**: `production-gate-2026-05-01.md` 将其列为关键缺口；VS Validation 多项未满足。
   - **Question**: 竖切验收是否 **必须以微信真机构建** 为准，抑或 **Unity Player/WebGL 内部包** 即可作为第一轮？
   - **Suggested Action**: 明确构建入口 Scene、菜单占位策略与结算界面；产出构建编号或路径说明。

2. **Playtest 证据缺失**
   - **Impact**: 「核心循环是否好玩」停留在 MANUAL CHECK，无法闭环门禁。
   - **Question**: 试玩人员是否 **必须包含非工程角色**，还是首轮允许工程 + QA？
   - **Suggested Action**: 至少 **3 次**会话；独立走完核心循环（无开发者口述）；用 `/playtest-report` 归档。

### Important Gaps（质量与协作）

3. **主菜单 / 暂停菜单 UX 规格缺失**
   - **Impact**: UX coverage 检查为 PARTIAL；不利于 UI 实现对齐。
   - **Question**: 竖切菜单是否允许 **极简占位**（单一按钮开始战斗）？
   - **Suggested Action**: `/ux-design main-menu`、`/ux-design pause-menu`。

4. **角色视觉档案缺失**
   - **Impact**: 门禁「artifacts」清单缺失；对外演示一致性弱。
   - **Question**: 是否接受竖切阶段 **纯灰盒**，档案与 Production 并行？
   - **Suggested Action**: 至少主角 + 1 敌人的一页式档案，或明确写入门禁豁免理由（若团队同意）。

### Nice-to-Have（规范与实践）

5. **叙事与关卡文档为空**
   - **Impact**: 长线内容管线不清晰；对当前竖切可能影响有限。
   - **Question**: MVP 是否刻意延后叙事/关卡文档？
   - **Suggested Action**: 若延后，在 `systems-index.md` 或 sprint 中 **明示「Phase 2」**。

6. **`production/milestones/` 未使用**
   - **Impact**: 依赖 sprint/gate 管理则无妨。
   - **Question**: 里程碑是否在别处（Notion/Jira）跟踪？
   - **Suggested Action**: 在本报告中注明单一真相来源（SSOT）。

---

## Recommended Next Steps

### Immediate Priority（先做）

1. **交付 Vertical Slice 可玩构建** — 解锁门禁最核心的「可验证」部分  
   - Suggested：工程任务为主；文档侧在 `production/playtests/` 或 `tests/evidence/` 增补 **构建与入口说明**  
   - Estimated effort: **M–L**（取决于微信导出/UI 范围）

2. **归档 ≥3 次 Playtest**  
   - Suggested skill: **`/playtest-report`**  
   - Estimated effort: **M**

3. **补齐 UX 规格**  
   - Suggested skill: **`/ux-design main-menu`**、**`/ux-design pause-menu`**  
   - Estimated effort: **S–M**

### Short-Term（本周 / 当前 Sprint 跨度）

4. **复检门禁**：**`/gate-check production`**（对比 `production-gate-2026-05-01.md` 缺口清单）  
5. **角色视觉档案**：最小集合或正式豁免记录  

### Medium-Term（下一里程碑）

6. **叙事 / 关卡文档**：若 MVP 需要，启动 **`/map-systems`** 后续拆解或与 **`/design-system`** 协同  
7. **`/milestone-review`**：若对外承诺日期，补充里程碑 SSOT  

---

## Role-Specific Recommendations

*本次请求未指定 `/project-stage-detect [role]`，以下为不分角色的共性建议。*

### Producer（制作人）

- **Focus**: 竖切范围冻结、试玩招募与门禁时间表、`production/stage.txt` 仅在 **`/gate-check`** 通过后更新  
- **Next tasks**: 定义 VS Demo 验收录像标准；推动 3 次 playtest；召集 UX 补文档  

### Programmer（程序）

- **Focus**: VS Scene 流程、`BattleFlowController` / `TempGameState` 与 UI 的串联、构建脚本与微信导出路径  
- **Next tasks**: 竖切构建清单；修复 playtest 阻塞 Bug；保持 EditMode 测试绿  

### Designer（策划 / UX）

- **Focus**: main-menu / pause-menu spec；竖切内玩家目标是否在 2 分钟内可读  
- **Next tasks**: `/ux-design`；参与 playtest 提纲；反馈 core loop  

---

## Follow-Up Skills to Run

- **`/gate-check production`** — 竖切与试玩完成后复检  
- **`/ux-design main-menu`**、**`/ux-design pause-menu`** — 补齐门禁 UX 缺口  
- **`/playtest-report`** — 结构化试玩归档  
- **`/project-stage-detect`** — 重大交付后复扫（更新本报告）  
- **`/milestone-review`** — 若有对外节点  
- **`/onboard [role]`** — 新成员入场  

---

## Appendix: File Counts by Directory（Approximate, 2026-05-06）

```
design/gdd/           12 *.md（根目录系统/概念/索引） + reviews/ ~10 *.md
design/narrative/     0
design/levels/        0

client/Assets/Game/   ~144 *.cs
framework/game.core ~402 *.cs
gameplay/gameplay   ~116 *.cs

docs/architecture/    13 ADR *.md

production/stage.txt           Pre-Production
production/sprints/            6 *.md
production/gate-checks/          等多份门禁
production/playtests/          0（待填充）
production/milestones/         0（未见文件）

tests/                          ~8 files（含 README / evidence）
client/Assets/Tests/EditMode/   32 *.cs

prototypes/                     1 file（parry-combat/REPORT.md）
client/Assets/Prototypes/ParryCombat/  README + 原型脚本（未在本附录逐项计数）
```

---

**End of Report**

*Generated by `/project-stage-detect`（ccgs-project-stage-detect）skill — user approved write to `production/project-stage-report.md`.*
