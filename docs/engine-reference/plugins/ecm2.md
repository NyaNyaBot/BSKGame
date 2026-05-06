# ECM2 (Easy Character Movement 2) v1.4 — 项目使用参考

## 安装

`client/Assets/3rdPart/ECM2/v1.4/`

## 组件架构

```
Character GameObject
├── Character (component) — 角色逻辑：移动模式/跳跃/蹲伏/旋转
├── CharacterMovement (component) — 底层物理：碰撞/地面检测/移动
└── CapsuleCollider (自动同步 radius/height)
    └── Model (child) — 视觉模型 + Animator
```

## 核心 API

### 移动
```csharp
character.SetMovementDirection(Vector3 worldDir);
character.GetSpeed();           // 当前速度标量
character.GetVelocity();        // 当前速度向量
```

### 跳跃/蹲伏
```csharp
character.Jump();
character.StopJumping();
character.Crouch();
character.UnCrouch();
```

### 状态查询
```csharp
character.IsGrounded()      // 在可行走地面
character.IsFalling()       // 下落中
character.IsCrouched()      // 蹲伏中
character.GetMovementMode() // Walking/Falling/Flying/Swimming/Custom/None
```

### 移动模式
```csharp
MovementMode.None     // 禁用移动（战斗站位时用）
MovementMode.Walking  // 地面行走
MovementMode.Falling  // 受重力下落
MovementMode.Flying   // 不受重力
```

### 旋转模式
```csharp
character.rotationMode = RotationMode.OrientRotationToMovement; // 探索用
character.rotationMode = RotationMode.Custom;                    // 战斗用
```

## 与 Animancer 集成

推荐方式：程序驱动位移（非 Root Motion）

```csharp
// 在 CharacterAnimController 中读取 ECM2 状态驱动动画
float speed = _character.GetSpeed();
bool grounded = _character.IsGrounded();

if (speed > 0.1f && grounded)
    PlayLocomotion(speed);
else if (grounded)
    PlayIdle();
```

## 模拟时序

```
LateFixedUpdate (coroutine)
└── Simulate(deltaTime)
    ├── BeforeSimulationUpdate — 模式切换, Jump/Crouch
    ├── SimulationUpdate — 计算期望速度, 执行旋转
    ├── AfterSimulationUpdate — 事件触发
    └── CharacterMovementUpdate — 实际物理移动
```

## 本项目定位

- **探索模式**: Walking + OrientRotationToMovement, 玩家通过输入控制
- **战斗模式**: None (禁用移动), Custom 旋转 (面向对方), 仅重力贴地
