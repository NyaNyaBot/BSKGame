# Entity 实体系统

## 分层架构

实体系统采用三层分离：

```
┌─────────────────────────────────────────────────┐
│  EntityLogic (Unity MonoBehaviour)               │ ← client/Assets/Game/Scripts/Gameplay/Entity/
│  处理 Unity 侧渲染/物理/输入/动画                  │
│  实现 ISelectable / IView / PointerHandler 等接口  │
├─────────────────────────────────────────────────┤
│  GameplayEntity (纯 C# 类)                       │ ← gameplay/gameplay/Entity/GameplayEntity/
│  领域逻辑：Position/Rotation 组件、伤害、死亡       │
│  持有 IEntity 引用，不依赖 Unity                   │
├─────────────────────────────────────────────────┤
│  IEntity / EntityManager (GameFramework 核心)     │ ← framework/game.core/GameFramework/Entity/
│  实体生命周期：Show/Hide/Attach/Detach             │
└─────────────────────────────────────────────────┘
```

## GameplayEntity 层级（gameplay/gameplay/Entity/）

### 基类
`GameplayEntity.cs` — 抽象基类
- 持有 `IEntity` 和 `EntityData`
- OnInit 时自动添加 `PositionComponent` 和 `RotationComponent`
- 提供 Kill / ReceiveDamage 方法（查找目标实体并调用 IDamageable）
- 生命周期回调：OnShow / OnUpdate / OnHide / OnRecycle / OnAttached / OnDetached

### 子类
- `BoardGameplayEntity` — 棋盘实体（ProceduralGraph/）
- `LatticeGameplayEntity` — 格子实体（ProceduralGraph/）
- `RoleGameplayEntity` — 角色实体（Role/）
- `RoleControllerGameplayEntity` — 角色控制器（RoleController/）
- `TileNodeGameplayEntity` — 瓦片地图节点（TileMapGraph/）
- `PlayerGameplayEntity` — 玩家实体

### 数据模型（Data/）
- `BoardEntityModel` / `LatticeEntityModel` / `RoleEntityModel` / `RoleControllerEntityModel`
- 各模型持有实体配置数据和运行时状态

### 组件（EntityComponent/）
- `PositionComponent` — 位置数据
- `RotationComponent` — 旋转数据
- 通过 `IEntity.AddComponent` / `GetComponent` 访问

### 属性（Property/）
- 属性定义与数组封装，用于角色数值系统

## EntityLogic 层级（client/.../Gameplay/Entity/EntityLogic/）

### 基类
`Base/` 下的实体逻辑基类，继承 GF 的 EntityLogic

### 接口
- `ISelectable` — 可选中接口
- `IView` — 视图同步接口
- `PointerHandler` — 输入指针处理

### 子类（与 GameplayEntity 一一对应）
- `ProceduralGraph/` — Board / Lattice 实体的 Unity 表现
- `Role/` — 角色实体（多 partial 文件：输入/伤害/可选中等）
- `RoleController/` — 角色控制器表现
- `TileMapGraph/` — 瓦片节点表现

### EntityLogicSocket（Entity/EntityLogicSocket/）
实体上的"插槽"式扩展组件：
- `Collider/` — 碰撞体挂载
- `MeshLoader/` — 网格加载器

## 关键约定

1. **域逻辑不引用 UnityEngine**：`GameplayEntity` 及其子类只引用 `game.core` 和 `game.gameplay`
2. **EntityLogic 负责视觉同步**：通过 `IView` 接口拉取 GameplayEntity 的数据来更新 Transform/动画等
3. **实体由 GF EntityManager 统一管理**：通过 `GameEntry.Entity.ShowEntity` / `HideEntity` 控制生命周期
4. **组件通过引用池管理**：`ReferencePool.Acquire<T>()` 获取，避免 GC
