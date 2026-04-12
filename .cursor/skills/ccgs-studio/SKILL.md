# CCGS — Claude Code Game Studios (Cursor 适配版)

> **来源**: [Donchitos/Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) v1.0.0-beta
> **适配日期**: 2026-04-12
> **卸载指南**: `.cursor/skills/ccgs-studio/MANIFEST.md`

本 skill 是 CCGS 在 Cursor 环境中的总导航入口。CCGS 提供 72 个游戏开发工作流 skill、39 个 Agent 角色定义、39 套文档模板和 11 条编码规范，覆盖从概念构思到发布上线的完整游戏开发流水线。

**与 BSK 项目的关系**: CCGS 提供「通用游戏制片方法论」，BSK 的 `bsk-*` skills 提供「项目特定的引擎/仓库知识」。两者互补而非替代。涉及 BSK 仓库结构、Unity 配置、HybridCLR 热更等话题时应优先参考 `bsk-*` skills。

---

## Cursor 兼容性说明

CCGS 原为 Claude Code 设计，迁移到 Cursor 时有以下差异需注意：

### 工具名称映射

| CCGS 原始 | Cursor 等价 | 说明 |
|-----------|------------|------|
| `AskQuestion` | `AskQuestion` | 已批量替换 |
| `Bash` | `Shell` | Cursor 中用 Shell 执行命令 |
| `Edit` | `StrReplace` | Cursor 的精确替换工具 |
| `Write` | `Write` | 相同 |
| `Read` | `Read` | 相同 |
| `Glob` | `Glob` | 相同 |
| `Grep` | `Grep` | 相同 |
| `Task` | `Task` | 见下方 Agent 使用说明 |
| `TodoWrite` | `TodoWrite` | 相同 |

### Agent 子任务调度

CCGS 的 skill 中会通过 `Task` 工具 spawn 具名 agent（如 `creative-director`、`systems-designer`）。在 Cursor 中：

- `Task` 的 `subagent_type` 只能是 `generalPurpose`、`explore`、`shell`、`best-of-n-runner`
- **替代方案**: 使用 `subagent_type: generalPurpose`，在 `prompt` 中引入对应 agent 的角色定义
- Agent 定义文件位于 `.cursor/skills/ccgs-studio/references/agents/[name].md`
- 示例: 当 skill 指示 "spawn `creative-director`"，实际操作为:
  ```
  Task(subagent_type=generalPurpose, prompt="按照 .cursor/skills/ccgs-studio/references/agents/creative-director.md 中的角色定义行事。任务: [具体任务描述]")
  ```

### Hooks（已适配 10/12 个）

CCGS 的 12 个 Hook 中有 10 个已迁移到 Cursor hooks 格式，注册在 `.cursor/hooks.json` 中：

| 脚本 | 事件 | 功能 |
|------|------|------|
| `ccgs-validate-commit.sh` | `beforeShellExecution` | 提交前检查 GDD 完整性、JSON 有效性、硬编码值 |
| `ccgs-validate-push.sh` | `beforeShellExecution` | 推送到保护分支（main/master/develop）时弹出确认 |
| `ccgs-validate-assets.sh` | `afterFileEdit` | 资产文件命名规范和 JSON 格式检查 |
| `ccgs-session-start.sh` | `sessionStart` | 显示当前分支、最近提交、Sprint 和会话恢复信息 |
| `ccgs-detect-gaps.sh` | `sessionStart` | 检测代码规模与文档缺口 |
| `ccgs-pre-compact.sh` | `preCompact` | 上下文压缩前保存会话状态快照 |
| `ccgs-session-stop.sh` | `stop` | 会话结束时归档日志到 `production/session-logs/` |
| `ccgs-log-agent.sh` | `subagentStart` | 子任务启动审计记录 |
| `ccgs-log-agent-stop.sh` | `subagentStop` | 子任务完成审计记录 |
| `ccgs-validate-skill-change.sh` | `afterFileEdit` | 修改 ccgs skill 文件后提醒验证 |

**未迁移**: `notify.sh`（Windows 专属）、`post-compact.sh`（Cursor 无对应事件）。

### Review Mode

CCGS 支持三种评审强度（`full` / `lean` / `solo`），通过 `production/review-mode.txt` 控制。在 Cursor 中可以继续使用此约定，也可以在对话中直接指定评审级别。

---

## 工作流阶段索引

CCGS 将游戏开发分为 7 个阶段，每个阶段有对应的 skill：

### Phase 1: 概念 (Concept)
| Skill | 用途 |
|-------|------|
| `ccgs-brainstorm` | 游戏创意探索（MDA 框架、玩家心理学） |
| `ccgs-setup-engine` | 引擎配置（BSK 已配置 Unity，可跳过） |
| `ccgs-art-bible` | 视觉风格圣经 |
| `ccgs-map-systems` | 系统分解与依赖排序 |

