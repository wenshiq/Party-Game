using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPlayerState : PlayerState
{
    public override void OnContact(Player player, Collider other)
    {
        player.WallDrag(other);
        player.GrabPole(other);
        player.PushRigidbody(other);
    }

    /// <summary>
    /// - 垂直速度清零 (防止下落或跳跃干扰)
    /// - 设置水平速度为"角色前方 * 冲刺力度"
    /// - 触发冲刺开始事件 (可用于播放音效、特效等)
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero; // 清空垂直速度
        player.lateralVelocity = player.transform.forward * player.stats.current.dashForce;
        player.playerEvents.OnDashStarted.Invoke(); // 调用事件：冲刺开始
    }

    /// <summary>
    /// 离开冲刺状态时调用
    /// - 限制当前水平速度不超过最大速度 topSpeed
    /// - 触发冲刺结束事件
    /// </summary>
    protected override void OnExit(Player player)
    {
        // 限制水平速度在最大速度范围内
        player.lateralVelocity = Vector3.ClampMagnitude(
            player.lateralVelocity, player.stats.current.topSpeed);
        player.playerEvents.OnDashEnded.Invoke(); // 调用事件：冲刺结束
    }

    protected override void OnStep(Player player)
    {
        player.Jump(); // 冲刺中仍然可以跳跃

        // 判断是否超过冲刺持续时间
        if (timeSinceEntered > player.stats.current.dashDuration)
        {
            if (player.isGrounded)
                player.states.Change<WalkPlayerState>(); // 地面 → 走路
            else
                player.states.Change<FallPlayerState>(); // 空中 → 下落
        }
    }
}
