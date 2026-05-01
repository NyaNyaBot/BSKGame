# Unity Editor Smoke Evidence - 2026-05-01

## Scope

- Gate context: Pre-Production epic/story completion + engine doc remediation.
- Evidence type: Unity Editor MCP smoke check.
- Platform: macOS Unity Editor.
- Unity version: 2022.3.17f1.
- Unity instance: `client@b6e1c347cc7cad76`.
- Active scene: `Assets/GameRes/Scenes/Launch.unity`.
- Build target: WebGL.

## Steps Performed

1. Confirmed Unity MCP connection and active Editor state.
2. Read Unity Console for errors (0 errors, 0 warnings).
3. Verified editor state: idle, not playing, not compiling, no pending domain reload.
4. Listed Unity Test Framework tests discovered by the Editor (6 total: 5 EditMode + 1 PlayMode).
5. Ran all EditMode tests.

## EditMode Test Results

- **Total**: 5
- **Passed**: 5
- **Failed**: 0
- **Skipped**: 0
- **Duration**: 1.63s
- **Result State**: Passed

### Individual Results

| Test | State | Duration |
|---|---|---|
| `ArchitectureSmokeTests.UnityTestFramework_IsAvailable` | Passed | 0.050s |
| `SceneContextIdentityTests.Battle_Dispose_ThenOldEventContext_IsNotCurrent` | Passed | 0.033s |
| `SceneContextIdentityTests.BeginSceneTransition_IncrementsSceneVersion_AndSetsTransitioning` | Passed | 0.018s |
| `SceneContextIdentityTests.EnterScene_EachEntry_NewContextId_AndIncrementsVersion` | Passed | 0.004s |
| `SceneContextIdentityTests.SceneOnlyEvent_AfterTransition_StaleBySceneVersion` | Passed | 0.001s |

## Engine Reference Remediation

- `docs/engine-reference/unity/deprecated-apis.md` — created, confirms no deprecated API usage in ADRs.
- `docs/engine-reference/unity/breaking-changes.md` — created, confirms no breaking change impact on BSKGame.

## Epic/Story Completion

- 5 new Epics created (Feature + Presentation layers): parry-system, enemy-ai-attack-patterns, action-system, battle-feedback, battle-ui.
- 16 new Stories created across all new Epics.
- `production/epics/index.md` updated: 12 Epics / 38 Stories total.

## Verdict

**PASS** for Unity Editor smoke readiness and test baseline.

## Remaining Evidence Needed

- HybridCLR hot-update and AOT generic preservation smoke evidence.
- WeChat/WebGL touch timestamp source, fallback ratio, bridge delay, and input-to-result latency evidence.
- Safe-area and touch hit target screenshots on target devices.
- Low-FPS timing and feedback degradation evidence.
- Vertical Slice playable build + playtest sessions (3+).
