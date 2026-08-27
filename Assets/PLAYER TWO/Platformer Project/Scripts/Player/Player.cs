using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Cinemachine.CinemachineOrbitalTransposer;
using static UnityEngine.ParticleSystem;

public class Player : Entity<Player>
{

    public PlayerEvents playerEvents;
    public PlayerInputManager inputs { get; protected set; }

    public PlayerStatsManager stats { get; protected set; }

    public int jumpCounter { get; protected set; }// 当前跳跃计数器，用于多段跳和土狼跳判定

    public int airDashCounter { get; protected set; } // 当前空中冲刺计数器，用于限制空中冲刺次数   
    public float lastDashTime { get; protected set; } // 上一次冲刺的时间，用于地面冲刺冷却判定
    public int airSpinCounter { get; protected set; } // 当前空中旋转计数器，用于限制空中旋转次数
    public Vector3 lastWallNormal { get; protected set; }// 上次蹬墙跳的方向

    public virtual bool isAlive => !health.isEmpty;

    // 捡起箱子的父节点
    public Transform pickSlot;
    // 捡起的箱子是谁
    public Pickable pickable { get; protected set; }

    public Pole pole { get; protected set; }
    public bool onWater { get; protected set; }
    public Health health { get; protected set; }
    public bool holding { get; protected set; }

    protected const float k_waterExitOffset = 0.25f; // 用于检测玩家是否离开水面的偏移量

    public Collider water { get; protected set; } // 当前玩家所在的水体碰撞体   

    // 皮肤初始位置与旋转（用于恢复外观）
    protected Vector3 m_skinInitialPosition=Vector3.zero;
    protected Quaternion m_skinInitialRotation=Quaternion.identity;

    public Transform skin;// 玩家模型的 Transform，用于调整模型位置和旋转
    private Vector3 m_respawnPosition;
    private Quaternion m_respawnRotation;

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
        InitializeStats();
        InitializeHealth();
        InitializeTag();
        InitializeRespawn();

        entityEvents.OnGroundEnter.AddListener(() => 
        {   ResetJumps(); 
            ResetAirDash();
            ResetAirSpin();
        });

