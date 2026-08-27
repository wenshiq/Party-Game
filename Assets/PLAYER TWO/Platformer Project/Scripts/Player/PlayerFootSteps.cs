using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerFootsteps : MonoBehaviour
{
    [System.Serializable]
    public class Surface
    {
        public string tag;
        public AudioClip[] footsteps;
        public AudioClip[] landings;
    }

    [Header("General Settings")]
    public float footstepVolume = 0.3f;
    public float stepOffset = 1.25f;
    public AudioClip[] defaultFootsteps;
    public AudioClip[] defaultLandings;
    public Surface[] surfaces;

    protected Player m_player;
    protected AudioSource m_audio;
    protected Vector3 m_lastLateralPosition;
    protected Dictionary<string, AudioClip[]> m_landings = new Dictionary<string, AudioClip[]>();
    protected Dictionary<string, AudioClip[]> m_footsteps = new Dictionary<string, AudioClip[]>();
    // 播放随机音效
    protected virtual void PlayRandomClip(AudioClip[] clips)
    {
        if (clips.Length > 0)
        {
            var index = Random.Range(0, clips.Length);
            m_audio.PlayOneShot(clips[index], footstepVolume);
        }
    }

    protected virtual void Landing()
    {
        if (!m_player.onWater) // 不在水里的时候才有落地的声音
        {
            if (m_landings.ContainsKey(m_player.groundHit.collider.tag)) // 检查地面是啥类型的
            {
                PlayRandomClip(m_landings[m_player.groundHit.collider.tag]);
            }
            else // 啥也不是就播放默认音效
            {
                PlayRandomClip(defaultLandings);
            }
        }
    }

    protected void Start()
    {
        m_player = GetComponent<Player>();
        // 监听，落地的时候发出落地声
        m_player.entityEvents.OnGroundEnter.AddListener(Landing);

        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        foreach (var surface in surfaces) // 初始化音效数组
        {
            m_footsteps.Add(surface.tag, surface.footsteps);
            m_landings.Add(surface.tag, surface.landings);
        }
    }

    protected virtual void Update()
    {
        // 如果玩家在地面上，且在走路状态
        if (m_player.isGrounded && m_player.states.IsCurrentOfType(typeof(WalkPlayerState)))
        {
            // 走路得走出一步的距离了，才能有脚步声，不然你移动0.001也要发出脚步声，就炸了
            var position = transform.position;
            var lateralPosition = new Vector3(position.x, 0, position.z);
            var distance = (m_lastLateralPosition - lateralPosition).magnitude;
            // 算一下，移动的距离超出我们规定的一步的距离之后
            if (distance >= stepOffset)
            {
                // 检查并播放脚步声
                if (m_footsteps.ContainsKey(m_player.groundHit.collider.tag))
                {
                    PlayRandomClip(m_footsteps[m_player.groundHit.collider.tag]);
                }
                else
                {
                    PlayRandomClip(defaultFootsteps);
                }
                // 记录上一步发出声音的位置
                m_lastLateralPosition = lateralPosition;
            }
        }
    }
}
