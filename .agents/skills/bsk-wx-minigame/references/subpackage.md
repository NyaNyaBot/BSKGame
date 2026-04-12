# 分包策略

## 概述

微信小游戏对主包大小有严格限制（通常 20MB），BSKGame 使用**小游戏分包**将大体积的 WASM 代码和数据包拆分加载。

## 当前分包配置

配置文件: `wxProject/minigame/game.json`

```json
{
  "subpackages": [
    { "name": "wasmcode", "root": "wasmcode/" },
    { "name": "data-package", "root": "data-package/" }
  ],
  "parallelPreloadSubpackages": [
    { "name": "wasmcode" },
    { "name": "data-package" }
  ]
}
```

### wasmcode 分包
路径: `wxProject/minigame/wasmcode/`
内容: Brotli 压缩后的 WebAssembly 代码
- 包含独立的 `game.js`（分包入口）
- 压缩后的 `.wasm.br` 文件

### data-package 分包
路径: `wxProject/minigame/data-package/`
内容: Brotli 压缩后的游戏数据
- 包含独立的 `game.js`（分包入口）
- 压缩后的 `.data.br` 文件

## 加载策略

1. **并行预加载**: `parallelPreloadSubpackages` 配置了 wasmcode 和 data-package，微信客户端会在启动时并行下载这两个分包
2. **从分包加载**: `game.js` 中 `loadDataPackageFromSubpackage: true` 表示资源包从分包加载而非 CDN
3. **Brotli 压缩**: `compressDataPackage: true` 启用 Brotli 压缩，减小下载体积

## CDN 回退

当分包中没有所需资源时，会从 CDN 加载：
- CDN 地址: `managerConfig.DATA_CDN`
- StreamingAssets 路径: `DATA_CDN + streamingUrlPrefixPath + StreamingAssets`

## 缓存机制

由 `unity-namespace.js` 控制：
- `isCacheableFile()` — 路径包含 `StreamingAssets` 或 `.dat` 且不包含 `json` 的文件会自动缓存
- `isErasableFile()` — 控制缓存清理时哪些文件不可被自动清理
- `WXAssetBundles` Map — 注册的 AssetBundle 不会被自动清理
- `maxStorage: 200` MB — 本地缓存上限

## Workers

配置: `"workers": "workers"`
路径: `wxProject/minigame/workers/`

Workers 用于在独立线程中执行计算密集任务，当前包含 `response/` 子目录。

## 修改指南

- 调整分包体积上限：联系微信平台调整配额
- 添加新分包：在 `game.json` 的 `subpackages` 中添加条目，创建对应目录
- 修改缓存策略：编辑 `unity-namespace.js` 的缓存判断函数
- 修改 CDN 地址：通过 `MiniGameConfig.asset` 修改后重新导出
