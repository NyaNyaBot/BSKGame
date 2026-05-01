# Unity Editor Smoke Evidence - 2026-04-29

## Scope

- Gate context: Technical Setup -> Pre-Production remaining evidence.
- Evidence type: Unity Editor MCP smoke check.
- Platform: macOS Unity Editor.
- Unity version: 2022.3.17f1.
- Unity instance: `client@b6e1c347cc7cad76`.
- Active scene: `Assets/Prototypes/ParryCombat/Scenes/ParryPrototype.unity`.

## Steps Performed

1. Confirmed Unity MCP connection and active Editor state.
2. Forced Unity asset refresh and requested script compilation.
3. Read Unity Console for errors and warnings.
4. Listed Unity Test Framework tests discovered by the Editor.
5. Ran the EditMode architecture smoke test.
6. Ran the discovered PlayMode prototype test entry.

## Observed Result

- Editor state before testing: idle, not playing, not compiling, no pending domain reload.
- Refresh result: succeeded; MCP recovered from one transient disconnect and reported the Editor ready.
- Console result: 0 errors or warnings.
- Discovered tests:
  - `Game.Tests.EditMode.ArchitectureSmokeTests.UnityTestFramework_IsAvailable` (`EditMode`)
  - `Berserker` (`PlayMode`)
- EditMode result:
  - total: 1
  - passed: 1
  - failed: 0
  - skipped: 0
  - result state: Passed
- PlayMode result:
  - result item: `Berserker` Passed
  - reported summary: total 0, passed 0, failed 0, skipped 0, result state Passed
  - note: this appears to be an existing prototype test entry with an unusual zero-total summary and should be normalized before relying on it as gate evidence.

## Verdict

PASS for Unity Editor smoke readiness.

This evidence confirms the Editor connection, script refresh/compile path, Console cleanliness, and EditMode test framework baseline. It does not close the remaining HybridCLR/WebGL/WeChat device evidence concerns.

## Remaining Evidence Needed

- HybridCLR hot-update and AOT generic preservation smoke evidence.
- WeChat/WebGL touch timestamp source, fallback ratio, bridge delay, and input-to-result latency evidence.
- Safe-area and touch hit target screenshots on target devices.
- Low-FPS timing and feedback degradation evidence.
- Unity deprecated/breaking/module reference documentation or explicit waiver before the next gate.
