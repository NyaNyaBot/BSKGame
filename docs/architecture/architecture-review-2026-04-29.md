# Architecture Review Report

> Date: 2026-04-29  
> Engine: Unity 2022.3.17f1  
> Mode: `/ccgs-architecture-review` full  
> Verdict: CONCERNS

## Loaded Inputs

- GDDs reviewed: 10 MVP system GDDs plus `game-concept.md` and `systems-index.md`
- ADRs reviewed: 13
- Architecture documents reviewed: `docs/architecture/architecture.md`
- Engine reference reviewed: `docs/engine-reference/unity/VERSION.md`
- Project standards reviewed: `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md`
- Missing optional inputs: `docs/engine-reference/unity/breaking-changes.md`, `docs/engine-reference/unity/deprecated-apis.md`, `docs/engine-reference/unity/modules/`, `docs/consistency-failures.md`

## Traceability Summary

| Metric | Count |
|---|---:|
| Total technical requirements | 38 |
| Covered by ADR | 38 |
| Partial ADR coverage | 0 |
| ADR gaps | 0 |

Full matrix: `docs/architecture/architecture-traceability.md`

## Coverage Gaps

None. No new MVP ADR is required for coverage.

## Cross-ADR Conflicts

No blocking cross-ADR conflicts were found.

Checked ownership boundaries:

- HP ownership is consistent: ADR-0005 makes `IDamageService` the only HP mutation path; ADR-0011 exposes character storage and controlled mutation boundaries without taking damage authority.
- Time authority is consistent: ADR-0004 gives the turn manager the only mutable `CombatClockMs`; ADR-0006, ADR-0007, and ADR-0009 only map, read, or request timing behavior.
- Input authority is consistent: ADR-0006 owns timestamp mapping, hit testing, priority, occlusion, and duplicate filtering; ADR-0010 owns UI layout and registration lifecycle only.
- Presentation boundaries are consistent: ADR-0009 and ADR-0010 make feedback/UI event consumers rather than gameplay fact owners.
- Event queue responsibility is explicit: ADR-0002 owns typed DTO/subscription/context filtering/enqueue mechanics, while ADR-0004 owns combat tick drain ordering.

## ADR Dependency Order

Foundation:

1. ADR-0001: 场景生命周期与场景上下文路由
2. ADR-0003: 玩法逻辑、Unity 适配器与 HybridCLR 的运行时程序集分层

Depends on foundation:

3. ADR-0002: 战斗事件总线与 DTO 版本策略
4. ADR-0005: 伤害与 HP 所有权
5. ADR-0011: 角色数据模型 Schema 与快照所有权

Core timing and input:

6. ADR-0004: 战斗时钟与确定性战斗排序
7. ADR-0006: 移动触摸时间戳与输入触区策略

Feature contracts:

8. ADR-0007: 弹反时间轴派生与 Counter Handoff
9. ADR-0008: 敌方攻击模式数据与调度所有权
10. ADR-0012: 技能与行动服务边界

Presentation and validation:

11. ADR-0009: 战斗反馈质量分级与 WebGL 降级策略
12. ADR-0010: UIFrame 战斗 HUD 组成方式
13. ADR-0013: 确定性战斗逻辑测试策略

No dependency cycles were detected. All 13 ADRs are now marked `Accepted` after gate remediation.

## GDD Revision Flags

None. All reviewed GDD assumptions are consistent with current ADRs and verified engine reality.

## Engine Compatibility Issues

Engine: Unity 2022.3.17f1

| Check | Result |
|---|---|
| ADRs with Engine Compatibility section | 13 / 13 |
| Version consistency | Pass: all ADRs reference Unity 2022.3.17f1 |
| Post-cutoff APIs | Pass: all ADRs declare none |
| Deprecated API check | Incomplete: `deprecated-apis.md` is missing |
| Breaking/module reference check | Incomplete: `breaking-changes.md` and `modules/` are missing |

Engine specialist findings:

