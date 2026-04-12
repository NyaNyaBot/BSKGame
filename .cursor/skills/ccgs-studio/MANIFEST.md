# CCGS Cursor 适配版 — 卸载清单

> **版本**: v1.0.0-beta (来自 [Donchitos/Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios))
> **安装日期**: 2026-04-12
> **安装方式**: Cursor Agent 手动迁移

## 完整卸载步骤

执行以下命令即可完全移除 CCGS 所有文件，不会影响 BSK 原有内容：

```bash
# 1. 删除所有 ccgs- 前缀的 skill 目录（含 ccgs-studio 自身）
rm -rf .cursor/skills/ccgs-*/

# 2. 删除所有 ccgs- 前缀的规则文件
rm -f .cursor/rules/ccgs-*.md

# 3. 删除所有 ccgs- 前缀的 hook 脚本
rm -f .cursor/hooks/ccgs-*.sh

# 4. 删除 hooks.json（或手动移除其中的 ccgs 条目）
rm -f .cursor/hooks.json
```

> **注意**: 如果你后来在 `hooks.json` 中添加了自己的 hooks，请勿直接删除该文件，而是手动编辑移除 ccgs 相关条目。

## 安装内容清单

### Skills（72 个目录）

所有 skill 目录位于 `.cursor/skills/`，统一使用 `ccgs-` 前缀：

```
ccgs-adopt/             ccgs-architecture-decision/  ccgs-architecture-review/
ccgs-art-bible/         ccgs-asset-audit/            ccgs-asset-spec/
ccgs-balance-check/     ccgs-brainstorm/             ccgs-bug-report/
ccgs-bug-triage/        ccgs-changelog/              ccgs-code-review/
ccgs-consistency-check/ ccgs-content-audit/          ccgs-create-architecture/
ccgs-create-control-manifest/ ccgs-create-epics/     ccgs-create-stories/
ccgs-day-one-patch/     ccgs-design-review/          ccgs-design-system/
ccgs-dev-story/         ccgs-estimate/               ccgs-gate-check/
ccgs-help/              ccgs-hotfix/                 ccgs-launch-checklist/
ccgs-localize/          ccgs-map-systems/            ccgs-milestone-review/
ccgs-onboard/           ccgs-patch-notes/            ccgs-perf-profile/
ccgs-playtest-report/   ccgs-project-stage-detect/   ccgs-propagate-design-change/
ccgs-prototype/         ccgs-qa-plan/                ccgs-quick-design/
ccgs-regression-suite/  ccgs-release-checklist/      ccgs-retrospective/
ccgs-reverse-document/  ccgs-review-all-gdds/        ccgs-scope-check/
ccgs-security-audit/    ccgs-setup-engine/           ccgs-skill-improve/
ccgs-skill-test/        ccgs-smoke-check/            ccgs-soak-test/
ccgs-sprint-plan/       ccgs-sprint-status/          ccgs-start/
ccgs-story-done/        ccgs-story-readiness/        ccgs-team-audio/
ccgs-team-combat/       ccgs-team-level/             ccgs-team-live-ops/
ccgs-team-narrative/    ccgs-team-polish/            ccgs-team-qa/
ccgs-team-release/      ccgs-team-ui/                ccgs-tech-debt/
ccgs-test-evidence-review/ ccgs-test-flakiness/      ccgs-test-helpers/
ccgs-test-setup/        ccgs-ux-design/              ccgs-ux-review/
ccgs-studio/  (本文件所在目录，包含 references/ 子目录)
```

### 参考文档（ccgs-studio/references/ 下）

- `agents/` — 39 个 Agent 角色定义（已过滤 Godot/Unreal）
- `templates/` — 38 套文档模板
- `docs/` — 23 个工作流/协调/编码标准文档

### Rules（11 个文件）

位于 `.cursor/rules/`：

```
ccgs-ai-code.md          ccgs-data-files.md       ccgs-design-docs.md
ccgs-engine-code.md       ccgs-gameplay-code.md    ccgs-narrative.md
ccgs-network-code.md      ccgs-prototype-code.md   ccgs-shader-code.md
ccgs-test-standards.md    ccgs-ui-code.md
```

## 不受影响的内容

以下 BSK 原有内容完全不受 CCGS 安装影响：

- `.agents/skills/bsk-*` — BSK 项目 domain skills
- `.cursor/rules/bsk-global.md` — BSK 全局规则
- `.cursor/skills/skill-creator/` — BSK skill 创建工具
- `AGENTS.md` — BSK 项目入口（未修改）

### Hooks（10 个脚本 + hooks.json）

位于 `.cursor/hooks/`，注册于 `.cursor/hooks.json`：

| 脚本 | Cursor 事件 | 用途 |
|------|------------|------|
| `ccgs-session-start.sh` | `sessionStart` | 显示分支、最近提交、Sprint 信息 |
| `ccgs-detect-gaps.sh` | `sessionStart` | 检测代码与文档的缺口 |
| `ccgs-validate-commit.sh` | `beforeShellExecution` | 提交前检查 GDD 节、JSON 有效性、硬编码值 |
| `ccgs-validate-push.sh` | `beforeShellExecution` | 推送到保护分支时提醒 |
| `ccgs-validate-assets.sh` | `afterFileEdit` | 资产文件命名规范、JSON 有效性 |
| `ccgs-validate-skill-change.sh` | `afterFileEdit` | 修改 ccgs skill 后提醒验证 |
| `ccgs-pre-compact.sh` | `preCompact` | 压缩前保存会话状态 |
| `ccgs-session-stop.sh` | `stop` | 结束时归档会话日志 |
| `ccgs-log-agent.sh` | `subagentStart` | 子任务启动审计日志 |
| `ccgs-log-agent-stop.sh` | `subagentStop` | 子任务完成审计日志 |

**跳过的 Hooks（2个）**:
- `notify.sh` — Windows PowerShell 专属通知，macOS 不适用
- `post-compact.sh` — Cursor 无 `postCompact` 事件

## 适配说明

- 所有 skill 中的 `.claude/` 路径已替换为 `.cursor/skills/ccgs-studio/references/` 路径
- `AskUserQuestion` 已替换为 `AskQuestion`
- 12 个 Hook 中 10 个已适配 Cursor hooks 格式（`hooks.json` + 脚本）
- Godot/Unreal 引擎专属 Agent 未安装（BSK 为 Unity 项目）
- Rules 中的路径模式保持 CCGS 原始值（`src/**`），如需在 BSK 中生效需手动适配
