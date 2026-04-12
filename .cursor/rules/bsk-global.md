# BSKGame 全局规则

## 语言
始终使用中文（简体）回复。

## 项目认知入口
开始任何涉及本项目代码的任务前，先阅读 `.agents/skills/bsk-project-map/SKILL.md`，根据任务类型选择对应的 domain skill 深入了解上下文。

## 禁止修改区域
- `client/Assets/GameRes/DLL/*.bytes` — 这些是 HybridCLR 编译产物，由 `CopyDLL.bat` 生成，不可手动编辑
- `client/Assets/3rdPart/` — 第三方库源码（GameFramework / ParadoxNotion / QFramework），除非用户明确要求
- `wxProject/minigame/webgl.wasm.framework.unityweb.js` — Unity 导出的框架胶水，体量 2w+ 行，不应手动修改

## 编码风格
- C# 代码遵循项目已有的命名空间约定：`Game.Client`（客户端）、`Game.Gameplay`（玩法 gameplay 库）、`Game.Core`（core 包运行时）、`Game.UIFramework`（UI 框架包）
- Unity 编辑器扩展放 `client/Assets/Editor/`，运行时代码放 `client/Assets/Game/`
- framework / gameplay 两个外部 C# 工程修改后需要重新编译并通过 `CopyDLL.bat` 同步 DLL

## 提交约定
- 忽略 `.DS_Store`、`TextToolDatas/log/` 目录下的日志文件
- 提交前确认不包含本地绝对路径（如 MiniGameConfig.asset 中的 `DST` / `relativeDST`）
