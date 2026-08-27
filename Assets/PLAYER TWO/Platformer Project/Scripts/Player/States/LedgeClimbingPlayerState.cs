using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeClimbingPlayerState : PlayerState
{
    // 攀爬状态要靠这个携程去实现
    protected IEnumerator m_routine;

    protected override void OnEnter(Player entity)
    {
        // 进入攀爬状态时，开启携程
        m_routine = SetPositionRoutine(entity);
        entity.StartCoroutine(m_routine);
    }

    protected override void OnExit(Player entity)
    {
        // 退出攀爬状态时，将skin重设到player下，并关闭携程
        entity.ResetSkinParent();
        entity.StopCoroutine(m_routine);
    }

    protected override void OnStep(Player entity)
    {

    }

    public override void OnContact(Player entity, Collider other)
    {
    }

    protected virtual IEnumerator SetPositionRoutine(Player player)
    {
        // 动作的时间
        var elapsedTime = 0f;
        // 动作要持续多久
        var totalDuration = player.stats.current.ledgeClimbingDuration;
        // 我们的攀爬动作分了两端，一段是先向上移动，然后到达一定高度再向前移动，模拟双手使劲撑起来再爬上台阶的动作
        var halfDuration = totalDuration / 2f;
        // 角色的初始位置
        var initialPosition = player.transform.localPosition;
        // 角色的目标垂直位置，即第一段位移向上移动的目标点
        var targetVerticalPosition =
            player.transform.position + Vector3.up * (player.height + Physics.defaultContactOffset);
        // 角色的侧向位置，即第二段位移向前移动的目标点
        var targetLateralPosition = targetVerticalPosition + player.transform.forward * player.radius * 2f;
        // 如果角色的父对象不为空
        if (player.transform.parent != null)
        {
            // 转换一下坐标点，以免在不同的local下发生错误
            targetVerticalPosition = player.transform.parent.InverseTransformPoint(targetVerticalPosition);
            targetLateralPosition = player.transform.parent.InverseTransformPoint(targetLateralPosition);
        }
        // 将模型的父类设置为台阶
        player.SetSkinParent(player.transform.parent);
        // 将模型的位置及方向设置为 朝向攀爬台阶面的方向的位置
        player.skin.position += player.transform.rotation * player.stats.current.ledgeClimbingSkinOffset;
        // 第一段位移
        while (elapsedTime <= halfDuration)
        {
            elapsedTime += Time.deltaTime;
            // 插值，向上移动
            player.transform.localPosition = Vector3.Lerp(initialPosition, targetVerticalPosition, elapsedTime / halfDuration);
            yield return null;
        }
        // 到达预定高度
        elapsedTime = 0;
        player.transform.localPosition = targetVerticalPosition;
        // 第二段位移
        while (elapsedTime <= halfDuration)
        {
            elapsedTime += Time.deltaTime;
            // 插值向前移动
            player.transform.localPosition = Vector3.Lerp(targetVerticalPosition, targetLateralPosition, elapsedTime / halfDuration);
            yield return null;
        }
        // 到达预定目标点，攀爬结束
        player.transform.localPosition = targetLateralPosition;
        player.states.Change<IdlePlayerState>();
    }
}
