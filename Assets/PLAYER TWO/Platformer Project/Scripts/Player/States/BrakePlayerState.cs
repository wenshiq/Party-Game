using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 将该类添加到 Unity 的组件菜单，方便在 Inspector 里挂载
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Brake Player State")]
public class BrakePlayerState : PlayerState
{
    /// <summary>
    /// 进入刹车状态时调用（此处无额外逻辑）
    /// </summary>
    protected override void OnEnter(Player player)
    {
    }

    /// <summary>
    /// 离开刹车状态时调用（此处无额外逻辑）
    /// </summary>
    protected override void OnExit(Player player)
    {
    }

    /// <summary>
    /// 每帧更新时调用（刹车逻辑）
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 获取玩家输入的方向（相对于摄像机的方向）
        var inputDirection = player.inputs.GetMovementCameraDirection();

        if (player.stats.current.canBackflip &&
            Vector3.Dot(inputDirection, player.transform.forward) < 0 &&
            player.inputs.GetJumpDown())
        {
            player.Backflip(player.stats.current.backflipBackwardTurnForce);
        }
        else
        {
            player.SnapToGround();
            player.Jump();
            // 执行减速逻辑（逐渐降低水平速度，直到停下）
            player.Decelerate();
            player.fall();
            // 如果玩家的水平速度为 0（完全停下来了）
            if (player.lateralVelocity.sqrMagnitude == 0)
            {
                // 状态切换为 Idle（待机状态）
                player.states.Change<IdlePlayerState>();
            }
            else if (player.verticalVelocity.y < 0)
            {
                player.StompAttack();
            }
        }
    }

    public override void OnContact(Player player, Collider other)
    {

    }
}