- No Unity 2022.3.17f1 behavior was found that independently causes architecture FAIL.
- High-risk validation gap: ADR-0002/ADR-0003 typed generic event bus plus HybridCLR/AOT needs a DTO/generic call-site preservation checklist or smoke test.
- High-risk validation gap: ADR-0006 WeChat/WebGL touch timestamp assumptions require device evidence for timestamp source, epoch/unit normalization, fallback ratio, bridge delay, and input-to-result latency.
- Medium-risk validation gap: ADR-0010 UGUI/UIFrame safe area and custom `InputHitAreaRegistry` boundaries require PlayMode and device evidence to avoid visible UI and judgment hit area mismatch.
- Medium-risk validation gap: ADR-0009 quality tiers are architecturally sound, but platform budgets and device evidence must still be validated during prototype work.
- Resource loading note: Addressables strategy remains P2 and the project currently appears AssetBundle/SBP-oriented. If MVP battle HUD/VFX requires async hot-update resource loading, add a resource loading ADR before implementation.

## Architecture Document Coverage

`docs/architecture/architecture.md` covers all 10 MVP systems from `systems-index.md`, includes data flow for input -> parry -> damage, enemy attack -> damage, counter, and scene unload, and lists future non-MVP systems as extension points.

No orphaned architecture modules were found.

Remaining acknowledged risks:

- Future systems such as 箱庭探索、回响值、记忆碎片、音频管理、存档系统 do not yet have GDD/ADR coverage. They are outside this MVP review scope.
- Deprecated/breaking/module Unity reference docs are incomplete and should be filled or explicitly waived before claiming full engine API audit coverage.

## Verdict

CONCERNS

The MVP architecture covers all 38 extracted technical requirements with no blocking cross-ADR conflict and no Unity version inconsistency. It does not receive PASS because engine reference coverage is incomplete and the highest-risk platform assumptions still require HybridCLR/WebGL/WeChat validation evidence.

## Blocking Issues

No coverage-gap or cross-ADR conflict blockers remain.

Implementation readiness concerns:

1. Add HybridCLR DTO/generic preservation smoke coverage.
2. Capture WeChat/WebGL timestamp, safe-area, low-FPS, and feedback performance evidence.
3. Fill or explicitly waive missing Unity reference docs before claiming deprecated/module API coverage.

## Required ADRs

No new MVP ADRs are required for coverage.

If MVP resource loading depends on hot-update async assets, create a follow-up resource loading ADR before implementing HUD/VFX resource flows.

## Handoff

Immediate actions:

1. Use the accepted ADR chain as the implementation baseline.
2. Run `/gate-check pre-production` after setup artifacts are updated.
3. Re-run `/ccgs-architecture-review` after any ADR or GDD changes to verify coverage remains complete.
# Architecture Review Report

> Date: 2026-04-29  
> Engine: Unity 2022.3.17f1  
> Mode: `/ccgs-architecture-review` full  
> Verdict: CONCERNS

## Loaded Inputs

- GDDs reviewed: 10 MVP system GDDs plus `game-concept.md` and `systems-index.md`
- ADRs reviewed: 13
- Architecture documents reviewed: `docs/architecture/architecture.md`
- Engine reference reviewed: `docs/engine-reference/unity/VERSION.md`
- Project standards reviewed: `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md`
- Missing optional inputs: `docs/engine-reference/unity/breaking-changes.md`, `docs/engine-reference/unity/deprecated-apis.md`, `docs/engine-reference/unity/modules/`, `docs/consistency-failures.md`

## Traceability Summary

| Metric | Count |
|---|---:|
| Total technical requirements | 38 |
| Covered by ADR | 38 |
| Partial ADR coverage | 0 |
| ADR gaps | 0 |

## Traceability Matrix

