using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingPlayerState : PlayerState
{
    /// <summary>
    /// 进入爬行状态时调用
    /// - 调整角色碰撞体高度为"蹲伏高度"
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.ResizeCollider(player.stats.current.crouchHeight);
    }

    /// <summary>
    /// 离开爬行状态时调用
    /// - 恢复角色碰撞体高度为原始站立高度
    /// </summary>
    protected override void OnExit(Player player)
    {
        player.ResizeCollider(player.originalHeight);
    }

    /// <summary>
    /// 每帧更新时调用（爬行逻辑）
    /// </summary>
    protected override void OnStep(Player player)
    {
        player.SnapToGround();
        player.Jump();
        player.Gravity();
        player.fall();

        var inputDirection = player.inputs.GetMovementCameraDirection();

        // 判断是否保持爬行状态：
        // 1. 玩家仍然按着"蹲伏/爬行"按键
        // 2. 或者角色当前不能站起来（头顶有障碍物）
        if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
        {
            // 如果有方向输入 → 爬行移动
            if (inputDirection.sqrMagnitude > 0)
            {
                // 执行爬行加速度移动
                player.CrawlingAccelerate(inputDirection);

                // 平滑转向，使角色朝向当前移动方向
                player.FaceDirectionSmooth(player.lateralVelocity);
            }
            else
            {
                // 没有输入时 → 逐渐减速
                player.Decelerate(player.stats.current.crawlingFriction);
            }
        }
        else 
        {
            // 玩家松开"蹲伏/爬行"按键且角色可以站起来 → 切换到站立状态
            player.states.Change<IdlePlayerState>();    
        }
    }

    /// <summary>
    /// 爬行状态下发生碰撞时调用（此处无逻辑）
    /// </summary>
    public override void OnContact(Player player, Collider other) 
    {
        player.WallDrag(other);
    }
}
