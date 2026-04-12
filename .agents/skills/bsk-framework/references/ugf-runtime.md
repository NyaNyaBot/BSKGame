# UnityGameFramework 运行时

## 位置

路径: `client/Assets/3rdPart/GameFramework/`

## 职责

将 `game.core` 中的纯 C# Manager 接口包装为 Unity MonoBehaviour Component，由 `GameEntry` 统一创建和访问。

## 结构

```
3rdPart/GameFramework/
├── Scripts/
│   ├── Runtime/              # Unity 侧运行时封装
│   │   ├── Base/             # BaseComponent / GameEntry（GF 原生入口）
│   │   ├── Config/ DataTable/ Entity/ Event/ Fsm/ Localization/
│   │   ├── Network/ ObjectPool/ Procedure/ Resource/
│   │   ├── Scene/ Setting/ Sound/ UI/ WebRequest/
│   │   ├── Debugger/         # 运行时调试器（各种 Info Window）
│   │   ├── ReferencePool/    # 引用池严格检查
│   │   └── Download/ FileSystem/
│   └── Editor/               # 编辑器工具
│       ├── Inspector/        # 各 Component 的自定义 Inspector
│       ├── ResourceBuilder/  # 资源构建器
│       ├── ResourceAnalyzer/ # 资源分析器
│       ├── ResourceCollection/ # 资源收集
│       └── ResourcePackBuilder/ # 资源包构建
└── Libraries/                # 附带的测试/参考源码
```

## 与 game.core 的关系

```
GameFramework (game.core)        UnityGameFramework (3rdPart)
─────────────────────            ────────────────────────────
IEntityManager                ←  EntityComponent : MonoBehaviour
IProcedureManager             ←  ProcedureComponent
IResourceManager              ←  ResourceComponent
IUIManager                    ←  UIComponent
...                               ...
```

每个 `*Component` 在 `Awake` 时向 `GameFrameworkEntry` 注册对应的 Manager 实现。

## 业务侧 GameEntry

项目的业务 GameEntry 位于 `client/Assets/Game/Scripts/Common/GameEntry/`：
- `GameEntry.cs` — partial 类，Start 时调用 InitBuiltinComponents + InitCustomComponents
- 其他 partial 文件提供快捷访问：`GameEntry.Entity` / `GameEntry.Event` / `GameEntry.Resource` 等

## 与 BSK Package 的关系

`3rdPart/GameFramework` 是 UGF 原版源码（以 Vendor 方式放入），`Packages/com.game.framework.bsk.core` 是 BSK 自己的封装层，通过 asmdef 引用 UGF 运行时程序集。