| Requirement ID | GDD | System | Requirement | ADR Coverage | Status |
|---|---|---|---|---|---|
| TR-concept-001 | `game-concept.md` | 产品 | 支持“回合制 RPG + 敌方攻击中嵌入实时弹反”的核心循环 | ADR-0003 | Covered |
| TR-concept-002 | `game-concept.md` | 表现 | 表现层必须放大精准操作的快感，但不得成为规则事实来源 | ADR-0003, ADR-0009 | Covered |
| TR-char-001 | `角色数据模型.md` | 角色数据 | 所有跨系统角色引用使用稳定 gameplay id | ADR-0011 | Covered |
| TR-char-002 | `角色数据模型.md` | 角色数据 | 区分静态定义、运行时实例和不可变快照 | ADR-0011 | Covered |
| TR-char-003 | `角色数据模型.md` | 角色数据 | 提供 `CanAct`、`CanBeTargeted`、`CanReceiveDamage`、`IsControllable` 等独立查询标记 | ADR-0011, ADR-0005 | Covered |
| TR-char-004 | `角色数据模型.md` | 角色数据 | 角色被击败后保留实例，直到战斗或场景清理显式移除 | ADR-0011, ADR-0005 | Covered |
| TR-input-001 | `输入系统.md` | 输入 | 战斗输入来自 Touch / PointerDown，不使用 UGUI click 合成事件做判定源 | ADR-0006 | Covered |
| TR-input-002 | `输入系统.md` | 输入 | 输出结构化 `ParryAttempt`，并携带时间戳诊断信息 | ADR-0006, ADR-0007 | Covered |
| TR-input-003 | `输入系统.md` | 输入 | 将平台时间戳映射到 `CombatClockMs`，并标记 fallback 来源 | ADR-0004, ADR-0006 | Covered |
| TR-input-004 | `输入系统.md` | 输入 | 拥有触区注册、优先级、遮挡和重复输入过滤 | ADR-0006, ADR-0010 | Covered |
| TR-scene-001 | `场景管理.md` | 场景 | 拥有 `SceneContext`、`sceneContextId` 和 `sceneVersion` | ADR-0001 | Covered |
| TR-scene-002 | `场景管理.md` | 场景 | 场景切换期间冻结战斗输入并拒绝迟到事件 | ADR-0001, ADR-0002 | Covered |
| TR-scene-003 | `场景管理.md` | 场景 | 在战斗场景生命周期内创建和销毁战斗临时运行态 | ADR-0001 | Covered |
| TR-turn-001 | `回合管理器.md` | 回合管理 | 拥有 `CombatClockMs` 和战斗阶段状态 | ADR-0004 | Covered |
| TR-turn-002 | `回合管理器.md` | 回合管理 | 发布显式 `BattleInputPhaseChanged` 事件 | ADR-0002, ADR-0004 | Covered |
| TR-turn-003 | `回合管理器.md` | 回合管理 | 调度敌方攻击段并分配时间轴序列身份 | ADR-0004, ADR-0008 | Covered |
| TR-turn-004 | `回合管理器.md` | 回合管理 | 将 `CounterWindow` 作为一等战斗状态 | ADR-0004, ADR-0007 | Covered |
| TR-turn-005 | `回合管理器.md` | 回合管理 | 只读取角色生命状态判定胜负，绝不写 HP | ADR-0005 | Covered |
| TR-dmg-001 | `伤害与生命系统.md` | 伤害 / HP | 作为唯一 HP 写入口 | ADR-0005, ADR-0011 | Covered |
| TR-dmg-002 | `伤害与生命系统.md` | 伤害 / HP | 消费具备幂等键的 `DamageRequest` | ADR-0005 | Covered |
| TR-dmg-003 | `伤害与生命系统.md` | 伤害 / HP | 精确应用弹反 `damageMultiplier` | ADR-0005, ADR-0007 | Covered |
| TR-dmg-004 | `伤害与生命系统.md` | 伤害 / HP | 发出 `DamageApplied`、`DamageRejected` 和 `CharacterDefeated` | ADR-0002, ADR-0005 | Covered |
| TR-parry-001 | `实时弹反系统.md` | 弹反 | 从 `AttackSegmentTimelineSeed` 派生并冻结弹反窗口 | ADR-0007 | Covered |
| TR-parry-002 | `实时弹反系统.md` | 弹反 | 用 `CombatClockMs` 而不是 Unity 帧时间结算 `ParryAttempt` | ADR-0004, ADR-0006, ADR-0007 | Covered |
| TR-parry-003 | `实时弹反系统.md` | 弹反 | 发出 `ParryResolved`、`ParryCancelled`、`OverlapRejected` 和时间轴拒绝事件 | ADR-0002, ADR-0007 | Covered |
| TR-parry-004 | `实时弹反系统.md` | 弹反 | 只由 `PerfectParry` 打开 counter，且不直接写 HP | ADR-0005, ADR-0007 | Covered |
| TR-enemy-001 | `敌人 AI 与攻击模式.md` | 敌人 AI | 按权重、冷却、HP 阶段和目标合法性选择脚本化攻击模式 | ADR-0008 | Covered |
| TR-enemy-002 | `敌人 AI 与攻击模式.md` | 敌人 AI | 拥有基础伤害、读招时序、连击规则等攻击内容事实 | ADR-0008 | Covered |
| TR-enemy-003 | `敌人 AI 与攻击模式.md` | 敌人 AI | 响应弹反、取消、重叠和 counter 事件，不得把被拒绝攻击转为必中 | ADR-0007, ADR-0008 | Covered |
| TR-action-001 | `技能与行动系统.md` | 行动 | MVP 只支持 `BasicAttackAction` 和 `CounterAction` | ADR-0012 | Covered |
| TR-action-002 | `技能与行动系统.md` | 行动 | 通过 `DamageRequest` 提交伤害，并保持行动请求幂等 | ADR-0005, ADR-0012 | Covered |
| TR-action-003 | `技能与行动系统.md` | 行动 | 只有收到 `CounterEntryOpened` 授权后才能执行 counter | ADR-0007, ADR-0012 | Covered |
| TR-fx-001 | `战斗反馈系统.md` | 反馈 | 只消费事件，绝不修改战斗事实 | ADR-0002, ADR-0009 | Covered |
| TR-fx-002 | `战斗反馈系统.md` | 反馈 | 发起 hit stop 请求，由回合管理器控制 `CombatClockMs` | ADR-0004, ADR-0009 | Covered |
| TR-fx-003 | `战斗反馈系统.md` | 反馈 | VFX 和震动可按性能等级降级，但不得改变结果 | ADR-0009 | Covered |
| TR-ui-001 | `战斗 UI.md` | 战斗 UI | 从事件和快照渲染状态，不直接读取可变玩法对象 | ADR-0002, ADR-0010 | Covered |
| TR-ui-002 | `战斗 UI.md` | 战斗 UI | 通过输入系统注册触区，counter 优先级高于 parry | ADR-0006, ADR-0010 | Covered |
| TR-ui-003 | `战斗 UI.md` | 战斗 UI | 不显示精确弹反倒计时；时机可读性来自敌人与反馈 cue | ADR-0010 | Covered |

