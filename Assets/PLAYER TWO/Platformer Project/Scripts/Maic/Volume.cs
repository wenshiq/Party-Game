using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用于检测物体进入或离开某个区域时触发事件，并可播放对应的音效。
/// </summary>
[RequireComponent(typeof(Collider))] // 强制挂载碰撞体组件
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Volume")] // Unity编辑器菜单路径
public class Volume : MonoBehaviour
{

    /// <summary>
    /// 当有物体进入触发区域时调用的事件。
    /// </summary>
    public UnityEvent onEnter;

    /// <summary>
    /// 当有物体离开触发区域时调用的事件。
    /// </summary>
    public UnityEvent onExit;

    /// <summary>
    /// 进入区域时播放的音效。
    /// </summary>
    public AudioClip enterClip;

    /// <summary>
    /// 离开区域时播放的音效。
    /// </summary>、
    public AudioClip exitClip;
    /// <summary>
    /// 当前物体上的触发器碰撞体。
    /// </summary>
    protected Collider m_collider;

    protected AudioSource m_audio;

    protected virtual void Start()
    {
        InitializeCollider();
        InitializeAudioSource();
    }

    /// <summary>
    /// 初始化Collider，并将其设为触发器（Trigger）。
    /// </summary>
    protected virtual void InitializeCollider()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true; // 开启触发器模式，只检测不物理碰撞
    }

    
    /// <summary>
    /// 初始化AudioSource，如果没有则自动添加。
    /// </summary>
    protected virtual void InitializeAudioSource()
    {
        // 尝试获取音源组件，不存在就自动创建
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
        // 空间混合：0=纯2D音效，1=纯3D，0.5=半2D半3D
        m_audio.spatialBlend = 0.5f;
    }

    /// <summary>
    /// 当其他物体进入触发区域时调用。
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        // 检查进入物体的边界点是否完全在本区域内
        if (!m_collider.bounds.Contains(other.bounds.max) ||
            !m_collider.bounds.Contains(other.bounds.min))
        {
            // 播放进入音效
            m_audio.PlayOneShot(enterClip);
            // 触发进入事件
            onEnter?.Invoke();
        }
    }
    /// <summary>
    /// 当其他物体离开触发区域时调用。
    /// </summary>
    protected virtual void OnTriggerExit(Collider other)
    {
        // 检查物体的位置是否不在区域内
        if (!m_collider.bounds.Contains(other.transform.position))
        {
            // 播放离开音效
            m_audio.PlayOneShot(exitClip);
            // 触发离开事件
            onExit?.Invoke();
        }
    }
}
