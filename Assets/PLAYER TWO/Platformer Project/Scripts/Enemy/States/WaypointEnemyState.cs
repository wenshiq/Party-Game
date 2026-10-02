using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointEnemyState : EnemyState
{
    protected override void OnEnter(Enemy entity)
    {

    }

    protected override void OnExit(Enemy entity)
    {

    }

    protected override void OnStep(Enemy entity)
    {
        entity.Gravity();
        entity.SnapToGround();
        // 目的地就是 当前的路径点1
        var destnation = entity.waypoints.current.position;
        // 拿到当前路径点的坐标信息，y值重设为怪物的y值，不然y值不同怎么样都到不了目的地，就卡住了
        destnation = new Vector3(destnation.x, entity.position.y, destnation.z);
        // 前进方向（向量长度要注意）
        var head = destnation - entity.position;
        // 前进距离
        var distance = head.magnitude;
        // 前进方向的单位向量
        var direction = head / distance;
        // 如果 到了路径点了
        if (distance <= entity.stats.current.waypointMinDistance)
        {
            // 减速，换目标到下一个路径点
            entity.Decelerate();
            entity.waypoints.Next();
        }
        else // 没到路径点的时候
        {
            // 朝路径点前进
            entity.Accelerate(direction, entity.stats.current.waypointAcceleration, entity.stats.current.waypointTopSpeed);
            // 如果没有朝向路径点，就改变怪物朝向
            if (entity.stats.current.faceWayPoint)
            {
                entity.FaceDirectionSmooth(direction);
            }
        }
    }

    public override void OnContact(Enemy entity, Collider other)
    {

    }
}