### Phase 2: 系统设计 (Systems Design)
| Skill | 用途 |
|-------|------|
| `ccgs-design-system` | 逐节编写单个系统 GDD |
| `ccgs-quick-design` | 小改动的轻量设计规格 |
| `ccgs-design-review` | GDD 完整性与一致性审查 |
| `ccgs-review-all-gdds` | 跨 GDD 一致性检查 |
| `ccgs-consistency-check` | 实体/公式/数值矛盾扫描 |
| `ccgs-propagate-design-change` | GDD 修改后的影响传播 |

### Phase 3: 技术架构 (Technical Setup)
| Skill | 用途 |
|-------|------|
| `ccgs-create-architecture` | 整体架构文档 |
| `ccgs-architecture-decision` | ADR（架构决策记录） |
| `ccgs-architecture-review` | 架构完整性审查 |
| `ccgs-create-control-manifest` | 程序员规则表 |

### Phase 4: 预生产 (Pre-Production)
| Skill | 用途 |
|-------|------|
| `ccgs-ux-design` | UX 规格（屏幕/流程/HUD） |
| `ccgs-ux-review` | UX 审查 |
| `ccgs-asset-spec` | 资产规格与 AI 生成提示 |
| `ccgs-prototype` | 快速原型验证 |
| `ccgs-create-epics` | GDD → Epic 分解 |
| `ccgs-create-stories` | Epic → Story 分解 |
| `ccgs-sprint-plan` | Sprint 规划 |
| `ccgs-test-setup` | 测试框架搭建 |

### Phase 5: 生产 (Production)
| Skill | 用途 |
|-------|------|
| `ccgs-dev-story` | 实现 Story |
| `ccgs-story-readiness` | Story 就绪检查 |
| `ccgs-story-done` | Story 完成验收 |
| `ccgs-code-review` | 代码审查 |
| `ccgs-sprint-status` | Sprint 进度快照 |
| `ccgs-bug-report` | Bug 报告 |
| `ccgs-bug-triage` | Bug 分级 |
| `ccgs-estimate` | 任务估算 |
| `ccgs-scope-check` | 范围蔓延检测 |
| `ccgs-retrospective` | Sprint 回顾 |

### Phase 6: 打磨 (Polish)
| Skill | 用途 |
|-------|------|
| `ccgs-perf-profile` | 性能分析 |
| `ccgs-balance-check` | 数值平衡检查 |
| `ccgs-asset-audit` | 资产规范审计 |
| `ccgs-playtest-report` | 游戏测试报告 |
| `ccgs-soak-test` | 持久测试协议 |

### Phase 7: 发布 (Release)
| Skill | 用途 |
|-------|------|
| `ccgs-release-checklist` | 发布前检查 |
| `ccgs-launch-checklist` | 上线最终门禁 |
| `ccgs-changelog` | 变更日志 |
| `ccgs-patch-notes` | 玩家版更新说明 |
| `ccgs-hotfix` | 紧急修复流程 |
| `ccgs-day-one-patch` | 首日补丁 |

### 团队协作 (Team Orchestration)
| Skill | 用途 |
|-------|------|
| `ccgs-team-combat` | 战斗系统多 Agent 协作 |
| `ccgs-team-narrative` | 叙事多 Agent 协作 |
| `ccgs-team-ui` | UI 多 Agent 协作 |
| `ccgs-team-audio` | 音频多 Agent 协作 |
| `ccgs-team-level` | 关卡多 Agent 协作 |
| `ccgs-team-polish` | 打磨多 Agent 协作 |
| `ccgs-team-release` | 发布多 Agent 协作 |
| `ccgs-team-qa` | QA 多 Agent 协作 |
| `ccgs-team-live-ops` | 运营多 Agent 协作 |

### QA 与测试
| Skill | 用途 |
|-------|------|
| `ccgs-qa-plan` | QA 测试计划 |
| `ccgs-smoke-check` | 冒烟测试 |
| `ccgs-regression-suite` | 回归测试套件 |
| `ccgs-test-helpers` | 测试辅助工具生成 |
| `ccgs-test-evidence-review` | 测试证据审查 |
| `ccgs-test-flakiness` | 不稳定测试检测 |

### 其他
| Skill | 用途 |
|-------|------|
| `ccgs-start` | 引导式首次入门 |
| `ccgs-help` | 当前阶段建议 |
| `ccgs-project-stage-detect` | 项目阶段检测 |
| `ccgs-adopt` | 已有项目接入 CCGS |
| `ccgs-onboard` | 新人入职文档 |
| `ccgs-localize` | 本地化流水线 |
| `ccgs-reverse-document` | 从代码反推设计文档 |
| `ccgs-content-audit` | 内容实现审计 |
| `ccgs-security-audit` | 安全审计 |
| `ccgs-tech-debt` | 技术债管理 |
| `ccgs-gate-check` | 阶段门禁检查 |
| `ccgs-milestone-review` | 里程碑审查 |
| `ccgs-skill-test` | Skill 质量测试 |
| `ccgs-skill-improve` | Skill 改进 |

