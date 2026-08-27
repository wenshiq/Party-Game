using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡控制器
/// 该类负责统一管理关卡内的常用操作，例如：
/// - 结束关卡 / 退出关卡
/// - 玩家重生 / 关卡重启
/// - 增加金币 / 收集星星 / 结算分数
/// - 暂停与恢复游戏
///
/// 它通过访问关卡功能类的单例（LevelFinisher、LevelRespawner、LevelScore、LevelPauser）
/// 来完成对应的操作，方便在其他地方统一调用，而不必直接操作这些类。
/// </summary>

[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Controller")]
public class LevelController : MonoBehaviour
{
    /// <summary>
    /// 关卡暂停控制器（单例）
    /// 用于控制游戏暂停和恢复
    /// </summary>
    protected LevelPauser m_pauser => LevelPauser.instance;

    protected LevelFinisher m_finisher =>LevelFinisher.instance;
    protected LevelScore m_score => LevelScore.instance;
    protected LevelRespawner m_respawner => LevelRespawner.instance;

    // ============ 计分系统 ============
    /// <summary>
    /// 增加金币数量
    /// </summary>
    /// <param name="amount">增加的金币数量</param>
    public virtual void AddCoins(int amount)
    {
        m_score.coins += amount;
    }

    public virtual void Respawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);

    // ============ 暂停与恢复 ============
    /// <summary>
    /// 暂停或恢复游戏
    /// </summary>
    /// <param name="value">true 为暂停，false 为恢复</param>
    public virtual void Pause(bool value) => m_pauser.Pause(value);

    public virtual void ReStart() => m_respawner.Restart();

    public virtual void Exit() => m_finisher.Exit();

    public virtual void Finish() => m_finisher.Finish();
}
