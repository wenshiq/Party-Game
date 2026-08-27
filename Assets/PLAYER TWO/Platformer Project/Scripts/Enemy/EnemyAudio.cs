using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [Header("Effects")]
    public AudioClip death; // 死亡音效


    protected Enemy m_enemy;
    protected AudioSource m_audio;

    protected virtual void InitializeEnemy()
    {
        m_enemy = GetComponent<Enemy>();
    }

    protected virtual void InitializeAudio()
    {
        // 没有AudioSource就加一个
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }

    protected virtual void InitializeCallbacks()
    {
        // 监听死亡，死了就放音效
        m_enemy.enemyEvents.OnDie.AddListener(() => m_audio.PlayOneShot(death));
    }

    protected void Start()
    {
        InitializeEnemy();
        InitializeAudio();
        InitializeCallbacks();
    }
}