# JS 桥接架构

## 入口文件链

```
game.js                          ← 小游戏入口
  ├── import weapp-adapter.js    ← 小程序 API 适配（window/document/navigator 模拟）
  ├── import events.js           ← 事件系统
  ├── import texture-config.js   ← 纹理配置
  ├── import unity-namespace.js  ← Unity/微信命名空间桥接
  ├── import webgl.wasm.framework.unityweb.js  ← Unity WebGL 框架（自动生成，勿改）
  ├── import unity-sdk/index.js  ← Unity SDK 能力合集
  ├── import check-version.js    ← 版本检查
  └── import plugin-config.js    ← 插件配置（launchEventType / scaleMode）
```

## game.js 启动流程

```javascript
1. 定义 managerConfig（MD5/CDN/分包/上下文配置等）
2. checkVersion() — 版本兼容性检查
3. requirePlugin('UnityPlugin') — 加载微信 Unity 插件
4. new UnityManager(managerConfig) — 创建管理器
5. gameManager.onLaunchProgress() — 监听启动各阶段
6. gameManager.onModulePrepared() — 模块就绪回调
   └── 同步 unityNamespace → GameGlobal
   └── 设置 assetPath
   └── 预加载微信系统字体
7. gameManager.startGame() — 启动 Unity WASM
```

## unity-namespace.js

项目级配置桥，将 Unity 编辑器的配置映射为 JS 运行时变量：

| 关键属性 | 说明 |
|---|---|
| bundleHashLength | Bundle hash 长度（8） |
| releaseMemorySize | 释放内存阈值（30MB） |
| unityVersion | 2022.3.17f1 |
| unityColorSpace | Gamma |
| maxStorage | 本地缓存上限（200MB） |
| texturesHashLength / texturesPath / needCacheTextures | 纹理缓存配置 |
| iOSAutoGCInterval | iOS GC 间隔（10000ms） |
| isDevelopmentBuild | 开发构建标记 |
| unityHeapReservedMemory | WASM 堆预留（256MB） |

关键方法：
- `isCacheableFile(path)` — 判断文件是否需要自动缓存（基于路径标识符）
- `isErasableFile(info)` — 清理缓存时是否可自动清理
- `isWXAssetBundle(path)` — 判断是否为 AssetBundle
- `isReportableHttpError(info)` — 是否上报网络异常

## unity-sdk/ 目录

按能力拆分的 JS 桥接模块：

```
unity-sdk/
├── index.js          # 合集入口
├── audio/            # 音频
├── touch/            # 触摸输入
├── video/            # 视频
├── font/             # 字体（含 preloadWxCommonFont）
├── texture.js        # 纹理（压缩纹理/缓存等）
├── mobileKeyboard/   # 移动端键盘
├── gyroscope/        # 陀螺仪
├── bluetooth/        # 蓝牙
├── TCPSocket/        # TCP
└── UDPSocket/        # UDP
```

## 全局对象

- `GameGlobal` — 全局命名空间，承载 managerConfig / unityNamespace / WXWASMSDK / manager 等
- `GameGlobal.events` — 事件总线
- `GameGlobal.realtimeLogManager` — 微信实时日志
- `GameGlobal.logmanager` — 微信用户反馈日志
