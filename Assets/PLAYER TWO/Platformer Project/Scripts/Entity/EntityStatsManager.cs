using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class EntityStatsManager<T> : MonoBehaviour where T : EntityStats<T>
{
    public T[] stats; // 可配置的属性数组，编辑器里可以设置多个属性（比如不同的状态属性）
    public T current { get; protected set; }

    public virtual void Change(int to)
    {
        // 边界校验：防止下标越界报错
        if (to >= 0 && to < stats.Length)
        {
            // 避免重复赋值：当前已经是目标属性就不执行切换
            if (current != stats[to])
            {
                current = stats[to];
            }
        }
    }

    protected virtual void Start()
    {
        // 数组存在配置时，默认加载第0个属性作为初始属性
        if (stats.Length > 0)
        {
            current = stats[0];
        }
    }
}
