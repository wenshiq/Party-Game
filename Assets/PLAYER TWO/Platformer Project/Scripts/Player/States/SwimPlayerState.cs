using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家游泳状态
/// - 玩家进入水中后触发
/// - 支持水中移动、浮力、潜水和跳出水面
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Swim Player State")]
public class SwimPlayerState : PlayerState
{
    public override void OnContact(Player entity, Collider other)
    {
        entity.PushRigidbody(other);
    }

    /// <summary>
    /// 进入游泳状态
    /// - 将玩家当前速度转换为水中速度
    /// </summary>
    protected override void OnEnter(Player player) =>
        player.velocity *= player.stats.current.waterConversion;

    protected override void OnExit(Player entity)
    {
        
    }

    /// <summary>
    /// 每帧更新游泳状态逻辑
    /// - 水中加速与面向控制
    /// - 浮力处理（水面上浮）
    /// - 水中跳跃（跳出水面）
    /// - 潜水（下沉）
    /// - 无输入时减速
    /// - 玩家脱离水面时切换到行走状态
    /// </summary>
    protected override void OnStep(Player player)
    {
        if (player.onWater)
        {
            // 获取玩家输入方向（相机方向）
            var inputDirection = player.inputs.GetMovementCameraDirection();

            // 水中加速与面向
            player.WaterAcceleration(inputDirection);
            player.WaterFaceDirection(player.lateralVelocity);

            // 处理水中浮力
            if (player.position.y < player.water.bounds.max.y)
            {
                // 玩家接触地面时，垂直速度归零
                if (player.isGrounded)
                {
                    player.verticalVelocity = Vector3.zero;
                }

                // 施加向上的水浮力
                player.verticalVelocity += Vector3.up * player.stats.current.waterUpwardsForce * Time.deltaTime;
            }
            else
            {
                // 超出水面高度，垂直速度归零
                player.verticalVelocity = Vector3.zero;

                // 玩家跳跃输入 → 跳出水面
                if (player.inputs.GetJumpDown())
                {
                    player.Jump(player.stats.current.waterJumpHeight);
                    player.states.Change<FallPlayerState>();
                }
            }

            // 玩家按下潜水键并未接触地面 → 下潜
            if (!player.isGrounded && player.inputs.GetDive())
            {
                player.verticalVelocity += Vector3.down * player.stats.current.swimDiveForce * Time.deltaTime;
            }

            // 没有输入时，减速
            if (inputDirection.sqrMagnitude == 0)
            {
                player.Decelerate(player.stats.current.swimDeceleration);
            }
        }
        else
        {
            // 玩家不在水中 → 切换到行走状态
            player.states.Change<WalkPlayerState>();
        }
    }
}
