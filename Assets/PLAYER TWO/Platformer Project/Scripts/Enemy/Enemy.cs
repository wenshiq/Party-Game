using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity<Enemy>
{
    // 敌人看见的物体
    protected Collider[] m_sightOverlaps = new Collider[1024];
    // 敌人能攻击到的物体
    protected Collider[] m_currentAttackOverlaps = new Collider[1024];
    // 有没有锁定player
    public Player player { get; protected set; }
    // 血量
    public Health health { get; protected set; }
    // 属性
    public EnemyStatsManager stats { get; protected set; }
    // 路线规划
    public WaypointManager waypoints { get; protected set; }
    // 事件
    public EnemyEvents enemyEvents;

    protected virtual void InitializeWaypointManager() => waypoints = GetComponent<WaypointManager>();
    protected virtual void InitializeStatsManager() => stats = GetComponent<EnemyStatsManager>();
    protected virtual void InitializeHealth() => health = GetComponent<Health>();
    protected virtual void InitializeTag() => tag = GameTags.Enemy;

    public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) =>
        Accelerate(direction, stats.current.turningDrag, acceleration, topSpeed);

    public virtual void Decelerate() => Decelerate(stats.current.deceleration);

    public virtual void Friction() => Decelerate(stats.current.friction);

    public virtual void Gravity() => Gravity(stats.current.gravity);

    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

    // 在Update时，敌人要不断地去搜索player是否进入锁定范围
    protected virtual void HandleSight()
    {
        // 没有锁定player时
        if (!player)
        {
            // 用球形射线判断 以自己为中心半径为spotRange的范围内的所有碰撞体
            var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);

            for (int i = 0; i < overlaps; i++)
            {
                // 如果发现Player
                if (m_sightOverlaps[i].CompareTag(GameTags.Player))
                {
                    if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
                    {
                        // 索敌
                        this.player = player;
                        enemyEvents.OnPlayerSpotted?.Invoke();
                        return;
                    }
                }
            }
        }
        else // 否则 就是已经锁定player
        {
            // 判断player和自己的距离
            var distance = Vector3.Distance(position, player.position);
            // 如果player死了或者距离太远了
            if (player.health.current == 0 || distance > stats.current.viewRange)
            {
                // 失去索敌
                player = null;
                enemyEvents.OnPlayerScaped?.Invoke();
            }
        }
    }
    // 接触攻击
    protected virtual void ContactAttack()
    {
        // 能接触攻击时
        if (stats.current.canAttackOnContact)
        {
            // 计算能接触到怪物的所有碰撞体
            var overlaps = OverlapEntity(m_currentAttackOverlaps, stats.current.contactOffset);

            for (int i = 0; i < overlaps; i++)
            {
                // 如果里面有player
                if (m_currentAttackOverlaps[i].CompareTag(GameTags.Player) && m_currentAttackOverlaps[i].TryGetComponent<Player>(out var player))
                {
                    // 计算 偏移量，怪物头顶减去 设定好的偏移量
                    var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;
                    // player在怪物头顶的时候能给怪物造成伤害，所以要判断player是不是在怪物上方
                    if (!player.IsPointUnderStep(stepping)) // 如果不在怪物上方
                    {
                        // 如果能被推开
                        if (stats.current.canContactPushBack)
                        {
                            // 施加推力给怪物
                            lateralVelocity = -transform.forward * stats.current.contactPushBackForce;
                        }
                        // 玩家受击
                        player.ApplyDamage(stats.current.contactDamage, transform.position);
                        enemyEvents.OnPlayerContact?.Invoke();
                    }
                }
            }
        }
    }

    public override void ApplyDamage(int amount, Vector3 origin)
    {
        // 如果血没空 且 不在无敌时间
        if (!health.isEmpty && !health.recovering)
        {
            // 受到伤害
            health.Damage(amount);
            enemyEvents.OnDamage?.Invoke();

            // 血空了就死掉
            if (health.isEmpty)
            {
                controller.enabled = false;
                enemyEvents.OnDie?.Invoke();
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        InitializeTag();
        InitializeStatsManager();
        InitializeHealth();
        InitializeWaypointManager();
    }

    protected override void OnUpdate()
    {
        HandleSight();
        ContactAttack();
    }
}
