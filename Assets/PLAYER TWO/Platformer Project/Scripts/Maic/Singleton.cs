using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T m_instance;

    /// <summary>
    /// 公有静态属性，外部通过 Singleton.instance 获取单例对象。
    /// 如果当前还没有实例，会自动在场景中查找一个。
    /// </summary>
    public static T instance
    {
        get
        {
            // 如果实例为空，则在场景中查找类型 T 的对象
            if (m_instance == null)
            {
                m_instance = FindAnyObjectByType<T>();
            }
            return m_instance;
        }
    }

    /// <summary>
    /// Awake 生命周期方法，用于确保单例唯一性。
    /// 如果当前对象不是单例实例，则销毁自己，避免重复存在。
    /// </summary>
    protected virtual void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