## Coverage Gaps

None. No new MVP ADR is required for coverage.

## Cross-ADR Conflicts

No blocking cross-ADR conflicts were found.

Checked ownership boundaries:

- HP ownership is consistent: ADR-0005 makes `IDamageService` the only HP mutation path; ADR-0011 exposes character storage and controlled mutation boundaries without taking damage authority.
- Time authority is consistent: ADR-0004 gives the turn manager the only mutable `CombatClockMs`; ADR-0006, ADR-0007, and ADR-0009 only map, read, or request timing behavior.
- Input authority is consistent: ADR-0006 owns timestamp mapping, hit testing, priority, occlusion, and duplicate filtering; ADR-0010 owns UI layout and registration lifecycle only.
- Presentation boundaries are consistent: ADR-0009 and ADR-0010 make feedback/UI event consumers rather than gameplay fact owners.
- Event queue responsibility is now explicit: ADR-0002 owns typed DTO/subscription/context filtering/enqueue mechanics, while ADR-0004 owns combat tick drain ordering.

## ADR Dependency Order

Foundation:

1. ADR-0001: 场景生命周期与场景上下文路由
2. ADR-0003: 玩法逻辑、Unity 适配器与 HybridCLR 的运行时程序集分层

Depends on foundation:

3. ADR-0002: 战斗事件总线与 DTO 版本策略
4. ADR-0005: 伤害与 HP 所有权
5. ADR-0011: 角色数据模型 Schema 与快照所有权

Core timing and input:

6. ADR-0004: 战斗时钟与确定性战斗排序
7. ADR-0006: 移动触摸时间戳与输入触区策略

Feature contracts:

8. ADR-0007: 弹反时间轴派生与 Counter Handoff
9. ADR-0008: 敌方攻击模式数据与调度所有权
10. ADR-0012: 技能与行动服务边界

Presentation and validation:

11. ADR-0009: 战斗反馈质量分级与 WebGL 降级策略
12. ADR-0010: UIFrame 战斗 HUD 组成方式
13. ADR-0013: 确定性战斗逻辑测试策略

Unresolved dependency note: all 13 ADRs are currently `Proposed`. Content coverage is complete, but dependent ADRs should not be treated as accepted implementation authority until their prerequisites are accepted.

No dependency cycles were detected.

## GDD Revision Flags

None. All reviewed GDD assumptions are consistent with current ADRs and verified engine reality.

## Engine Compatibility Issues

Engine: Unity 2022.3.17f1

| Check | Result |
|---|---|
| ADRs with Engine Compatibility section | 13 / 13 |
| Version consistency | Pass: all ADRs reference Unity 2022.3.17f1 |
| Post-cutoff APIs | Pass: all ADRs declare none |
| Deprecated API check | Incomplete: `deprecated-apis.md` is missing |
| Breaking/module reference check | Incomplete: `breaking-changes.md` and `modules/` are missing |

