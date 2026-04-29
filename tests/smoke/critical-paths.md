# Smoke Test: Critical Paths

**Purpose**: Run these checks before QA hand-off or gate advancement.  
**Run via**: `/smoke-check` after the first playable combat prototype exists.  
**Update**: Add sprint-specific checks as systems are implemented.

## Core Stability

1. Unity project opens without compile errors.
2. Hot-update assemblies load without HybridCLR metadata errors.
3. Game reaches the initial scene without crash.

## MVP Combat Loop

4. Battle context can be created from a valid scene context.
5. Player command phase enables only player action input.
6. Enemy attack phase enables parry input.
7. PerfectParry produces zero damage and opens counter.
8. NormalParry applies half damage.
9. FailedParry applies full damage.
10. Scene unload clears input areas, HUD subscriptions, active attack segments, and pending feedback.

## Platform Evidence

11. WeChat/WebGL device evidence records timestamp source and fallback ratio.
12. Low-FPS simulation keeps parry result based on mapped input timestamp.
13. Feedback degradation does not change parry grade, damage, or event order.
