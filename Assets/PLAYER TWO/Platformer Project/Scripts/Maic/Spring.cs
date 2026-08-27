using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

// 继承实体碰撞
public class Spring : MonoBehaviour, IEntityContact
{
    public float force = 25f;
    public AudioClip clip;

    protected Collider m_collider;
    protected AudioSource m_audio;

    public void ApplyForce(Player player)
    {
        // player落到垫子上时，给player应用向上的力，播放音效
        if (player.verticalVelocity.y <= 0)
        {
            m_audio.PlayOneShot(clip);
            player.verticalVelocity = Vector3.up * force;
        }
    }

    public void OnEntityContact(EntityBase entity)
    {
        // 实体是否是踩在垫子上方，踩垫子的是player，player还活着
        if (entity.IsPointUnderStep(m_collider.bounds.max) && entity is Player player && player.isAlive)
        {
            // 加力
            ApplyForce(player);
            // 算一次跳跃并重置dash和spin
            player.SetJumps(1);
            player.ResetAirDash();
            player.ResetAirSpin();
            // 切换到掉落状态
            player.states.Change<FallPlayerState>();
        }
    }

    protected void Start()
    {
        tag = GameTags.Spring;
        m_collider = GetComponent<Collider>();

        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }
}