Engine specialist findings:

- No Unity 2022.3.17f1 behavior was found that independently causes architecture FAIL.
- High-risk validation gap: ADR-0002/ADR-0003 typed generic event bus plus HybridCLR/AOT needs a DTO/generic call-site preservation checklist or smoke test.
- High-risk validation gap: ADR-0006 WeChat/WebGL touch timestamp assumptions require device evidence for timestamp source, epoch/unit normalization, fallback ratio, bridge delay, and input-to-result latency.
- Medium-risk validation gap: ADR-0010 UGUI/UIFrame safe area and custom `InputHitAreaRegistry` boundaries require PlayMode and device evidence to avoid visible UI and judgment hit area mismatch.
- Medium-risk validation gap: ADR-0009 quality tiers are architecturally sound, but FPS, frame budget, draw calls, and memory ceiling remain `[TO BE CONFIGURED]` in technical preferences.
- Resource loading note: Addressables strategy remains P2 and the project currently appears AssetBundle/SBP-oriented. If MVP battle HUD/VFX requires async hot-update resource loading, add a resource loading ADR before implementation.

## Architecture Document Coverage

`docs/architecture/architecture.md` covers all 10 MVP systems from `systems-index.md`, includes data flow for input -> parry -> damage, enemy attack -> damage, counter, and scene unload, and lists future non-MVP systems as extension points.

No orphaned architecture modules were found.

Remaining acknowledged risks:

- Future systems such as 箱庭探索、回响值、记忆碎片、音频管理、存档系统 do not yet have GDD/ADR coverage. They are outside this MVP review scope.
- Engine reference docs and performance budgets are incomplete and should be closed or explicitly waived before a pre-production PASS.

## Verdict

CONCERNS

The MVP architecture now covers all 38 extracted technical requirements with no blocking cross-ADR conflict and no Unity version inconsistency. It does not receive PASS yet because every ADR remains `Proposed`, engine reference coverage is incomplete, and the highest-risk platform assumptions still require HybridCLR/WebGL/WeChat validation evidence.

## Blocking Issues

No coverage-gap or cross-ADR conflict blockers remain.

Implementation readiness concerns before treating this as a PASS baseline:

1. Accept the P0 ADR dependency chain, especially ADR-0001 through ADR-0006.
2. Configure target FPS, frame budget, draw call budget, and memory ceiling.
3. Add HybridCLR DTO/generic preservation smoke coverage.
4. Plan WeChat/WebGL timestamp, safe-area, low-FPS, and feedback performance device evidence.
5. Fill or explicitly waive missing Unity reference docs before claiming deprecated/module API coverage.

## Required ADRs

No new MVP ADRs are required for coverage.

If MVP resource loading depends on hot-update async assets, create a follow-up resource loading ADR before implementing HUD/VFX resource flows.

## Handoff

Immediate actions:

1. Review and accept or revise ADR-0001 through ADR-0006 as the P0 implementation baseline.
2. Configure performance budgets in `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md`.
3. Add validation tasks for HybridCLR AOT preservation and WeChat WebGL touch timestamp evidence.

When all concerns are resolved, run `/gate-check pre-production` to advance. Re-run `/ccgs-architecture-review` after any ADR changes to verify coverage remains complete.
# Architecture Review Report

> Date: 2026-04-29  
> Engine: Unity 2022.3.17f1  
> Mode: `/ccgs-architecture-review` full  
> Verdict: FAIL

## Loaded Inputs

- GDDs reviewed: 10 MVP system GDDs plus `game-concept.md` and `systems-index.md`
- ADRs reviewed: 6
- Architecture documents reviewed: `docs/architecture/architecture.md`
- Engine reference reviewed: `docs/engine-reference/unity/VERSION.md`
- Project standards reviewed: `.cursor/skills/ccgs-studio/references/docs/technical-preferences.md`
- Missing optional inputs: `docs/engine-reference/unity/breaking-changes.md`, `docs/engine-reference/unity/deprecated-apis.md`, `docs/engine-reference/unity/modules/`, `docs/architecture/tr-registry.yaml`, `docs/consistency-failures.md`

## Traceability Summary

| Metric | Count |
|---|---:|
| Total technical requirements | 38 |
| Covered by ADR | 26 |
| Partial ADR coverage | 6 |
| ADR gaps | 6 |

## Traceability Matrix