        entityEvents.OnRailsEnter.AddListener(() =>
        {
            ResetAirDash();
            ResetJumps();
            ResetAirSpin();
            StartGrind();
        });
    }

    public virtual void StartGrind() => states.Change<RailGrindPlayerState>();

    protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();

    protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();

    protected virtual void InitializeHealth() => health = GetComponent<Health>();

    protected virtual void InitializeTag() => tag = GameTags.Player;

    // 初始化重生点
    protected virtual void InitializeRespawn()
    {
        m_respawnPosition = transform.position;
        m_respawnRotation = transform.rotation;
    }

    public virtual void Accelerate(Vector3 direction)
    {
        // 根据是否按下 Run 键、是否在地面，决定不同的转向阻尼与加速度
        var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
        var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
        var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration; // 空中与地面不同
        var topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;


        // 调用底层 Accelerate(方向，转向阻尼，加速度，最大速度)
        Accelerate(direction, turningDrag, finalAcceleration, topSpeed);
    }

    /// <summary>
    /// 玩家复活：重置生命值、位置、旋转，并切换到 Idle 状态
    /// </summary>
    public virtual void Respawn()
    {
        health.Reset();   // 重置生命
        transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation); // 回到重生点
        states.Change<IdlePlayerState>(); // 状态机切换为待机
    }
    public virtual void SetRespawn(Vector3 position, Quaternion rotation)
    {
        m_respawnPosition = position;
        m_respawnRotation = rotation;
    }
    /// <summary>
    /// 在指定方向上平滑移动玩家（匍匐状态的参数）
    /// </summary>
    public virtual void CrawlingAccelerate(Vector3 direction) =>
        Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);

    /// <summary>
    /// 在空翻动作中平滑移动玩家（后空翻参数）
    /// </summary>
    public virtual void BackflipAcceleration()
    {
        var direction = inputs.GetMovementCameraDirection();
        Accelerate(direction,
            stats.current.backflipTurningDrag,
            stats.current.backflipAirAcceleration,
            stats.current.backflipTopSpeed);
    }

    /// <summary>
    /// 进入水中（切换到游泳状态）
    /// </summary>
    /// <param name="water">水的碰撞体</param>
    public virtual void EnterWater(Collider water)
    {
        // 不在水里 且 角色存活（血量不为空）
        if (!onWater && !health.isEmpty)
        {
            //Throw(); // 丢掉手上的拾取物
            onWater = true;
            this.water = water;
            states.Change<SwimPlayerState>(); // 切换游泳状态
        }
    }
    /// <summary>
    /// 在指定方向上平滑移动玩家（水下的参数）
    /// </summary>
    public virtual void WaterAcceleration(Vector3 direction) =>
        Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);

    /// <summary>
    /// 平滑朝向某个方向旋转（水中旋转速度）
    /// </summary>
    public virtual void WaterFaceDirection(Vector3 direction) => FaceDirection(direction, stats.current.waterRotationSpeed);
    /// <summary>
    /// 离开水域
    /// </summary>
    public virtual void ExitWater()
    {
        if (onWater)
        {
            onWater = false;
        }
    }

    /// <summary>
    /// 触发检测（玩家停留在触发器内）
    /// 用于检测是否进入水体或离开水体
    /// </summary>
    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.VolumeWater))
        {
            // 如果当前不在水中，但进入了水体包围盒
            if (!onWater && other.bounds.Contains(unsizedPosition))
            {
                EnterWater(other);
            }
            // 如果已经在水中，则检测是否离开
            else if (onWater)
            {
                // 计算一个向下偏移点，判断是否离开水面
                var exitPoint = position + Vector3.down * k_waterExitOffset;

                if (!other.bounds.Contains(exitPoint))
                {
                    ExitWater();
                }
            }
        }
    }
    /// <summary>
    /// 滑翔（下落时减缓下落速度）
    /// </summary>
    public virtual void Glide()
    {
        if (!isGrounded && inputs.GetGlide() && verticalVelocity.y <= 0 && stats.current.canGlide)
            states.Change<GlidingPlayerState>();
    }

    public virtual void WallDrag(Collider other)
    {
        // 如果属性支持，在掉落中，手里没拿东西，碰到的物体不是刚体
        if (stats.current.canWallDrag && velocity.y <= 0 && !holding && !other.TryGetComponent<Rigidbody>(out _))
        {
            // 如果 向前发射胶囊体射线，检测是是规定layer中的东西 且 没有检测到边缘
            if (CapsuleCast(transform.forward, 0.25f, out var hit, stats.current.wallDragLayers)&& !DetectingLedge(0.25f, height, out _))
            {
                // 如果碰到的物体时platform
                if (hit.collider.CompareTag(GameTags.Platform))
                {
                    // 设置父对象
                    transform.parent = hit.transform;
                }
                // 记录法线方向
                lastWallNormal = hit.normal;
                states.Change<WallDragPlayerState>();
            }
        }
    }

    public virtual void GrabPole(Collider other)
    {
        if (stats.current.canPoleClimb && velocity.y <= 0 && !holding && other.TryGetComponent(out Pole pole))
        {
            this.pole = pole;
            states.Change<PoleClimbingPlayerState>();
        }
    }

    /// <summary>
    /// 设置皮肤模型的父物体（比如挂在某个武器或挂点上）
    /// </summary>
    public virtual void SetSkinParent(Transform parent)
    {
        if (skin)
        {
           skin.parent = parent;
        }
    }
    /// <summary>
    /// 重置皮肤的父物体（回到玩家本体，恢复初始位置和旋转）
    /// </summary>
    public virtual void ResetSkinParent()
    {
        if (skin)
        {
            skin.parent = transform;
            skin.localPosition = m_skinInitialPosition;
            skin.localRotation = m_skinInitialRotation;
        }
    }

    public virtual void PickAndThrow()
    {
        // 如果 能捡起东西 且 检测到按下拾取键
        if (stats.current.canPickUp && inputs.GetPickAndDropDown())
        {
            // 如果没有捡起东西
            if (!holding)
            {
                // 检测自己前方拾取距离内是否有东西
                if (CapsuleCast(transform.forward, stats.current.pickDistance, out var hit))
                {
                    // 如果有东西看看是否是可以捡起的东西
                    if (hit.transform.TryGetComponent(out Pickable pickable))
                    {
                        // 如果是就捡起
                        PickUp(pickable);
                    }
                }
            }
            else // 如果捡起了东西就丢掉
            {
                Throw();
            }
        }
    }

    public virtual void PickUp(Pickable pickable)
    {
        // 如果没有在捡起东西 且 （在地面上 或 可以在空中捡起东西）
        if (!holding && (isGrounded || stats.current.canPickUpAir))
        {
            // 捡起
            holding = true;
            this.pickable = pickable;
            // 调用物品的被捡起函数
            pickable.PickUp(pickSlot);
            // 捡起的东西要重新生成时，需要给它加一个监听函数
            pickable.onRespawn.AddListener(RemovePickable);
            playerEvents.OnPickUp?.Invoke();
        }
    }

    public virtual void RemovePickable()
    {
        // 如果在捡起的状态下箱子重新生成了，所以就要把手里的东西置空
        if (holding)
        {
            pickable = null;
            holding = false;
        }
    }

    public virtual void Throw()
    {
        // 拿着东西的时候才能扔
        if (holding)
        {
            // 扔要有个力，力的大小和人物的速度有关
            var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
            // 调用物品的释放方法
            pickable.Release(transform.forward, force);
            pickable = null;
            holding = false;
            playerEvents.OnThrow?.Invoke();
        }
    }

    public virtual void DirectionalJump(Vector3 direction, float height, float distance)
    {
        // 蹬墙跳了，次数+1
        jumpCounter++;
        // 垂直速度增加
        verticalVelocity = Vector3.up * height;
        // 水平速度按照方向增加
        lateralVelocity = direction * distance;
        playerEvents.OnJump?.Invoke();
    }

    /// <summary>
    /// 推动刚体（例如推动箱子）
    /// </summary>
    public virtual void PushRigidbody(Collider other)
    {
        // 排除台阶上方的情况，检测目标是否是刚体
        if (!IsPointUnderStep(other.bounds.max) &&
            other.TryGetComponent(out Rigidbody rigidbody))
        {
            // 推动力量与玩家的水平速度相关
            var force = lateralVelocity * stats.current.pushForce;
            // 模拟物理推力（除以质量，乘上deltaTime）
            rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
        }
    }

    /// <summary>
    /// 空中俯冲（下劈攻击）
    /// </summary>
    public virtual void AirDive()
    {
        // 必须允许空中俯冲，且不在地面上，没有拿物品，按下了空中俯冲按键
        if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
        {
            states.Change<AirDivePlayerState>(); // 切换到空中俯冲状态
            playerEvents.OnAirDive?.Invoke();
        }
    }

    /// <summary>
    /// 后空翻
    /// </summary>
    public virtual void Backflip(float force)
    {
        // 判断能否执行后空翻：开启后空翻功能 + 没有手持物体
        if (stats.current.canBackflip && !holding)
        {
            // 设置垂直向上起跳速度
            verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
            // 给人物自身向后的水平推力
            lateralVelocity = -transform.forward * force;
            // 切换状态机至后空翻专属状态
            states.Change<BackflipPlayerState>();
            // 触发后空翻事件，供动画、音效、特效监听
            playerEvents.OnBackflip.Invoke();
        }
    }

    public virtual void ResetAirDash() => airDashCounter = 0;

    public virtual void ResetAirSpin() => airSpinCounter = 0;   

    /// <summary>
    /// 冲刺（包括地面冲刺和空中冲刺）
    /// </summary>
    public virtual void Dash()
    {
        // 是否可以空中冲刺
        var canAirDash = stats.current.canAirDash && !isGrounded &&
            airDashCounter < stats.current.allowedAirDashes;

        // 是否可以地面冲刺（冷却结束）
        var canGroundDash = stats.current.canGroundDash && isGrounded &&
            Time.time - lastDashTime > stats.current.groundDashCoolDown;

        // 如果按下冲刺键，且符合条件
        if (inputs.GetDashDown() && (canAirDash || canGroundDash))
        {
            if (!isGrounded) airDashCounter++; // 空中冲刺计数+1
            lastDashTime = Time.time; // 记录冲刺时间
            states.Change<DashPlayerState>(); // 切换到冲刺状态
        }
    }

    /// <summary>
    /// 执行旋转动作（Spin）
    /// </summary>
    public virtual void Spin()
    {
        // 空中旋转条件：允许空中旋转 && 未超过上限
        var canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;

        // 满足旋转条件 + 没有持物 + 按下旋转键
        if (stats.current.canSpin && canAirSpin && !holding && inputs.GetSpinDown())
        {
            if (!isGrounded)
            {
                airSpinCounter++; // 空中旋转次数 +1
            }

            states.Change<SpinPlayerState>(); // 切换到旋转状态
            playerEvents.OnSpin?.Invoke();    // 触发旋转事件
        }
    }
    // 边缘检测，两个参数是最大向前检测距离和最大向下检测距离，不在这个范围内说明不能成功扒在物体边缘
    protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
    {
        // 接触偏移量 是 默认接触偏移量 + 每帧位置移动量
        var contactOffset = Physics.defaultContactOffset + positionDelta;
        // 边缘最大距离 是 角色胶囊体半径 + 传入的向前检测最大距离
        var ledgeMaxDistance = radius + forwardDistance;
        // 边缘高度偏移 是 身高的一半 + 接触偏移量
        var ledgeHeightOffset = height * 0.5f + contactOffset;
        // 向上偏移 是 方向向上，长度为边缘高度偏移的向量
        var upwardOffset = transform.up * ledgeHeightOffset;
        // 向前偏移 是 方向向前，长度为边缘最大距离的向量
        var forwardOffset = transform.forward * ledgeMaxDistance;
        // 如果 从头顶向前方发射的射线检测到物体了（说明头顶还没高过物体边缘）
        if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
        // 或者 从 面前向上发射的射线检测到物体了（说明头顶上有东西挡住了）
        || Physics.Raycast(position + forwardOffset * 0.01f, transform.up, ledgeHeightOffset, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // 那就不能悬挂
            ledgeHit = new RaycastHit();
            return false;
        }
        // 否则 从 头顶前方 向下发射一条射线，检测物体的上面，能碰到就说明可以挂，碰不到就挂不了
        var origin = position + upwardOffset + forwardOffset;
        var distance = downwardDistance + contactOffset;

        return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance, stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
    }
    // 边缘悬挂的检测
    public virtual void LedgeGrab()
    {
        // 当 角色可以挂在边缘时 且 角色已经在向下掉落了 且 手里没拿东西
        if (stats.current.canLedgeHang && velocity.y < 0 && !holding
        // 且 状态机里面包含悬挂状态
        && states.ContainsStateOfType(typeof(LedgeHangingPlayerState))
        // 且 边缘检测成功
        && DetectingLedge(stats.current.ledgeMaxForwardDistance, stats.current.ledgeMaxDownwardDistance, out var hit))
        {
            // 如果 边缘检测返回的物体是胶囊形 或 球形 的碰撞体，那就不能抓在上面，要滑下去
            if (!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
            {
                // 否则就是能挂了
                // 边缘的最大距离 是 角色胶囊体半径 + 最大向前距离
                var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
                // 侧向偏移 是 角色面朝向方向，长度为ledgeDistance的向量
                var lateralOffset = transform.forward * ledgeDistance;
                // 纵向偏移 是 向下的方向，长度为身高的一半
                var verticalOffset = Vector3.down * height * 0.5f - center;
                // 速率归零
                velocity = Vector3.zero;
                // 父节点设置为要挂上去的物体，如果这个物体不是Platform，那就不设置父对象
                transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
                // 角色位置 移动到 射线监测点 减去 侧向偏移 加上 纵向偏移的位置
                // 这是将角色先挪到一个合适的位置，进入悬挂状态后要进行射线判断
                transform.position = hit.point - lateralOffset + verticalOffset;
                states.Change<LedgeHangingPlayerState>();
                playerEvents.OnLedgeGrabbed?.Invoke();
            }
        }
    }

    /// <summary>
    /// 踩踏攻击（从空中下踩敌人）
    /// </summary>
    public virtual void StompAttack()
    {
        if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
        {
            states.Change<StompPlayerState>();
        }
    }
    public virtual void Decelerate() => Decelerate(stats.current.deceleration);
    public virtual void Friction()
    {
        if (OnSlopingGround())
            Decelerate(stats.current.slopeFriction); // 在斜坡上使用斜坡摩擦
        else
            Decelerate(stats.current.friction);     // 普通摩擦
    }

    /// <summary>
    /// 根据相机方向来平滑移动玩家
    /// </summary>
    public virtual void AccelerateToInputDirection()
    {
        var inputDirection = inputs.GetMovementCameraDirection(); // 输入相对于相机的方向
        Accelerate(inputDirection);
    }

    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

    /// <summary>
    /// 施加重力，使玩家下落
    /// </summary>
    public virtual void Gravity()
    {
        
        if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed  = verticalVelocity.y;
            // 上升时用普通重力，下落时用更强的下落重力
            var force  = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
            speed -= force * gravityMultiplier * Time.deltaTime;

            // 限制最大下落速度
            speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }

    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

    /// <summary>
    /// 对玩家施加伤害
    /// </summary>
    /// <param name="amount">要扣除的生命值</param>
    /// <param name="origin">伤害来源位置（用于计算击退方向）</param>
    public override void ApplyDamage(int amount, Vector3 origin)
    {
        if (!health.isEmpty && !health.recovering) // 确保玩家未死亡且不在恢复无敌状态
        {
            health.Damage(amount); // 扣血
            var damageDir = origin - transform.position; // 计算受击方向
            damageDir.y = 0; // 忽略垂直方向
            damageDir = damageDir.normalized;
            FaceDirection(damageDir); // 面向攻击方向

            // 受伤时向后击退
            lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;

            if (!onWater) // 如果不在水中，则会被击飞向上并进入受伤状态
            {
                verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
                states.Change<HurtPlayerState>();
            }

            playerEvents.OnHurt?.Invoke(); // 触发受伤事件

            //如果血量空了->死亡
            if (health.isEmpty)
            {
                Throw(); // 丢掉物品
                playerEvents.OnDie?.Invoke(); // 触发死亡事件
            }
        }
    }

    public virtual bool canStandUp => !SphereCast(Vector3.up,originalHeight);

    public virtual void ResetJumps() => jumpCounter = 0;

    /// <summary>
    /// 设置跳跃计数为指定值（特殊用途）
    /// </summary>
    public virtual void SetJumps(int amount) => jumpCounter = amount;

    public virtual void fall() 
    { 
        if(!isGrounded)
        states.Change<FallPlayerState>(); 
    }

    /// <summary>
    /// 执行跳跃逻辑（包括多段跳、土狼跳、持物判定）
    /// </summary>
    public virtual void Jump()
    {
        // 是否可以进行二段 / 多段跳
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        // 土狼跳判定（离地一小段时间内仍然可以跳）
        var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
        // 是否允许在持物状态下跳跃
        //var holdJump :bool = !holding || stats.current.canJumpWhileHolding;
        
        // 地面 / 轨道 / 多段跳 / 土狼跳条件满足时才允许跳跃
        if ((isGrounded || canMultiJump || canCoyoteJump) )
        {
            if (inputs.GetJumpDown()) // 按下跳跃键
            {
                Jump(stats.current.maxJumpHeight);
            }
        }

        // 松开跳跃键时，如果还在上升，限制为最小跳跃高度（实现"按得短跳得低"的效果），早松手就早限制
        if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        }
    }

    /// <summary>
    /// 执行一个标准的向上跳跃
    /// </summary>
    public virtual void Jump(float height)
    {
        jumpCounter++; // 增加跳跃计数
        verticalVelocity = Vector3.up * height; // 设置垂直速度
        states.Change<FallPlayerState>(); // 切换为下落状态（跳起后最终会落下）
        playerEvents.OnJump?.Invoke(); // 触发跳跃事件
    }
}
