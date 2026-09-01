# 乐园派对

一款 3D 动作冒险游戏。玩家操控角色在类开放世界关卡中，通过移动、跳跃、滑翔、攀爬、悬挂、爬杆、滑轨、冲刺、攻击等丰富动作，克服障碍、收集金币并到达终点。演示视频场景仅为展示游戏功能搭建场景。
链接: https://pan.baidu.com/s/1Zqg8D5y-_LMRGH2rn8T2Tw?pwd=qfa6 提取码: qfa6


> 基于 Unity 2023.2 开发，核心是一套自研的泛型 + CRTP 组件化有限状态机框架，将角色、怪物、相机、存档、UI 等系统统一在事件驱动架构下。

---

## 技术栈

| 类别 | 技术 |
| --- | --- |
| 引擎 | Unity 2023.2（URP 渲染管线） |
| 语言 | C# |
| 相机 | Cinemachine（3rd Person Follow） |
| 输入 | Input System（InputActionAsset） |
| 数据配置 | ScriptableObject |
| 序列化 | BinaryFormatter / JsonUtility / PlayerPrefs |
| 设计模式 | 状态模式、单例、观察者（事件）、反射 |

---

## 核心框架（架构亮点）

### 1. 泛型 + CRTP 组件化有限状态机

```
MonoBehaviour
├── EntityBase                    纯运动/物理基类（碰撞、加速、重力、贴地…）
│   └── Entity<T> : EntityBase     CRTP 泛型实体，持有状态机组件
│       ├── Player : Entity<Player>
│       └── Enemy  : Entity<Enemy>
│
EntityState<T>                    纯 C# 状态对象（非 MonoBehaviour）
├── PlayerState  └── 20 个玩家状态
└── EnemyState   └── 3 个敌人状态

EntityStateManager<T>             FSM 核心（List + Dictionary 索引状态）
├── PlayerStateManager
└── EnemyStateManager
```

- **状态与实体分离**：状态是纯 C# 对象，只实现 `OnEnter / OnStep / OnExit / OnContact` 四个生命周期；实体负责物理与运动，状态负责决策与行为。
- **CRTP 泛型**（`Entity<T> where T : Entity<T>`）：让子类拿到自身具体类型，状态内可直接访问 `player.stats`、`enemy.player` 等强类型字段，避免装箱与强制转换。
- **状态机核心** `EntityStateManager<T>`：
  - `List<EntityState<T>>`（有序，下标即状态 ID）+ `Dictionary<Type, EntityState<T>>`（按类型索引）
  - `Change<TState>()` / `Change(int)` / `Change(EntityState<T>)` 三种切换方式，切换时广播 `onExit / onEnter / onChange` 事件
- **反射实例化**：状态类名以字符串存于 Inspector，运行时用 `Activator.CreateInstance` 反射创建（`CreateListFromStringArray`）。
- **Inspector 下拉配置**：自定义 `PropertyAttribute`（`ClassTypeName`）+ `PropertyDrawer`（`ClassTypeNameDrawer`），反射遍历所有状态子类生成下拉列表——策划无需改代码即可增删状态。

### 2. 状态机与 Animator 联动

- `PlayerAnimator` / `EnemyAnimator` 把**当前状态在数组中的下标**写入 Animator 整数参数 `State`，Animator Controller 按编号选择对应动画。
- 额外写入 `Last State / Lateral Speed / Vertical Speed / Jump Counter / Is Grounded` 等参数，驱动动画混合与速度缩放。
- `On State Changed` Trigger + 可配置的 `forcedTransitions`（强制过渡表）处理打断与强制切换。

### 3. 事件驱动（观察者模式）

- 基于 UnityEvent 的事件层：`EntityEvents`（落地/离地/上轨/离轨）、`PlayerEvents`（跳跃/受伤/死亡/踩踏/抓边/冲刺…）、`EnemyEvents`（发现玩家/追击/受伤/死亡）、`EntityStateManagerEvents`（状态进入/退出/切换）。
- 桥接组件 `EntityStateManagerListener` / `PlayerEventListener` 把内部事件转发到 Inspector，音效/粒子/脚步解耦订阅。

---

## 核心功能

### 角色动作系统（20 个状态）

| 分类 | 状态 |
| --- | --- |
| 基础移动 | Idle（闲置）、Walk（行走/奔跑）、Brake（刹车急停）、Crouch（下蹲）、Crawling（匍匐） |
| 空中动作 | Fall（空中核心）、Gliding（滑翔）、Dash（冲刺）、Spin（旋转攻击）、Stomp（踩踏）、AirDive（俯冲）、Backflip（后空翻） |
| 攀爬/悬挂 | WallDrag（贴墙/蹬墙）、PoleClimbing（爬杆）、LedgeHanging（悬挂边缘）、LedgeClimbing（翻上平台） |
| 特殊 | RailGrind（轨道滑行）、Swim（游泳）、Hurt（受伤硬直）、Die（死亡） |

### 敌人 AI（3 个状态）