| Requirement ID | GDD | System | Requirement | ADR Coverage | Status |
|---|---|---|---|---|---|
| TR-concept-001 | `game-concept.md` | 产品 | 支持回合制 RPG + 实时弹反核心循环 | ADR-0003 | Covered |
| TR-concept-002 | `game-concept.md` | 表现 | 表现层放大精准操作但不成为规则事实来源 | ADR-0002, ADR-0003 | Covered |
| TR-char-001 | `角色数据模型.md` | 角色数据 | 所有跨系统角色引用使用稳定 gameplay id | ADR-0003 | Partial |
| TR-char-002 | `角色数据模型.md` | 角色数据 | 区分静态定义、运行时实例和不可变快照 | - | Gap |
| TR-char-003 | `角色数据模型.md` | 角色数据 | 提供 `CanAct`、`CanBeTargeted`、`CanReceiveDamage`、`IsControllable` | ADR-0005 | Covered |
| TR-char-004 | `角色数据模型.md` | 角色数据 | 被击败角色保留实例直到显式清理 | ADR-0005 | Covered |
| TR-input-001 | `输入系统.md` | 输入 | 战斗输入来自 Touch / PointerDown，不用 UGUI click 判定 | ADR-0006 | Covered |
| TR-input-002 | `输入系统.md` | 输入 | 输出结构化 `ParryAttempt` 和时间戳诊断 | ADR-0006 | Covered |
| TR-input-003 | `输入系统.md` | 输入 | 平台时间戳映射到 `CombatClockMs` 并标记 fallback | ADR-0004, ADR-0006 | Covered |
| TR-input-004 | `输入系统.md` | 输入 | 输入系统拥有触区注册、优先级、遮挡和去重 | ADR-0006 | Covered |
| TR-scene-001 | `场景管理.md` | 场景 | 拥有 `SceneContext`、`sceneContextId`、`sceneVersion` | ADR-0001 | Covered |
| TR-scene-002 | `场景管理.md` | 场景 | 切换期间冻结输入并拒绝迟到事件 | ADR-0001, ADR-0002 | Covered |
| TR-scene-003 | `场景管理.md` | 场景 | 战斗生命周期内创建和销毁战斗临时运行态 | ADR-0001 | Covered |
| TR-turn-001 | `回合管理器.md` | 回合 | 拥有 `CombatClockMs` 和战斗阶段状态 | ADR-0004 | Covered |
| TR-turn-002 | `回合管理器.md` | 回合 | 发布 `BattleInputPhaseChanged` | ADR-0002, ADR-0004 | Covered |
| TR-turn-003 | `回合管理器.md` | 回合 | 调度敌方攻击段并分配时间轴序列身份 | ADR-0004 | Covered |
| TR-turn-004 | `回合管理器.md` | 回合 | `CounterWindow` 是一等战斗状态 | ADR-0004 | Covered |
| TR-turn-005 | `回合管理器.md` | 回合 | 只读 HP 判定胜负，不写 HP | ADR-0005 | Covered |
| TR-dmg-001 | `伤害与生命系统.md` | 伤害/HP | 唯一 HP 写入口 | ADR-0005 | Covered |
| TR-dmg-002 | `伤害与生命系统.md` | 伤害/HP | 消费幂等 `DamageRequest` | ADR-0005 | Covered |
| TR-dmg-003 | `伤害与生命系统.md` | 伤害/HP | 精确应用弹反 `damageMultiplier` | ADR-0005 | Covered |
| TR-dmg-004 | `伤害与生命系统.md` | 伤害/HP | 发出 `DamageApplied`、`DamageRejected`、`CharacterDefeated` | ADR-0002, ADR-0005 | Covered |
| TR-parry-001 | `实时弹反系统.md` | 弹反 | 从 seed 派生并冻结弹反窗口 | - | Gap |
| TR-parry-002 | `实时弹反系统.md` | 弹反 | 用 `CombatClockMs` 而非 Unity 帧时间结算 | ADR-0004, ADR-0006 | Covered |
| TR-parry-003 | `实时弹反系统.md` | 弹反 | 发出 resolved/cancelled/overlap/timeline rejection 事件 | ADR-0002 | Covered |
| TR-parry-004 | `实时弹反系统.md` | 弹反 | Perfect 打开 counter，且不直接写 HP | ADR-0004, ADR-0005, ADR-0006 | Partial |
| TR-enemy-001 | `敌人 AI 与攻击模式.md` | 敌人 AI | 按权重、冷却、HP 阶段、目标合法性选择攻击模式 | - | Gap |
| TR-enemy-002 | `敌人 AI 与攻击模式.md` | 敌人 AI | 拥有基础伤害、读招时序、连击规则等内容事实 | ADR-0004 | Partial |
| TR-enemy-003 | `敌人 AI 与攻击模式.md` | 敌人 AI | 响应弹反/取消/重叠/counter，拒绝攻击不得转必中 | ADR-0002, ADR-0004 | Partial |
| TR-action-001 | `技能与行动系统.md` | 行动 | MVP 只支持 `BasicAttackAction` 和 `CounterAction` | - | Gap |
| TR-action-002 | `技能与行动系统.md` | 行动 | 通过 `DamageRequest` 提交伤害并保持行动幂等 | ADR-0005 | Partial |
| TR-action-003 | `技能与行动系统.md` | 行动 | 只有 `CounterEntryOpened` 授权后才能执行 counter | ADR-0004, ADR-0006 | Partial |
| TR-fx-001 | `战斗反馈系统.md` | 反馈 | 只消费事件，不修改战斗事实 | ADR-0002, ADR-0003 | Covered |
| TR-fx-002 | `战斗反馈系统.md` | 反馈 | 发起 hit stop 请求，由回合管理器控制时钟 | ADR-0004 | Covered |
| TR-fx-003 | `战斗反馈系统.md` | 反馈 | VFX/震动可按性能等级降级但不得改变结果 | - | Gap |
| TR-ui-001 | `战斗 UI.md` | UI | 从事件和快照渲染，不直接读可变玩法对象 | ADR-0002, ADR-0003 | Covered |
| TR-ui-002 | `战斗 UI.md` | UI | 通过输入系统注册触区，counter 优先级高于 parry | ADR-0006 | Covered |
| TR-ui-003 | `战斗 UI.md` | UI | 不显示精确弹反倒计时，读招来自敌人与反馈 cue | - | Gap |

