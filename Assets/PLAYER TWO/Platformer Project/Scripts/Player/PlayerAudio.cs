using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Audio")]
public class PlayerAudio : MonoBehaviour
{
    [Header("Voices")]
    public AudioClip[] jump;
    public AudioClip[] hurt;
    public AudioClip[] attack;
    public AudioClip[] lift;
    public AudioClip[] maneuver;

    [Header("Effects")]
    public AudioClip spin;
    public AudioClip pickUp;
    public AudioClip drop;
    public AudioClip airDive;
    public AudioClip stompSpin;
    public AudioClip stompLanding;
    public AudioClip ledgeGrabbing;
    public AudioClip dash;
    public AudioClip startRailGrind;
    public AudioClip railGrind;

    [Header("Other Sources")]
    public AudioSource grindAudio;

    protected Player m_player;
    protected AudioSource m_audio;
    /// <summary>
    /// 生命周期：脚本开始运行时，初始化音频、玩家和回调
    /// </summary>
    protected virtual void Start()
    {
        InitializeAudio();      // 确保有 AudioSource
        InitializePlayer();     // 获取 Player 引用
        InitializeCallbacks();  // 绑定事件与音效
    }

    /// <summary>
    /// 从一组音效中随机选择并播放
    /// </summary>
    protected virtual void PlayRandom(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            var index = Random.Range(0, clips.Length); // 随机取一个下标
            if (clips[index]) // 确保选中的音频有效
                Play(clips[index]);
        }
    }

    /// <summary>
    /// 播放指定音效
    /// stopPrevious = true 时，会先停止当前正在播放的音效
    /// </summary>
    protected virtual void Play(AudioClip audio, bool stopPrevious = true)
    {
        if (audio == null)
            return;

        if (stopPrevious)
            m_audio.Stop();

        m_audio.PlayOneShot(audio); // 播放一次音效（不会打断正在播放的音乐）
    }

    /// <summary>
    /// 绑定玩家事件和音效回调
    /// </summary>
    protected virtual void InitializeCallbacks()
    {
        // 跳跃时播放随机跳跃音效
        m_player.playerEvents.OnJump.AddListener(() => PlayRandom(jump));

        // 受伤时播放随机受伤音效
        m_player.playerEvents.OnHurt.AddListener(() => PlayRandom(hurt));

        // 丢东西时播放 drop 音效（不会打断其它音效）
        m_player.playerEvents.OnThrow.AddListener(() => Play(drop, stopPrevious: false));

        // 踩踏开始时播放旋转转音效
        m_player.playerEvents.OnStompStarted.AddListener(() => Play(stompSpin, stopPrevious: false));

        // 踩踏落地时播放落地音效
        m_player.playerEvents.OnStompLanding.AddListener(() => Play(stompLanding));

        // 抓住边缘时播放 ledgeGrabbing 音效（不打断其它）
        m_player.playerEvents.OnLedgeGrabbed.AddListener(() => Play(ledgeGrabbing, stopPrevious: false));

        // 爬上边缘时播放 lift 音效（随机挑选）
        m_player.playerEvents.OnLedgeClimbing.AddListener(() => PlayRandom(lift));

        // 后空翻等特殊动作时播放 maneuver 音效
        m_player.playerEvents.OnBackflip.AddListener(() => PlayRandom(maneuver));

        // 冲刺开始时播放 dash 音效
        m_player.playerEvents.OnDashStarted.AddListener(() => Play(dash));

        // 离开滑轨时，停止 grindAudio
        m_player.entityEvents.OnRailsExit.AddListener(() => grindAudio?.Stop());

        // 拾取物品时：先播放 lift 音效，再叠加 pickUp 音效
        m_player.playerEvents.OnPickUp.AddListener(() =>
        {
            PlayRandom(lift);
            m_audio.PlayOneShot(pickUp);
        });

        // 旋转攻击时：播放 attack 音效，并叠加 spin 音效
        m_player.playerEvents.OnSpin.AddListener(() =>
        {
            PlayRandom(attack);
            m_audio.PlayOneShot(spin);
        });

        // 空中俯冲时：播放 attack 音效，并叠加 airDive 音效
        m_player.playerEvents.OnAirDive.AddListener(() =>
        {
            PlayRandom(attack);
            m_audio.PlayOneShot(airDive);
        });

        // 进入滑轨时：播放开始滑轨音效，并让 grindAudio 播放循环音效
        m_player.entityEvents.OnRailsEnter.AddListener(() =>
        {
            Play(startRailGrind, stopPrevious: false);
            grindAudio?.Play();
        });

        // 游戏暂停时：暂停玩家音频和滑轨音频
        LevelPauser.instance?.OnPause.AddListener(() =>
        {
            m_audio.Pause();
            grindAudio.Pause();
        });

        // 游戏恢复时：继续播放音效
        LevelPauser.instance?.OnUnpause.AddListener(() =>
        {
            m_audio.UnPause();
            grindAudio.UnPause();
        });
    }

    /// <summary>
    /// 初始化玩家引用
    /// </summary>
    protected virtual void InitializePlayer() => m_player = GetComponent<Player>();

    /// <summary>
    /// 初始化音频组件，如果物体上没有 AudioSource 就自动添加一个
    /// </summary>
    protected virtual void InitializeAudio()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }
}
