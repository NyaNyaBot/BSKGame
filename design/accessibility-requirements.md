# Accessibility Requirements

> Status: Committed  
> Tier: Basic  
> Last Updated: 2026-04-29  
> Applies To: MVP and Pre-Production prototypes

## Accessibility Tier

BSKGame commits to **Basic** accessibility for MVP and Pre-Production. The game is a small solo/lean project targeting touch-only WeChat WebGL, so requirements focus on avoiding preventable blockers while preserving the timing-sensitive parry fantasy.

## Required Baseline

| Area | Requirement | Evidence |
|---|---|---|
| Touch targets | Core combat hit areas must meet or exceed the input GDD minimum, currently `88dp`; preferred parry area is `112dp`. | Safe-area device evidence |
| Safe areas | Parry, counter, and critical UI buttons must not overlap system gesture or notch areas. | WebGL / device screenshots |
| Color readability | Critical states cannot be color-only. Perfect, Normal, Failed, Counter, and Low HP need shape, motion, text, or audio differences. | UX review / playtest |
| Text readability | HUD and result text must remain readable on mobile screens; debug overlays may be smaller but must be development-only. | UX screenshot evidence |
| Audio dependency | Audio cues may reinforce timing but must not be the only way to read enemy attacks. | Playtest checklist |
| Motion and flash | PerfectParry effects may peak briefly, but repeated flash/shake must have a low-intensity fallback through feedback quality tiers. | Feedback profile review |
| Pause | Battle must support pause/system suspend without losing or replaying combat input. | PlayMode / smoke evidence |

## Non-Goals for MVP

- Full remapping beyond touch area layout options.
- Text-to-speech support.
- Full colorblind simulation tooling.
- Complete motor accessibility menu.
- Localization accessibility beyond readable layout and font sizing.

## Gate Criteria

Before entering Production, key UX specs must reference this document and state how each screen satisfies the Basic tier. Any exception must be recorded in the relevant UX review or gate report.