## Coverage Gaps

| Requirement | Gap | Suggested ADR | Domain | Engine Risk |
|---|---|---|---|---|
| TR-parry-001 | Core parry timeline derivation and frozen window authority are not governed by an ADR. | `/architecture-decision 弹反时间轴派生与 counter handoff` | Combat / Timing | High |
| TR-enemy-001 | Scripted enemy pattern selection rules are not governed by an ADR. | `/architecture-decision 敌方攻击模式数据与调度所有权` | Combat AI | Low/Medium |
| TR-action-001 | MVP action set and action service boundary are not governed by an ADR. | `/architecture-decision 技能与行动服务边界` | Combat Actions | Low |
| TR-fx-003 | Feedback quality tier and WebGL degradation policy are not governed by an ADR. | `/architecture-decision 战斗反馈质量分级与 WebGL 降级策略` | Presentation / Performance | High |
| TR-ui-003 | HUD composition and no-countdown readability rule are not governed by an ADR. | `/architecture-decision UIFrame 战斗 HUD 组成方式` | UI | Medium |
| TR-char-002 | Character definition/runtime/snapshot ownership needs a governing ADR or explicit inclusion in ADR-0005. | `/architecture-decision 角色数据模型 schema 与快照所有权` | State / Data Model | Low |

## Cross-ADR Conflicts

### Conflict: ADR-0002 vs ADR-0004

Type: Integration / State ownership

ADR-0002 claims: the battle event bus runs on the gameplay thread and drains a tick-local event queue by priority and enqueue order.

ADR-0004 claims: the turn manager owns the tick-local drain loop and deterministic priority order.

Impact: implementation may produce two competing schedulers or unclear authority for same-tick ordering, directly affecting input -> parry -> damage -> counter determinism.

Resolution options:

1. Make ADR-0004 the authority for combat tick drain order; ADR-0002 only defines typed DTO and subscription mechanics.
2. Make ADR-0002 event bus own drain mechanics; ADR-0004 defines the required priority policy consumed by the bus.
3. Split responsibilities explicitly: turn manager starts/ends ticks, event bus drains using ADR-0004 priority table.

## ADR Dependency Order

Foundation:

1. ADR-0001: 场景生命周期与场景上下文路由
2. ADR-0003: 玩法逻辑、Unity 适配器与 HybridCLR 的运行时程序集分层

Depends on Foundation:

