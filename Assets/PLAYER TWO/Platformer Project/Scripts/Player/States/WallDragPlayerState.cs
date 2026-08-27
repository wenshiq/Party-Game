using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDragPlayerState : PlayerState
{
    protected override void OnEnter(Player entity)
    {
        // 既然要蹬墙跳，那就要重置所有这些空中只能做一次的事情
        entity.ResetJumps();
        entity.ResetAirSpin();
        entity.ResetAirDash();
        // 归零速度
        entity.velocity = Vector3.zero;
        // 下一次跳出的方向是墙面的反方向
        var direction = entity.lastWallNormal;
        direction = new Vector3(direction.x, 0, direction.z).normalized;
        // 面对跳出方向
        entity.FaceDirection(direction);
        // 设置模型偏移量
        entity.skin.position += entity.transform.rotation * entity.stats.current.wallDragSkinOffset;
    }

    protected override void OnExit(Player entity)
    {
        // 退出时要还原模型的偏移量
        entity.skin.position -= entity.transform.rotation * entity.stats.current.wallDragSkinOffset;
        // 判断一下 如果不在地上 且 还有父对象
        if (!entity.isGrounded && entity.transform.parent != null)
        {
            // 在空中时不应该有父节点
            entity.transform.parent = null;
        }
    }

    protected override void OnStep(Player entity)
    {
        // 设置垂直速度，挂墙上的时候会往下溜
        entity.verticalVelocity += Vector3.down * entity.stats.current.wallDragGravity * Time.deltaTime;
        // 如果在地面上 或者 没有检测到前方人物半径内有东西
        if (entity.isGrounded || !entity.CapsuleCast(-entity.transform.forward, entity.radius))
        {
            // idle
            entity.states.Change<IdlePlayerState>();
        }
        else if (entity.inputs.GetJumpDown()) // 如果检测到按下跳跃键
        {
            // 是否要锁定跳跃控制
            if (entity.stats.current.wallJumpLockMovement)
            {
                entity.inputs.LockMovementDirection();
            }
            // 按规定方向跳跃
            entity.DirectionalJump(entity.transform.forward, entity.stats.current.wallJumpHeight, entity.stats.current.wallJumpDistance);
            // 跳出去后要切换到掉落状态
            entity.states.Change<FallPlayerState>();

        }
    }

    public override void OnContact(Player entity, Collider other)
    {

    }
}
