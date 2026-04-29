## Review — 2026-04-26 — Verdict: MAJOR REVISION NEEDED
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, economy-designer, ai-programmer, ux-designer, ui-programmer, audio-director, gameplay-programmer, performance-analyst, unity-specialist, technical-artist, creative-director
Blocking items: 8 | Recommended: 10+
Summary: Senior review found the GDD direction strong but not implementation-ready because the player fantasy bridge, timing authority, event contracts, result semantics, overlap rules, feedback budgets, and testability were underspecified. The revision applied in this session locks MVP scope, CombatClock usage, AttackSegmentTimeline, ParryAttempt/ParryResolved payloads, Perfect/Normal/Failed value separation, overlap/unparryable decisions, provisional feedback budgets, and testable acceptance criteria.
Prior verdict resolved: First review

### Key Blocking Findings

1. MVP single-character scope needed an explicit bridge back to the full three-character fantasy.
2. `NormalParry = no damage + echo gain` diluted the value of precise Perfect timing.
3. Combat timing lacked a single authoritative clock across input, hit stop, pause, low FPS, and animation.
4. Formula gates, buffered input semantics, timeline invariants, and output ranges were internally inconsistent.
5. `ParryResult` was too narrow for cancellation, invalid input, too-early input, duplicate input, and target invalid states.
6. Multi-enemy overlap, unparryable attacks, and combo continuation had conflicting or non-deterministic rules.
7. UI/audio/VFX feedback direction was strong but lacked event contracts, priorities, budgets, and degradation rules.
8. Multiple acceptance criteria were not independently testable without thresholds, event payloads, or observable counters.

### Revision Applied

- Status moved to `In Review — Revised after MAJOR REVISION review`.
- Added revision decisions for MVP scope, result value, combo interruption, unparryable attacks, overlap rejection, and echo ownership.
- Added `Implementation Contract` covering `CombatClockMs`, `AttackSegmentTimeline`, event payloads, event order, and ownership.
- Updated formulas for latency compensation, eligibility gates, buffered input semantics, Normal damage multiplier, and clamped provisional echo gain.
- Added provisional feedback budgets, degradation rules, UGUI/PointerDown requirements, HUD state matrix, touch layout contract, and UI event contract.
- Replaced the acceptance criteria with testable logic, event, timing, UI, device, and performance-observation criteria.

## Re-Review — 2026-04-26 — Verdict: NEEDS REVISION
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, economy-designer, ai-programmer, ux-designer, ui-programmer, audio-director, gameplay-programmer, performance-analyst, unity-specialist, technical-artist
Blocking items: 10 | Recommended: 10+
Summary: Specialist re-review found the previous major blockers mostly resolved, but identified remaining implementation-contract issues around late-window timing, buffered input, no-result versus failed-result flow, ID naming, overlap handling, UI raycast handoff, audio scheduling, performance pass criteria, Unity layering, and touch timestamp mapping. A formal creative-director re-review could not be spawned because the Cursor tool returned an unpaid-invoice error; the main session applied the consensus fixes directly.
Prior verdict resolved: Partially — major revision reduced to targeted revision.

### Follow-Up Revisions Applied

- Split `visualImpactMs` and `damageCommitMs`, and added `center_before_impact_ms >= normal_late_window_ms` as a hard invariant.
- Reworked buffered input so valid buffer inputs resolve at `window_open_timestamp_ms` instead of failing by original timestamp.
- Replaced `eligible_for_resolution` with `resolution_flow = Resolved / Rejected / NoResult`, keeping cancellation, invalid targets, unparryable tests, and overlap rejection out of `ParryResolved(FailedParry)`.
- Renamed ambiguous IDs to `timelineSequenceId`, `inputSequenceId`, and `resolutionId`.
- Added `CombatClock`, `TouchTimestampAdapter`, runtime layering, DTO purity, combo interrupt rules, and attack-mode callback obligations.
- Tightened `OverlapRejected` so rejected attacks must be rescheduled or cancelled, not converted into unavoidable unparryable hits.
- Clarified `EchoIntent` ownership and demoted `test_echo_gain` to an automation/prototype-only fixture.
- Added quality tiers, performance pass criteria, audio event timing, voice priority, touch anchor specs, input feedback layers, UI event versions, and counter/raycast handoff rules.
- Updated acceptance criteria to reflect the revised timing, event, ID, buffer, device evidence, and performance logging contracts.

## Formal Full Re-Review — 2026-04-27 — Verdict: NEEDS REVISION
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, economy-designer, ai-programmer, ux-designer, ui-programmer, audio-director, gameplay-programmer, performance-analyst, unity-specialist, technical-artist, creative-director
Blocking items: 8 | Recommended: 6
Summary: Formal full review confirmed the GDD no longer needs a major redesign, but it is not yet implementation-ready. Remaining blockers are focused on timestamp/latency ownership, event result matrices, attack timeline seed/full derivation, echo event authority, counter handoff, DTO alignment with input/scene GDDs, HUD-independent readability, and mobile device evidence.
Prior verdict resolved: Partially — MAJOR REVISION resolved; still NEEDS REVISION before implementation.

### Required Before Implementation

1. Unify timestamp and latency compensation ownership so raw timestamp mapping and `latency_compensation_ms` cannot double-compensate or push boundary inputs out of valid windows.
2. Add a definitive event matrix for `Resolved / Rejected / NoResult`, mapping each condition to `ParryResolved`, `ParryCancelled`, `ParryAttemptRejected`, or `OverlapRejected` with payload and downstream effects.
3. Split `AttackSegmentTimelineSeed` and frozen `AttackSegmentTimeline`, including derivation formulas, validation failure behavior, and `timelineSequenceId` lifecycle.
4. Resolve echo event authority: either `ParryResolved.echoIntent` or `EchoIntentRequested` is the production entry point, with `resolutionId` idempotency.
5. Lock the MVP counter handoff path, including `CounterEntryOpened`, close/consume/expire events, and raycast/input ownership.
6. Align DTO payloads with approved `输入系统` and `场景管理` GDDs, including scene context/version and device evidence fields.
7. Add HUD-independent readability acceptance criteria so enemy animation/audio remain the primary timing source.
8. Expand mobile device evidence for touch timestamp bridge, safe area/gesture inset, audio onset, input-to-result latency, and device/build metadata.

### Senior Verdict

The core design is sound and consistent with the game concept, but implementation should wait until the remaining ownership and validation contracts are tightened. The next revision should be focused, not a rewrite.

## Focused Re-Review — 2026-04-27 — Verdict: APPROVED
Scope signal: L
Specialists: lean main-session review against formal full review blockers
Blocking items: 0 | Recommended: 3
Summary: Focused revision resolved the 8 implementation blockers from the formal full review. The GDD now has explicit timestamp/latency ownership, a definitive resolution event matrix, split seed/frozen attack timelines, one production echo entry point, locked counter handoff, DTO alignment with input/scene GDDs, HUD-independent readability criteria, and expanded mobile evidence requirements.
Prior verdict resolved: Yes — formal full re-review blockers addressed.

### Remaining Recommendations

1. 回合管理器 GDD should adopt `CombatClockMs`, `AttackSegmentTimelineSeed`, `FrozenAttackSegmentTimeline`, and `CounterEntryClosed` semantics rather than redefining attack timing.
2. 伤害与生命系统 GDD should explicitly consume `ParryResolved.damageMultiplier` and own final HP/death rules.
3. 敌人 AI 与攻击模式 GDD should enforce `OverlapRejected` reschedule/cancel behavior and combo interrupt rules.
