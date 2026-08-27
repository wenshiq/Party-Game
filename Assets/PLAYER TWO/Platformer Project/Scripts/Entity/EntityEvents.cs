using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class EntityEvents 
{
    /// <summary>
    /// 当实体落地时触发的事件。
    /// </summary>
    public UnityEvent OnGroundEnter;

    /// <summary>
    /// 当实体离开地面时触发的事件。
    /// </summary>
    public UnityEvent OnGroundExit;

    /// <summary>
    /// 当实体进入“轨道”（Rails）时触发的事件。
    /// </summary>
    public UnityEvent OnRailsEnter;

    /// <summary>
    /// 当实体离开“轨道”时触发的事件。
    /// </summary>
    public UnityEvent OnRailsExit;
}
