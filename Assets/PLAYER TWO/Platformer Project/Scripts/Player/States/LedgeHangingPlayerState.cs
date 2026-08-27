using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeHangingPlayerState : PlayerState
{
    // 人物挂在墙上的时候会有父对象，这里设置bool值来判断是否保留这个父对象
    protected bool m_keepParent;
    // 清除父对象的携程，我们要让player对象去开启携程，而不是这个状态开启携程
    protected Coroutine m_clearParentRoutine;
    protected const float k_clearParentDelay = 0.25f;

    protected override void OnEnter(Player entity)
    {
        // 在进入状态时，判断清除父对象携程是不是空的，如果不是空的，就要把这个携程停止
        if (m_clearParentRoutine != null)
        {
            entity.StopCoroutine(m_clearParentRoutine);
        }
        // 我们不知道人物接下来是要走了还是要接着在这个物体上爬，所以先把保留父对象设置为false
        m_keepParent = false;
        // skin 就是 lily 预制体里面的模型，这里就是要修改模型位置
        // entity.stats.current.ledgeHangingSkinOffset 是一个 Vector3 类型的变量，这个值没有修改过，所以是（0，0，0）
        // 那么这里就是将偏移量旋转到角色模型的方向来，然后给角色模型加上偏移量
        entity.skin.position += entity.transform.rotation * entity.stats.current.ledgeHangingSkinOffset;
        // 重设跳跃次数，这个函数没有实现，其实就是跳跃次数 = 0，挂在墙上的时候也能按下空格进行跳跃
        entity.ResetJumps();
    }

    protected override void OnExit(Player entity)
    {
        // 退出状态时，要进行父对象的清除，上文所说要player去开启携程清除父对象
        m_clearParentRoutine = entity.StartCoroutine(ClearParentRoutine(entity));
        // 既然都退出这个状态了，对模型的偏移就可以设置回去了，减去同方向的偏移量就行了
        entity.skin.position -= entity.transform.rotation * entity.stats.current.ledgeHangingSkinOffset;
    }

    protected override void OnStep(Player entity)
    {
        // 首先，这里已经进入了边缘攀爬的状态，也就是说我们一定是已经扒在边缘上了，我们需要向前发射球形射线和从头顶前方向下发射射线检测我们扒到的这个物体，以保证我们时刻都扒在物体上，如果检测不到了，说明物体消失了或者其他的什么情况，那就要掉下来。
        // 我们计算出球形射线的起点，player.position实际上就已经是player的中心点了，所以再加上半身高，再减去这个偏移量，也就是说这个点在头顶的下方。
        var sideOrigin = entity.position + Vector3.up * entity.height * 0.5f +
                         Vector3.down * entity.stats.current.ledgeSideHeightOffset;
        // 球形射线的半径，这是定好的一个参数
        var rayRadius = entity.stats.current.ledgeSideCollisionRadius;
        // 球形射线发射的距离，角色碰撞体半径 + 设定好的距离
        var rayDistance = entity.radius + entity.stats.current.ledgeSideMaxDistance;
        // 我们先来看下面的判断
        // 那么球形射线发射完了，前方的物体检测完了，还需要检测纵向的物体，从头顶前方的一个位置向下发射一条射线，它要碰撞到东西才能保证我们一直挂在边缘上
        // 这个偏移量是 一半身高 + 纵向偏移，注意这是个数字
        var ledgeTopHeightOffset = entity.height * 0.5f + entity.stats.current.ledgeMaxDownwardDistance;
        // 这个距离是 半径 + 前向偏移，注意也是个数字
        var ledgeTopMaxDistance = entity.radius + entity.stats.current.ledgeMaxForwardDistance;
        // 那么这个点就很明显了，就是角色的中心点先向上移动 ledgeTopHeightOffset 的距离，它现在已经离开头顶了；然后再向前移动 ledgeTopMaxDistance 的距离
        var topOrigin = entity.position + Vector3.up * ledgeTopHeightOffset +
                        entity.transform.forward * ledgeTopMaxDistance;
        // 这里发射了球形射线进行检测，碰到物体返回true，没碰到返回false，sideHit就是我们拿到的碰到物体的信息
        if (Physics.SphereCast(sideOrigin, rayRadius, entity.transform.forward, out var sideHit, rayDistance, entity.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore)
            // 那么从头顶前方的这个点向下发射射线，检测碰到的物体
            && Physics.Raycast(topOrigin, Vector3.down, out var topHit, entity.height, entity.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
        {
            // 都检测到了，说明我们扒在物体边缘了
            // 此时 sideForward 是我们如果要扒在这个物体上时，面朝向应该在哪个方向，这就使用了球形射线与物体碰撞时的法线方向的反方向。
            // 法线方向垂直于被碰撞的物体表面，那么 负的 法线方向就是反过来垂直（面壁）
            var sideForward = -new Vector3(sideHit.normal.x, 0, sideHit.normal.z).normalized;
            // 这里我们拿到键盘输入的数据，这是为了我们能够在扒在物体上面的时候左右移动
            var inputDirection = entity.inputs.GetMovementDirection();
            // 我们需要检测左右移动时不能超过物体的边界，所以也需要发射一条射线，检测是否已经到达物体的左右边界。
            // 那么这个检测起始点就需要在发射球形射线的点基础上进行左右移动
            // 如果我们向右挪动，那么这个点就应该在角色的右肩膀上，如果向左移动，就应该在左肩膀上（形象的比喻一下）
            var ledgeSideOrigin = sideOrigin + entity.transform.right * Mathf.Sign(inputDirection.x) * entity.radius;
            // 角色扒在物体上时，它的position肯定是要在物体边缘下方的。因为position是角色的中心点，我们扒在物体上时，最高点是手，所以身体要下调这段距离。
            var ledgeHeight = topHit.point.y - entity.height * 0.5f;

            // 这两个参数是为了攀爬做的，攀爬是啥动作呢，就是人物挂在边缘的时候，按一下w键他就自己爬上去站在台子上面，所以我们要算一下我们要站的目标点合理不合理
            // 爬上去站的目标点的高度
            var destinationHeight = entity.height * 0.5f + Physics.defaultContactOffset;
            // 爬上去的目标点 是 从上往下的射线碰到的点 + 上面算出来的高度 + 向前偏移角色半径
            var climbDestination = topHit.point + Vector3.up * destinationHeight + entity.transform.forward * entity.radius;
            // 让角色面朝向攀爬的物体
            entity.FaceDirection(sideForward);
            // 如果检测到我们还在物体的内部，即没有达到左右边界，那就可以根据输入的数值给横向速度来改变角色左右的位置
            if (Physics.Raycast(ledgeSideOrigin, sideForward, rayDistance, entity.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
            {
                entity.lateralVelocity = entity.transform.right * inputDirection.x * entity.stats.current.ledgeMovementSpeed;
            }
            else // 如果已经到达边缘了，就停下来
            {
                entity.lateralVelocity = Vector3.zero;
            }
            // 由于位移的更改在状态更改之后，所以我们上面只改了速率，让位置更改的部分去根据速度改位置，我们这里就保证角色扒在边缘的位置的正确性，x和z就是被抓住物体边缘的平面坐标，y坐标就是我们上面所说的向下挪动了的一些位置，让玩家看起来更像是挂在边缘。最后减去的这个量，就是半径，胶囊体不能和这个物体重合，边缘相切就行了。
            entity.transform.position = new Vector3(sideHit.point.x, ledgeHeight, sideHit.point.z) - sideForward * entity.radius - entity.center;
            // 如果检测到按下了释放按钮（鼠标右键），那就让角色面朝物体反方向，切换到掉落状态，如果不切换到反方向，就又扒住了
            if (entity.inputs.GetReleaseLedgeDown())
            {
                entity.FaceDirection(-sideForward);
                entity.states.Change<FallPlayerState>();
            }
            // 如果检测到跳跃按钮，那就跳起来
            else if (entity.inputs.GetJumpDown())
            {
                entity.Jump(entity.stats.current.maxJumpHeight);
                entity.states.Change<FallPlayerState>();
            }
            // 最后如果 输入方向 z 大于 0 （即按了w） 且 能进行攀爬
            else if (inputDirection.z > 0 && entity.stats.current.canClimbLedges &&
            // 且 头顶射线碰到的这个物体 属于 能攀爬的物体中的一种
                    ((1 << topHit.collider.gameObject.layer) & entity.stats.current.ledgeClimbingLayers) != 0 &&
                    // 且 这个 攀爬的目标点合适
                    entity.FitsIntoPosition(climbDestination))
            {
                // 攀爬时父节点要保留，切换到攀爬的状态
                m_keepParent = true;
                entity.states.Change<LedgeClimbingPlayerState>();
                entity.playerEvents.OnLedgeClimbing?.Invoke();
            }
        }
        else
        {
            entity.states.Change<FallPlayerState>();
        }
    }

    public override void OnContact(Player entity, Collider other)
    {
    }

    protected virtual IEnumerator ClearParentRoutine(Player player)
    {
        // 如果要保留父对象，就不做操作了，结束携程
        if (m_keepParent)
        {
            yield break;
        }

        // 否则等一下就清空父对象
        yield return new WaitForSeconds(k_clearParentDelay);

        player.transform.parent = null;
    }
}
