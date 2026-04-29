# Test Infrastructure

**Engine**: Unity 2022.3.17f1  
**Test Framework**: Unity Test Framework / NUnit  
**CI**: `.github/workflows/tests.yml`  
**Setup date**: 2026-04-29

## Directory Layout

```text
tests/
  unit/           # Isolated unit tests for formulas, state machines, and pure logic
  integration/    # Cross-system tests and adapter contracts
  smoke/          # Critical path test list for /smoke-check
  evidence/       # Manual evidence, screenshots, device logs, and sign-offs
```

Unity-discovered tests live under `client/Assets/Tests/` so the Unity Test Framework can compile and run them. The root `tests/` directory is the planning and evidence index used by production gates.

## Running Tests

Use Unity Test Runner locally:

1. Open `client/` in Unity 2022.3.17f1.
2. Open `Window -> General -> Test Runner`.
3. Run EditMode tests first, then PlayMode tests when adapter scenes exist.

CI runs EditMode and PlayMode through `game-ci/unity-test-runner`.

## Test Naming

- Files: `[system]_[feature]_test.cs`
- Methods: `Scenario_ExpectedResult`
- Example: `CombatClockTests.Advance_WhenActive_IncreasesNowMs`

## Story Type → Test Evidence

| Story Type | Required Evidence | Location |
|---|---|---|
| Logic | Automated unit test | `tests/unit/[system]/` and `client/Assets/Tests/EditMode/` |
| Integration | Integration test or playtest evidence | `tests/integration/[system]/` |
| Visual/Feel | Screenshot, device capture, or lead sign-off | `tests/evidence/` |
| UI | Manual walkthrough or interaction test | `tests/evidence/` |
| Config/Data | Smoke check pass | `production/qa/smoke-*.md` |

## CI

Tests run on push to `main` and on pull requests. Unity CI requires a `UNITY_LICENSE` repository secret before the first run.
