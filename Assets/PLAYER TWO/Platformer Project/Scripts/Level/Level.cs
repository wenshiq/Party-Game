using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : Singleton<Level>
{
    /// <summary>
    /// 当前关卡中玩家对象的缓存引用。
    /// 第一次访问时会查找玩家，之后直接使用缓存，避免重复调用 FindObjectOfType。
    /// </summary>
    protected Player m_player;

    /// <summary>
    /// 获取当前关卡中激活的玩家对象。
    /// 如果缓存为空（m_player == null），则会使用 FindObjectOfType 搜索场景中的 Player 脚本。
    /// 搜索后会将结果缓存到 m_player，后续直接使用缓存。
    /// </summary>
    public Player player
    {
        get
        {
            // 如果还没有找到玩家对象，则进行查找
            if (!m_player)
            {
                m_player = FindObjectOfType<Player>();
            }
            // 返回当前玩家对象（可能为 null，如果场景中没有 Player）
            return m_player;
        }
    }
}

