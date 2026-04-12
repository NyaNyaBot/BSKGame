---
name: bsk-wx-minigame
description: BSKGame 微信小游戏 domain。覆盖微信 WASM 小游戏导出全流程（MiniGameConfig → wxProject）、JS 桥接层（game.js / unity-namespace.js / unity-sdk/）、分包策略（wasmcode / data-package）、CDN 资源配置、UnityPlugin 插件对接、加载页与启动流程、WebGL 构建输出。当涉及微信小游戏、wxProject、小程序、WASM、WebGL 导出、CDN、分包、UnityPlugin、unity-namespace、game.js、minigame 等关键词时触发。
---

# BSKGame 微信小游戏

## 概览

项目通过微信官方的 **Unity WebGL 转微信小游戏** 方案发布。Unity 编辑器中使用 `WX-WASM-SDK-V2` 插件导出到 `wxProject/` 目录，生成的微信小游戏工程由 `minigame/` 目录承载。

## 关键路径

| 内容 | 路径 |
|---|---|
| Unity 侧导出配置 | `client/Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset` |
| 小游戏工程根 | `wxProject/minigame/` |
| 小游戏入口 | `wxProject/minigame/game.js` |
| 命名空间桥接 | `wxProject/minigame/unity-namespace.js` |
| Unity SDK 能力桥 | `wxProject/minigame/unity-sdk/` |
| 小游戏全局配置 | `wxProject/minigame/game.json` |
| 微信项目配置 | `wxProject/minigame/project.config.json` |
| 标准 WebGL 输出 | `wxProject/webgl/` 和 `build/webglbuild/` |

## 导出与启动流程

详见 `references/export-pipeline.md`。

## JS 桥接架构

详见 `references/js-bridge.md`。

## 分包策略

详见 `references/subpackage.md`。

## 当前配置摘要

- **AppID**: wxdfe72aa8c7017a1c
- **CDN**: https://a.unity.cn/client_api/v1/buckets/.../release_by_badge/Dev/content/
- **Unity 版本**: 2022.3.17f1
- **UnityPlugin 版本**: 1.2.91
- **内存预留**: 256 MB
- **本地缓存上限**: 200 MB
- **设备方向**: 横屏 (landscape)
- **iOS 高性能模式**: 开启
- **数据包加载方式**: 小游戏分包 + Brotli 压缩
- **WebGL 2.0**: 未开启

## 修改指南

- 修改 CDN 地址：编辑 `MiniGameConfig.asset` 的 `CDN` 字段，导出后会同步到 `game.js`
- 修改分包配置：编辑 `game.json` 的 `subpackages` 和 `parallelPreloadSubpackages`
- 修改加载页样式：编辑 `game.js` 中的 `loadingPageConfig`
- 添加预加载资源：编辑 `game.js` 中的 `preloadDataList`
- 修改缓存策略：编辑 `unity-namespace.js` 中的 `isCacheableFile` / `isErasableFile`
- 注意：`webgl.wasm.framework.unityweb.js` 是 Unity 导出产物（2w+ 行），不应手动修改
