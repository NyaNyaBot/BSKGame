# Unity 2022.3 LTS — Deprecated APIs Reference

> **Engine**: Unity 2022.3.17f1
> **Last Updated**: 2026-05-01
> **Source**: Unity 2022 LTS Upgrade Guide + official docs

## Deprecated in Unity 2022.3 (relevant to project)

| API / Feature | Status | Impact on BSKGame | Mitigation |
|---|---|---|---|
| Enlighten Baked Global Illumination | Deprecated | None — project uses Progressive Lightmapper | No action needed |
| `GraphicsFormat.DepthAuto` / `ShadowAuto` / `VideoAuto` | Deprecated (obsolete in 2023.2) | None — not used in project | Avoid in new code |
| Old Input Manager Physical Keys default | Changed default | Low — project uses touch input via WeChat bridge | No action needed |
| `NavMeshSurface` etc. (core → AI Navigation package) | Moved to package | None — project uses custom navigation/graph system | No action needed |
| Nested Prefab child reordering overrides | No longer supported | Low — review any nested prefab hierarchies | Verify prefab order on upgrade |

## APIs NOT Deprecated (confirmed safe for 2022.3)

| API | Status | Used by |
|---|---|---|
| `UnityEngine.EventSystems` / UGUI | Active | 输入系统触区 |
| `UnityEngine.UI` (legacy UGUI) | Active | UIFrame / Battle HUD |
| `UnityEngine.SceneManagement` | Active | 场景管理 |
| `NUnit` / Unity Test Framework | Active | EditMode / PlayMode 测试 |
| `Addressables` 1.x | Active | 资源加载（项目已引入） |
| `Application.targetFrameRate` | Active | WebGL 帧率控制 |
| `Input.GetTouch` / `Input.touchCount` | Active | 触摸输入采样 |
| `Screen.safeArea` | Active | 安全区域检测 |

## WebGL-Specific Deprecated / Unavailable APIs

| API | Status | BSKGame Impact |
|---|---|---|
| `System.Threading.Thread` | Unavailable on WebGL | 项目不使用多线程；纯 C# 逻辑为同步 |
| `System.IO.File` (full) | Limited on WebGL | 存档系统（Alpha）需使用 PlayerPrefs 或 IndexedDB |
| `DynamicMethod` / `Expression.Emit` | Unavailable on AOT/WebGL | ADR-0003 已禁止；不影响 |
| `GC.Collect()` forced | Limited effectiveness | 避免在战斗循环中调用 |

## HybridCLR-Specific Considerations

| Concern | Status | ADR Coverage |
|---|---|---|
| Generic method AOT preservation | Must preserve `Publish<T>` / `Subscribe<T>` call sites | ADR-0002, ADR-0003 |
| Reflection-based construction on AOT | Limited — avoid `Activator.CreateInstance` for unreferenced generics | ADR-0003 |
| Assembly boundary for hot-update | gameplay.dll + Unity hot-update; Launch/AOT no combat logic | ADR-0003 |

## Project Decision

本项目不使用上述已弃用 API。所有 ADR 的 "Engine Compatibility" 章节均已确认无 post-cutoff 或 deprecated API 依赖。Gate check 可标记此文件为 "已审计"。
