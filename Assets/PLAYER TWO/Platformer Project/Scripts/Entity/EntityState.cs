using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public abstract class EntityState<T> where T : Entity<T>
{
    public UnityEvent onEnter;

    public UnityEvent onExit;

    public float timeSinceEntered { get; protected set; }

    public void Enter(T entity)
    {
        // 重置计时
        timeSinceEntered = 0;
        // 触发外部注册的进入事件回调
        onEnter?.Invoke();
        // 调用子类自定义的进入逻辑
        OnEnter(entity);
    }

    public void Exit(T entity)
    {
        // 触发外部注册的退出事件回调
        onExit?.Invoke();
        // 调用子类自定义的退出逻辑
        OnExit(entity);
    }

    public void StateStep(T entity)
    {
        // 调用子类实现的持续运行逻辑
        OnStep(entity);
        // 累计该状态已持续的时间，单位秒
        timeSinceEntered += Time.deltaTime;
    }

    protected abstract void OnEnter(T entity);

    protected abstract void OnExit(T entity);

    protected abstract void OnStep(T entity);

    public abstract void OnContact(T entity, Collider other);
    public static EntityState<T> CreateListFromString(string typeName)
    {
        
            return (EntityState<T>)Activator.CreateInstance(Type.GetType(typeName));
       
    }
    public static List<EntityState<T>> CreateListFromStringArray(string[] array)
    {
        var list = new List<EntityState<T>>();

        foreach(var typeName in array)
        {
            list.Add(CreateListFromString(typeName));
        }

        return list;
    }
}