- `IdleEnemyState`（闲置）、`WaypointEnemyState`（沿路点巡逻，支持 Loop / PingPong / Once 三种模式）、`FollowEnemyState`（发现玩家后追击）。
- 敌人 `HandleSight()` 用 `OverlapSphereNonAlloc` 侦测玩家（`spotRange` 发现范围 / `viewRange` 丢失范围），`ContactAttack()` 做接触伤害与击退。

### 第三人称跟随相机（Cinemachine）

- 基于 `CinemachineVirtualCamera + Cinemachine3rdPersonFollow + CinemachineBrain`。
- **死区阈值 + 每帧限速平滑**：地面/空中分别配置上下死区，超出部分按最大速度截断，实现「带死区的限速跟随」，镜头不抖动、不漂移；支持手动/自动视角旋转。

### 存档系统（三种序列化）

- `GameSaver` 单例统一管理 5 个存档槽，支持 **二进制 / JSON / PlayerPrefs** 三种方式切换。
- 存：重试次数、各关卡解锁状态、金币、通关时间、星级收集、时间戳。

### 属性配置（ScriptableObject）

- `EntityStats<T>` + `EntityStatsManager<T>`：玩家/怪物属性做成 ScriptableObject，Inspector 配置多套数值、`Change(int)` 切换；字段覆盖移动、跳跃、冲刺、滑翔、游泳、攀爬、攻击等全部动作参数。

### 输入系统（Input System）

- `PlayerInputManager` 封装 InputActionAsset，统一处理键鼠与手柄。
- **十字死区**（`GetAxisWithCrossDeadZone` + 重映射）、**跳跃缓冲**（0.15s，`GetJumpDown`）、移动方向相对相机朝向的旋转（`GetMovementCameraDirection`）。

### UI 系统

- loading 界面、主菜单、关卡列表、存档列表、HUD（血条/金币/计时/星级）、暂停、关卡结算。
- 协程实现淡入淡出（`Fader.FadeOut/In`、`Flash` 全屏白闪）、`GameLoader` 协程异步加载场景。

### 关卡元素交互（20+ 种）

- 沼泽/泳池（`Volume` + 浮力/减速）、危险物（`Hazard`）、可破坏物（`Breakable`）、滑翔翼（`Glider`）、检查点（`Checkpoint`）、传送门（`Portal`）、攀爬柱（`Pole`）、弹簧（`Spring`）、可收集物（`Collectable`/`Star`）、可拾取/投掷物（`Pickable`）、道具箱（`ItemBox`）、移动/掉落平台、压力板（`Panel`）、触发器（`Toggle`）等，统一通过 `IEntityContact` 接口实现双向交互。

---

## 亮点实现细节（关键函数）

| 函数 | 位置 | 亮点 |
| --- | --- | --- |
| `EntityBase.Accelerate` | `Entity/Entity.cs` | 带转向阻力的水平加速，动作手感核心 |
| `EntityBase.SphereCast / CapsuleCast / OverlapEntity` | `Entity/Entity.cs` | 统一碰撞/形状检测原语，各状态大量复用 |
| `EntityStateManager<T>.Change<TState>` | `Entity/EntityStateManager.cs` | 状态切换，广播进入/退出事件 |
| `EntityState.CreateListFromStringArray` | `Entity/EntityState.cs` | 反射实例化状态 |
| `ClassTypeNameDrawer` | `Tools/Editor/ClassTypeNameDrawer.cs` | Inspector 下拉配置状态，免改代码 |
| `PlayerCamera.HandleOffset` | `Player/PlayerCamera.cs` | 死区 + 限速平滑跟随 |
| `PlayerCamera.HandleOrbit / HandleVelocityOrbit` | `Player/PlayerCamera.cs` | 手动/自动视角旋转 |
| `PlayerInputManager.GetAxisWithCrossDeadZone` | `Player/PlayerInputManager.cs` | 十字死区 + 重映射 |
| `PlayerInputManager.GetJumpDown` | `Player/PlayerInputManager.cs` | 跳跃缓冲 |
| `GameSaver.SaveBinary / SaveJSON / SavePlayerPrefs` | `Game/GameSaver.cs` | 三种序列化存档 |
| `EntityHitbox.HandleEntityAttack / Rebound / PushBack` | `Entity/EntityHitbox.cs` | 攻击判定、反弹、击退 |
| `GameLoader.LoadRoutine` | `Game/GameLoader.cs` | 协程异步加载 + loading 过渡 |

---

## 目录结构

```
Assets/PLAYER TWO/Platformer Project/Scripts/
├── Entity/       通用实体基类、命中盒、状态机、事件
├── Player/       玩家、状态机、相机、输入、动画、音效、粒子、20 个状态
├── Enemy/        敌人、状态机、动画、音效、3 个状态
├── Game/         存档、游戏管理器、异步加载
├── Level/        关卡分数、暂停、复活、开始/结束
├── Maic/         20+ 种可交互关卡元素、单例、淡入淡出
├── UI/           HUD、UI 动画、关卡/存档列表
├── WayPoint/     路径点
├── Interfaccs/   交互接口 IEntityContact
└── Tools/        自定义 Inspector 属性
```

---