3. ADR-0002: 战斗事件总线与 DTO 版本策略 (depends on ADR-0001)
4. ADR-0004: 战斗时钟与确定性战斗排序 (depends on ADR-0002, ADR-0003)
5. ADR-0005: 伤害与 HP 所有权 (depends on ADR-0002, ADR-0003)

Feature input layer:

6. ADR-0006: 移动触摸时间戳与输入触区策略 (depends on ADR-0001, ADR-0002, ADR-0004)

Unresolved dependency note: all 6 ADRs are currently `Proposed`; any ADR depending on another Proposed ADR is not safe to treat as accepted implementation authority yet.

No dependency cycles were detected.

## GDD Revision Flags

None. No GDD assumptions were found that conflict with verified engine behavior or accepted ADR decisions.

## Engine Compatibility Issues

Engine: Unity 2022.3.17f1

| Check | Result |
|---|---|
| ADRs with Engine Compatibility section | 6 / 6 |
| Version consistency | Pass: all ADRs reference Unity 2022.3.17f1 |
| Post-cutoff APIs | Pass: all ADRs declare none |
| Deprecated API check | Incomplete: `deprecated-apis.md` is missing |
| Breaking/module reference check | Incomplete: `breaking-changes.md` and `modules/` are missing |

Engine specialist findings:

- HIGH: ADR-0006 and ADR-0004 correctly isolate WeChat WebGL touch timestamp risk, but must be backed by device evidence: fallback ratio, bridge delay, low-FPS replay tests, and input-to-result latency.
- HIGH: ADR-0002 and ADR-0003 correctly identify HybridCLR/AOT generic preservation risk; final architecture should require an AOT generic/link preservation strategy and DTO coverage checklist.
- HIGH: ADR-0001 and ADR-0003 correctly avoid threading assumptions and require explicit WebGL cleanup; still needs scene unload/retry smoke tests and subscription/resource release evidence.
- MEDIUM: engine reference library is incomplete, so deprecated API and module-level API checks remain a validation gap, not a discovered compatibility failure.

## Architecture Document Coverage

`docs/architecture/architecture.md` covers all 10 MVP systems from `systems-index.md`, includes data flow for input -> parry -> damage, enemy attack -> damage, counter, and scene unload, and lists future non-MVP systems as extension points.

No orphaned architecture modules were found.

Open coverage issues already acknowledged by the architecture document:

- 弹反时间轴与结果权威
- 表现降级与 hit stop 权威细化
- UIFrame HUD 与输入区域所有权细化
- 确定性战斗逻辑测试策略
- 敌方攻击模式数据与调度所有权
- 行动系统边界

## Verdict

FAIL

The architecture has improved substantially since the previous review: 26 of 38 technical requirements now have ADR coverage. It still cannot pass because there is one blocking cross-ADR ownership conflict, the core parry timeline requirement lacks a governing ADR, several MVP implementation-facing requirements are partial or uncovered, and all current ADRs remain `Proposed`.

## Blocking Issues

1. Resolve ADR-0002 vs ADR-0004 event drain ownership before implementation uses either policy.
2. Create or approve an ADR for `TR-parry-001`: parry timeline derivation and counter handoff.
3. Cover the remaining MVP gaps for enemy pattern selection, action service boundary, feedback degradation, HUD no-countdown/readability, and character snapshot ownership.
4. Move P0 ADRs from `Proposed` to accepted status only after their dependency chain and conflict resolution are settled.
5. Fill or explicitly waive missing engine reference docs before claiming deprecated/module API coverage.

## Required ADRs

Highest priority:

1. `/architecture-decision 弹反时间轴派生与 counter handoff`
2. `/architecture-decision 敌方攻击模式数据与调度所有权`
3. `/architecture-decision 战斗反馈质量分级与 WebGL 降级策略`

Also required or merge into existing ADRs:

1. `/architecture-decision UIFrame 战斗 HUD 组成方式`
2. `/architecture-decision 角色数据模型 schema 与快照所有权`
3. `/architecture-decision 技能与行动服务边界`
4. `/architecture-decision 确定性战斗逻辑测试策略`

## Handoff

Immediate actions:

1. Revise ADR-0002 or ADR-0004 to make event queue draining ownership explicit.
2. Write the parry timeline / counter handoff ADR.
3. Write enemy attack ownership and feedback degradation ADRs next, because they carry the highest remaining implementation risk.

When blocking issues are resolved, run `/gate-check pre-production` to advance. Re-run `/ccgs-architecture-review` after each new ADR is written to verify coverage improves.
