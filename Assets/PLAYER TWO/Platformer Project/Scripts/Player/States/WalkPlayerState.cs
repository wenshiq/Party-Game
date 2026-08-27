using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkPlayerState : PlayerState
{
    public override void OnContact(Player player, Collider other)
    {
        player.PushRigidbody(other);
    }

    protected override void OnEnter(Player player)
    {
        
    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        player.SnapToGround();
        player.Jump();
        player.PickAndThrow();
        player.Gravity();
        player.fall();
        player.Dash();
        player.Spin();
        // 获取玩家输入方向（相机方向）
        var inputDirection = player.inputs.GetMovementCameraDirection();

        if (inputDirection.sqrMagnitude > 0)
        {
            // 输入方向与当前水平速度的点乘，用于判断刹车阈值
            var dot = Vector3.Dot(lhs: inputDirection, rhs: player.lateralVelocity);

            if (dot >= player.stats.current.brakeThreshold)
            {
                //超过刹车阈值 → 正常加速与面向方向
                player.Accelerate(inputDirection);
                player.FaceDirectionSmooth(player.lateralVelocity);
            }
            else
            {
                player.states.Change<BrakePlayerState>();
            }
        }
        else
        {
            player.Friction();

            if (player.lateralVelocity.sqrMagnitude <= 0)
            {
                player.states.Change<IdlePlayerState>();
            }
        }
        // 玩家按下蹲或爬行 → 切换到蹲伏状态
        if (player.inputs.GetCrouchAndCraw())
        {
            player.states.Change<CrouchPlayerState>();
        }

    }
}
