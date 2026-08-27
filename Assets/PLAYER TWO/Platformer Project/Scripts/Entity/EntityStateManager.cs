using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using static UnityEditor.VersionControl.Asset;

public abstract class EntityStateManager : MonoBehaviour
{
    public EntityStateManagerEvents events;
}

public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{

    protected List<EntityState<T>> m_list = new List<EntityState<T>>();
    protected Dictionary<Type, EntityState<T>> m_state= new Dictionary<Type, EntityState<T>>();
    public EntityState<T> current { get; protected set; }
    public EntityState<T> last { get; protected set; }

    public int index => m_list.IndexOf(current);// 获取当前状态在状态列表中的索引

    public int lastIndex => m_list.IndexOf(last);// 获取上一个状态在状态列表中的索引
    public T entity { get; protected set; }

    protected abstract List<EntityState<T>> GetStateList();


    protected virtual void InitializeEntity() => entity = GetComponent<T>();
    protected virtual void InitializeStates()
    {
        m_list = GetStateList();

        foreach (var state in m_list)
        {
            var type = state.GetType();

            if (!m_state.ContainsKey(type))
            {
                m_state.Add(type, state);
            }

            if (m_list.Count > 0)
            {
                current = m_list[0];
            }
        }

    }
    protected virtual void Start()
    {
        InitializeEntity();
        InitializeStates();
         
    }
    /// <summary>
    /// 判断当前状态是否为指定类型。
    /// </summary>
    /// <param name="type">需要比较的状态类型。</param>
    /// <returns>如果当前状态类型等于参数类型返回 true，否则返回 false。</returns>
    public virtual bool IsCurrentOfType(Type type)
    {
        if (current == null)
        {
            return false;
        }
        return current.GetType() == type;
    }
    /// <summary>
    /// 判断状态管理器是否包含指定类型的状态。
    /// </summary>
    /// <param name="type">状态类型。</param>
    /// <returns>如果包含返回 true，否则 false。</returns>
    public virtual bool ContainsStateOfType(Type type) => m_state.ContainsKey(type);
    public virtual void ManagerStep()
    {
        if (current != null && Time.timeScale > 0)
        {
            current.StateStep(entity);
        }
    }

    public virtual void Change(int to)
    {
        if (to >= 0 && to < m_list.Count)
        {
            Change(m_list[to]);
        }
    }

    public virtual void Change<TState>() where TState : EntityState<T>
    {
        var type = typeof(TState);
        if (m_state.ContainsKey(type))
        {
            Change(m_state[type]);
        }
    }

    public virtual void Change(EntityState<T> to)
    {
        if (to != null && Time.timeScale > 0)
        {
            if (current != null)
            {
                // ① 调用旧状态的退出逻辑
                current.Exit(entity);
                // ② 触发“退出旧状态”事件
                events.onExit.Invoke(current.GetType());
                // ③ 把当前状态存起来，方便后面回退
                last = current;
            }
            // ① 把当前状态换成新状态
            current = to;
            // ② 调用新状态的进入逻辑
            current.Enter(entity);
            // ③ 触发“进入新状态”事件
            events.onEnter.Invoke(current.GetType());
            // ④ 触发“状态切换”事件（不管从哪切到哪，只要切换了就触发）
            events.onChange?.Invoke();
        }
    }
    /// <summary>
    /// 当实体与其他碰撞体接触时调用，将事件传递给当前状态。
    /// </summary>
    /// <param name="other">碰撞到的其他碰撞体。</param>
    public virtual void OnContact(Collider other)
    {
        if (current != null && Time.timeScale > 0)
        {
            current.OnContact(entity, other);
        }
    }
}
