using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckPoint : MonoBehaviour
{
    public Transform respawn;
    public AudioClip clip;
    public UnityEvent OnActivate;

    protected Collider m_collider;
    protected AudioSource m_audio;

    public bool activated { get; protected set; }

    public virtual void Activate(Player player)
    {
        // 激活检查点
        if (!activated)
        {
            activated = true;
            m_audio.PlayOneShot(clip); // 播放音效
            player.SetRespawn(respawn.position, respawn.rotation); // 设置重生位置
            OnActivate?.Invoke();
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        // 如果检查点没激活过，就可以激活
        if (!activated && other.CompareTag(GameTags.Player))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                Activate(player);
            }
        }
    }

    protected void Awake()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }
}