---

## Agent 角色一览

共 39 个 Agent 定义（已过滤 Godot/Unreal 专属角色），位于 `references/agents/`：

### Tier 1 — 总监 (Directors)
- `creative-director` — 创意总监，最高创意权威
- `technical-director` — 技术总监，最高技术权威
- `producer` — 制作人，协调跨部门

### Tier 2 — 部门主管 (Leads)
- `game-designer` / `lead-programmer` / `art-director` / `audio-director`
- `narrative-director` / `qa-lead` / `release-manager` / `localization-lead`

### Tier 3 — 专家 (Specialists)
- 程序: `gameplay-programmer`, `engine-programmer`, `ai-programmer`, `network-programmer`, `tools-programmer`, `ui-programmer`
- 设计: `systems-designer`, `level-designer`, `economy-designer`, `ux-designer`
- 美术/音频: `technical-artist`, `sound-designer`
- 叙事: `writer`, `world-builder`
- 其他: `prototyper`, `performance-analyst`, `devops-engineer`, `analytics-engineer`, `security-engineer`, `qa-tester`, `accessibility-specialist`, `live-ops-designer`, `community-manager`

### Unity 专家
- `unity-specialist` — Unity 总专家
- `unity-dots-specialist` / `unity-shader-specialist` / `unity-addressables-specialist` / `unity-ui-specialist`

---

## 编码规范 (Rules)

11 条路径作用域规则，位于 `.cursor/rules/ccgs-*.md`：

| 规则文件 | 作用范围 | 内容 |
|---------|---------|------|
| `ccgs-gameplay-code` | `src/gameplay/**` | 数据驱动、delta time、无 UI 引用 |
| `ccgs-engine-code` | `src/core/**` | 零分配热路径、线程安全 |
| `ccgs-ai-code` | `src/ai/**` | 性能预算、可调试性 |
| `ccgs-network-code` | `src/networking/**` | 服务器权威、版本化消息 |
| `ccgs-ui-code` | `src/ui/**` | 不持有游戏状态、本地化就绪 |
| `ccgs-shader-code` | `src/shaders/**` | Shader 编码规范 |
| `ccgs-design-docs` | `design/gdd/**` | GDD 8 必需节、公式格式 |
| `ccgs-test-standards` | `tests/**` | 测试命名、覆盖率 |
| `ccgs-prototype-code` | `prototypes/**` | 放松标准、需 README |
| `ccgs-narrative` | `design/narrative/**` | 叙事文档规范 |
| `ccgs-data-files` | `assets/data/**` | 数据文件规范 |

> **注意**: 这些规则的路径模式（如 `src/gameplay/**`）来自 CCGS 通用模板，不直接匹配 BSKGame 的目录结构。如需实际生效，需将 `paths` 改为 BSKGame 对应路径（如 `client/Assets/Game/Scripts/Gameplay/**`）。也可保持原样作为参考规范使用。

---

## 文档模板

39 套模板位于 `references/templates/`，涵盖：
- 游戏设计: `game-design-document.md`, `game-concept.md`, `game-pillars.md`, `systems-index.md`
- 架构: `architecture-decision-record.md`, `architecture-traceability.md`, `technical-design-document.md`
- 生产: `sprint-plan.md`, `milestone-definition.md`, `release-checklist-template.md`
- UX: `ux-spec.md`, `hud-design.md`, `interaction-pattern-library.md`, `accessibility-requirements.md`
- 叙事: `narrative-character-sheet.md`, `player-journey.md`, `faction-design.md`
- 其他: `art-bible.md`, `sound-bible.md`, `economy-model.md`, `test-plan.md` 等

---

## 快速开始

1. **已有 BSK 项目，想用 CCGS 管理新系统设计**: 读取 `ccgs-design-system` skill
2. **想做代码审查**: 读取 `ccgs-code-review` skill
3. **想规划 Sprint**: 读取 `ccgs-sprint-plan` skill
4. **想从头了解 CCGS 工作流**: 读取 `ccgs-start` skill
5. **想了解当前项目处于哪个阶段**: 读取 `ccgs-project-stage-detect` skill

详细工作流定义见 `references/docs/workflow-catalog.yaml`。

---

## 完整卸载

```bash
rm -rf .cursor/skills/ccgs-*/
rm -f .cursor/rules/ccgs-*.md
rm -f .cursor/hooks/ccgs-*.sh
rm -f .cursor/hooks.json  # 如果有自定义 hooks 则手动编辑移除 ccgs 条目
```

详细清单见 `MANIFEST.md`。
