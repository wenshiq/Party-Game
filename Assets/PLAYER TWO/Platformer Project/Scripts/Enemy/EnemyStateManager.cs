using PLAYERTWO.PlatformerProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 和玩家状态机一样，也是通过一个string列表在Inspector窗口里面设置怪物的状态
[RequireComponent(typeof(Enemy))]
public class EnemyStateManager : EntityStateManager<Enemy>
{
    [ClassTypeName(typeof(EnemyState))]
    public string[] states;

    protected override List<EntityState<Enemy>> GetStateList()
    {
        // 通过反射去创建实例
        return EnemyState.CreateListFromStringArray(states);
    }
    /// <summary>
    /// 根据状态列表索引切换当前状态。
    /// </summary>
    /// <param name="to">目标状态在状态列表中的索引。</param>
    public new virtual void Change(int to)
    {
        if (to >= 0 && to < m_list.Count)
        {
            Change(m_list[to]);
        }
    }

}
