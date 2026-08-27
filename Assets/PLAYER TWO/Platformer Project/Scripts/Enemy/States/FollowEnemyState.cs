using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Follow Enemy State")]
public class FollowEnemyState : EnemyState
{
    // 进入状态
    protected override void OnEnter(Enemy enemy) { }

    // 退出状态
    protected override void OnExit(Enemy enemy) { }

    // 状态每帧逻辑
    protected override void OnStep(Enemy enemy)
    {
        enemy.Gravity();        // 应用重力
        enemy.SnapToGround();   // 贴地，防止浮空

        var head = enemy.player.position - enemy.position; // 玩家位置与敌人位置的向量

        var direction = new Vector3(head.x, 0f, head.z).normalized; // 只取水平分量，归一化

        // 朝着玩家方向前进
        enemy.Accelerate(direction, enemy.stats.current.followAcceleration, enemy.stats.current.followTopSpeed);
        enemy.FaceDirectionSmooth(direction);
    }

    // 碰撞接触回调
    public override void OnContact(Enemy enemy, Collider other) { }
}
