# 导出流程

## Unity → 微信小游戏 全链路

```
1. Unity 编辑器
   │  WX-WASM-SDK-V2 插件
   │  配置文件: MiniGameConfig.asset
   │  ├── ProjectConf: 项目名/AppID/CDN/资源加载方式/内存/方向等
   │  ├── SDKOptions: 社交关系/压缩纹理/字体等开关
   │  ├── CompileOptions: DevelopBuild/Il2CppOptimize/Webgl2/fbslim/CleanBuild 等
   │  └── CompressTexture: 纹理压缩配置
   │
   │  点击导出
   ▼
2. WebGL Player 构建
   │  输出: wxProject/webgl/ (标准 WebGL)
   │  含: .wasm / .framework.js / .data / .loader.js
   ▼
3. 微信转换工具
   │  将 WebGL 产物转换为微信小游戏格式
   │  输出: wxProject/minigame/
   │  ├── 生成 game.js（入口，含 managerConfig）
   │  ├── 生成 unity-namespace.js（配置桥）
   │  ├── 生成 webgl.wasm.framework.unityweb.js（框架 JS）
   │  ├── 生成 wasmcode/ 分包（压缩后的 wasm）
   │  ├── 生成 data-package/ 分包（压缩后的 data）
   │  └── 同步 game.json / project.config.json
   ▼
4. 微信开发者工具
   │  打开 wxProject/minigame/ 目录
   │  预览/上传
   ▼
5. 小游戏运行
```

## MiniGameConfig 关键字段

| 字段 | 值 | 说明 |
|---|---|---|
| projectName | BSKGame | 项目名 |
| Appid | wxdfe72aa8c7017a1c | 微信 AppID |
| CDN | https://a.unity.cn/...Dev/content/ | 资源 CDN 地址 |
| assetLoadType | 1 | 资源加载方式 |
| compressDataPackage | 1 | 启用 Brotli 压缩 |
| relativeDST / DST | /Users/.../wxProject | 导出目标路径（本地绝对路径，勿提交） |
| MemorySize | 256 | WASM 堆内存（MB） |
| Orientation | 1 | 横屏 |
| maxStorage | 200 | 本地缓存上限（MB） |
| bundleHashLength | 8 | Bundle 文件名 hash 长度 |
| DevelopBuild | 1 | 开发构建 |
| fbslim | 1 | 代码裁剪 |
| CleanBuild | 1 | 清除旧构建 |

## 注意事项

- `DST` / `relativeDST` 是本地绝对路径，每位开发者不同，不应提交到版本控制
- 导出后 `webgl.wasm.framework.unityweb.js` 由工具自动生成，手动修改会在下次导出时被覆盖
- `game.js` 中的 `managerConfig` 部分字段（如 DATA_FILE_MD5 / CODE_FILE_MD5 / DATA_FILE_SIZE）会在每次导出时更新
