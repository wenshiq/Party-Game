using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RailGrindPlayerState : PlayerState
{
    protected bool m_backwards;
    protected float m_speed;
    protected float m_lastDashTime;
    // 滑轨状态下玩家碰撞体被换成 trigger，与金币(trigger)之间不触发 OnTrigger 事件，
    // 需要主动检测周围金币。此偏移量是拾取半径的额外余量。
    protected const float k_collectRadiusOffset = 0.5f;

    protected override void OnEnter(Player entity)
    {
        // 进入滑轨状态时，我们要算一下角色的朝向，位置等信息，你要沿着滑轨移动，就得有方向，位置，旋转等等信息
        Evaluate(entity, out var point, out var forward, out var upward, out var t);
        // 更新玩家位置，进入滑轨状态了得把玩家吸到滑轨上
        UpdatePosition(entity, point, upward);
        // 是否反方向 要看player的面朝向和forward之间的夹角
        m_backwards = Vector3.Dot(entity.transform.forward, forward) < 0;
        // 在滑轨上的速度就是取一个最大值，看是玩家的速度大还是初始速度大
        m_speed = Mathf.Max(entity.lateralVelocity.magnitude, entity.stats.current.minGrindInitialSpeed);
        // 把玩家速度归零
        entity.velocity = Vector3.zero;
        // 开启自定义碰撞
        entity.UseCustomCollision(entity.stats.current.useCustomCollision);
    }

    protected override void OnExit(Player entity)
    {
        // 退出滑轨
        entity.ExitRail();
        // 关闭自定义碰撞
        entity.UseCustomCollision(false);
    }

    protected override void OnStep(Player entity)
    {
        // 只有跳跃能离开滑轨
        entity.Jump();
        // 在滑轨上时
        if (entity.onRails)
        {
            // 滑轨时玩家是 trigger 胶囊体，与金币(trigger)之间不触发 OnTrigger 事件，
            // 因此在这里主动检测并拾取周围的金币。
            CollectNearbyCollectables(entity);
            // 计算目标方向等数据
            Evaluate(entity, out var point, out var forward, out var upward, out var t);
            // 移动的方向要看玩家是否反向，反向就取反
            var direction = m_backwards ? -forward : forward;
            // 导轨有升高和降低，升高时要加上坡的力，降低时要加下坡的力
            var factor = Vector3.Dot(Vector3.up, direction);
            var multiplier = factor <= 0
                ? entity.stats.current.slopeDownwardForce
                : entity.stats.current.slopeUpwardForce;
            // 控制减速
            HandleDeceleration(entity);
            // 控制冲刺
            HandleDash(entity);
            // 如果应用上下坡的时候加减速
            if (entity.stats.current.applyGrindingSlopeFactor)
            {
                // 上面算的速度就加上
                m_speed -= factor * multiplier * Time.deltaTime;
            }
            // 但是不论怎么样上下坡的时候速度不能越界
            m_speed = Mathf.Clamp(m_speed, entity.stats.current.minGrindSpeed, entity.stats.current.grindTopSpeed);
            // 人物在导轨上面也得旋转
            RotateOnRail(entity, direction, upward);
            // 应用在导轨上的速度，让玩家沿导轨以m_speed进行移动
            entity.velocity = direction * m_speed;
            // 如果导轨是闭环的，或者t在0~0.9范围内
            if (entity.rails.Spline.Closed || (t > 0 && t < 0.9f))
            {
                // 重新设置位置，让玩家不要掉下去了
                UpdatePosition(entity, point, upward);
            }
        }
        else // 不在滑轨上时就掉落
        {
            entity.states.Change<FallPlayerState>();
        }
    }

    protected virtual void CollectNearbyCollectables(Player player)
    {
        // 金币(Collectable)是 trigger，检测玩家周围并拾取。
        // QueryTriggerInteraction.Collide 让 OverlapSphere 能命中 trigger 碰撞体。
        var colliders = Physics.OverlapSphere(player.position, player.radius + k_collectRadiusOffset,
            Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<Collectable>(out var collectable))
            {
                collectable.Collect(player);
            }
        }
    }

    public override void OnContact(Player entity, Collider other)
    {
    }
    // 返回参数有 在哪个点 向前的方向 向上的方向 与最近点对应的归一化插值率
    protected virtual void Evaluate(Player player, out Vector3 point, out Vector3 forward, out Vector3 upward, out float t)
    {
        // 起始点 是 将玩家位置转换到滑轨坐标系下的位置
        var origin = player.rails.transform.InverseTransformPoint(player.transform.position);
        // 计算滑轨上面离输入点最近的点，这个获取到的点就是我们在unity里面一个一个加的那个点，同时获取到与最近点对应的归一化插值率。
        SplineUtility.GetNearestPoint(player.rails.Spline, origin, out var nearest, out t);
        // 将最近的点坐标转换到世界坐标系下
        point = player.rails.transform.TransformPoint(nearest);
        // 向前的方向就应该是 计算世界空间中样条线上点 t 的切向量 所得的方向
        forward = Vector3.Normalize(player.rails.EvaluateTangent(t));
        // 向上的方向就应该是 计算世界空间中样条线上点 t 的向上向量
        upward = Vector3.Normalize(player.rails.EvaluateUpVector(t));
        // 总之，我们拿到了玩家在滑轨上时最近的要去的点，向前的方向，向上的方向，还有这个t
    }

    protected virtual void UpdatePosition(Player player, Vector3 point, Vector3 upward)
    {
        // 就是把player吸到轨道上面，加一个offset不要穿模
        player.transform.position = point + upward * GetDistanceToRail(player);
    }

    protected virtual float GetDistanceToRail(Player player)
    {
        // 这个offset就是 player 身高的一半 + 导轨中心到边缘的距离，就是算player的中心点应该在哪
        return player.originalHeight * 0.5f + player.stats.current.grindRadiusOffset;
    }

    protected virtual void RotateOnRail(Player player, Vector3 forward, Vector3 upward)
    {
        // 前方不是没有值的时候
        if (forward != Vector3.zero)
        {
            // 让玩家面朝向forward方向，以transform.up为旋转轴
            player.transform.rotation = Quaternion.LookRotation(forward, player.transform.up);
        }
        // 玩家的头顶朝向也需要根据导轨的旋转进行改变，导轨倾斜了玩家的头顶方向也要倾斜
        player.transform.rotation = Quaternion.FromToRotation(player.transform.up, upward) * player.transform.rotation;
    }

    protected virtual void HandleDeceleration(Player player)
    {
        // 如果能刹车 且 检测到玩家按了刹车键
        if (player.stats.current.canGrindBrake && player.inputs.GetGrindBrake())
        {
            // 每帧刹车减多少速度
            var decelerationDelta = player.stats.current.grindBrakeDeceleration * Time.deltaTime;
            // 减速
            m_speed = Mathf.MoveTowards(m_speed, 0, decelerationDelta);
        }
    }

    protected virtual void HandleDash(Player player)
    {
        // 如果玩家能在导轨上冲刺 且 检测到冲刺按下 且 不在冲刺冷却时间里面
        if (player.stats.current.canGrindDash && player.inputs.GetDashDown() && Time.time >= m_lastDashTime + player.stats.current.grindDashCoolDown)
        {
            // 重置冲刺冷却时间，然后冲刺，就是加上冲刺所得的速度
            m_lastDashTime = Time.time;
            m_speed = player.stats.current.grindDashForce;
            player.playerEvents.OnDashStarted?.Invoke();
        }
    }
}
