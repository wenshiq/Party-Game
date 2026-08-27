using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoleClimbingPlayerState : PlayerState
{
    // 爬杆的时候要吸附在杆子上面，然后还要能左右转

    // 杆子的半径
    protected float m_collisionRadius;

    protected override void OnEnter(Player entity)
    {
        // 进入爬杆状态，那就重设这一堆东西
        entity.ResetJumps();
        entity.ResetAirDash();
        entity.ResetAirSpin();
        // 速度设置为0
        entity.velocity = Vector3.zero;
        // 获取玩家位置到杆子中心的朝向
        entity.pole.GetDirectionToPole(entity.transform, out m_collisionRadius);
        // 给模型设置偏移量，在杆子上面不要穿模
        entity.skin.position += entity.transform.rotation * entity.stats.current.poleClimbSkinOffset;
    }

    protected override void OnExit(Player entity)
    {
        // 当然退出时要把偏移量去掉
        entity.skin.position -= entity.transform.rotation * entity.stats.current.poleClimbSkinOffset;
    }

    protected override void OnStep(Player entity)
    {
        // 获取玩家到杆子中心的朝向
        var poleDirection = entity.pole.GetDirectionToPole(entity.transform);
        // 获取输入方向
        var inputDirection = entity.inputs.GetMovementDirection();
        // 玩家应当面向杆子中心
        entity.FaceDirection(poleDirection);
        // 玩家现在任何的水平速度都要围绕着杆子进行设置，也就是只能在杆子上左右旋转，输入方向的x轴就是ad键
        entity.lateralVelocity = entity.transform.right * inputDirection.x * entity.stats.current.climbRotationSpeed;
        // 如果有向上爬的输入（ws）
        if (inputDirection.z != 0)
        {
            var speed = inputDirection.z > 0
                ? entity.stats.current.climbUpSpeed
                : entity.stats.current.climbDownSpeed;
            entity.verticalVelocity = Vector3.up * speed;
            // 向上爬就给向上的速度，向下爬就向下的速度
        }
        else // 没有输入那就没速度
        {
            entity.verticalVelocity = Vector3.zero;
        }
        // 检测跳跃，跳跃要沿着杆子中心方向的反方向去跳，与蹬墙跳相似
        if (entity.inputs.GetJumpDown())
        {
            entity.FaceDirection(-poleDirection);
            entity.DirectionalJump(-poleDirection, entity.stats.current.poleJumpHeight, entity.stats.current.poleJumpHeight);
            entity.states.Change<FallPlayerState>();
        }
        // 顺着杆子溜到地上了，就站地上
        if (entity.isGrounded)
        {
            entity.states.Change<IdlePlayerState>();
        }
        // 这里计算的是杆子的最上部和最下部都有一些不能爬上去的区域
        var offset = entity.height * 0.5f + entity.center.y;
        var center = new Vector3(entity.pole.center.x, entity.transform.position.y, entity.pole.center.z);
        var position = center - poleDirection * m_collisionRadius;
        // 将玩家的位置钳制在可用区域内
        entity.transform.position = entity.pole.ClampPointToPoleHeight(position, offset);
    }

    public override void OnContact(Player entity, Collider other)
    {
    }
}
