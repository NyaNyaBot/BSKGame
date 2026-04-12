# game.core 模块地图

## 工程信息

路径: `framework/game.core/`
工程文件: `game.core.csproj`
目标框架: `netstandard2.1`
语言版本: C# 11
根命名空间: `bsk.game`
允许 unsafe: 是
条件编译宏: `WEIXINMINIGAME` 始终定义
外部引用: `Game.Math.dll` / `Newtonsoft.Json.dll`（从 `Library/` 目录引用）
PostBuild: 执行 `CopyDLL.bat` 将产物拷贝到 Unity 工程

## 模块目录

### GameFramework/（Starforce 核心）

| 子目录 | 模块 | 核心接口/类 |
|---|---|---|
| Base/ | 框架入口/事件池/引用池/任务池/序列化 | GameFrameworkEntry / ReferencePool |
| Procedure/ | 流程管理 | IProcedureManager / ProcedureBase |
| Fsm/ | 有限状态机 | IFsmManager / Fsm<T> |
| Entity/ | 实体管理 | IEntityManager / IEntity |
| Resource/ | 资源加载/版本列表/更新器 | IResourceManager / LocalVersionList |
| DataTable/ | 数据表 | IDataTableManager / IDataRow |
| UI/ | UI 管理 | IUIManager |
| Event/ | 全局事件 | IEventManager |
| Network/ | 网络 | INetworkManager |
| ObjectPool/ | 对象池 | IObjectPoolManager / IObjectPool<T> |
| Download/ | 下载 | IDownloadManager |
| Scene/ | 场景 | ISceneManager |
| Sound/ | 音频 | ISoundManager |
| Config/ | 配置 | IConfigManager |
| Localization/ | 本地化 | ILocalizationManager |
| WebRequest/ | Web 请求 | IWebRequestManager |
| Setting/ | 设置持久化 | ISettingManager |
| Debugger/ | 调试器 | IDebuggerManager |
| FileSystem/ | 文件系统 | IFileSystemManager |
| DataNode/ | 数据节点树 | IDataNodeManager / IDataNode |
| Utility/ | 工具类 | Utility.Text / Utility.Json / etc. |

### 扩展模块

| 目录 | 职责 |
|---|---|
| AI/ReGoap/ | GOAP AI 规划器 |
| Pool/ | 集合池（ListPool / DictionaryPool / Wrapper） |
| Graph/ | 通用图结构（ArrayGraph / GraphExtension） |
| GameplayTag/ | Gameplay 标签系统 |
| Common/ | PriorityQueue 等通用数据结构 |
| Debug/ Log/ | 调试颜色 / 日志工具 |
| Defination/ | 项目常量（Entity/UIGroup）和 AssetUtility |
| Extension/ | DataTable / BinaryReader 等扩展方法 |
| Time/ | ITimeUtility 时间工具接口 |
| QFramework/Interface/ | ILateUpdateable 等薄接口（与 QF 生态对接） |
| Resource/ | IWXHelper.cs 微信平台资源辅助 |

## 编译配置

| 配置 | 宏定义 | 用途 |
|---|---|---|
| Debug | LOG_DEBUG + LOG_WARNING + LOG_ERROR + LOG_PUBLISH + WEIXINMINIGAME | 开发 |
| Release | LOG_WARNING + LOG_ERROR + LOG_PUBLISH + WEIXINMINIGAME | 测试 |
| ReleaseNPublish | LOG_WARNING + LOG_ERROR + LOG_PUBLISH + SHIPPING_EXTERNAL + WEIXINMINIGAME | 发布 |
