# Unity 2022.3 LTS — Breaking Changes Reference

> **Engine**: Unity 2022.3.17f1
> **Last Updated**: 2026-05-01
> **Source**: Unity 2022 LTS Upgrade Guide

## Breaking Changes in Unity 2022.3

| Change | Category | Impact on BSKGame | Action Required |
|---|---|---|---|
| `ArticulationDrive.forceLimit` accepts force instead of impulse | Physics | None — not used | None |
| Generated lightmap UV procedure changed | Lighting | Low — may need rebake | Rebake if using lightmaps |
| `GradientField` color picker no longer HDR by default | UI Toolkit (Editor) | None — not used in runtime | None |
| Physical Keys enabled by default in Input Manager | Input | None — project uses touch/pointer | None |
| Enlighten baking backend removed from default | Lighting | None — uses Progressive | None |
| Minimum Bounces property removed from Lighting window | Lighting | None | None |
| Gradle templates updated (Android) | Build | None — target is WebGL/WeChat | None |
| Navigation moved to AI Navigation package | Navigation | None — custom graph system | None |
| Nested Prefab child reordering no longer supported | Prefabs | Low — review prefab hierarchies | Check before upgrade |

## WebGL-Specific Breaking Changes

| Change | Impact | Mitigation |
|---|---|---|
| WebGL threading not supported | Cannot use `Task.Run` / `Thread` | All logic synchronous — confirmed |
| WebGL memory model limited | 256MB practical ceiling | ADR performance budgets set |
| AOT code generation constraints | No `Reflection.Emit` | ADR-0003 prohibits |

## HybridCLR Integration Breaking Points

| Concern | Risk | Mitigation |
|---|---|---|
| Generic preservation across hot-update boundary | HIGH | Maintain AOT generic reference list; ADR-0002/0003 |
| Assembly load order on domain reload | MEDIUM | Launch procedure sequence tested; bsk-launch skill |
| Metadata supplement for hot-update assemblies | MEDIUM | CopyDLL.bat workflow; CI smoke test |

## Summary

BSKGame 不受 Unity 2022.3 breaking changes 的直接影响。项目关键风险在于 WebGL/HybridCLR 平台约束而非引擎版本升级变更，这些已通过 ADR-0003 和性能预算覆盖。
