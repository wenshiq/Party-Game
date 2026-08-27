using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("PLAYER TWO/Platform Project/Entity/Player Event Linstener")]
public class PlayerEventListener : MonoBehaviour
{

    public Player player; // 玩家引用，可在 Inspector 中手动赋值
    public PlayerEvents events; // 玩家事件引用，可在 Inspector 中手动赋值
    /// <summary>
    /// Unity 生命周期方法。
    /// 在 Start 阶段调用，自动初始化玩家引用和事件回调。
    /// </summary>
    protected virtual void Start()
    {
        InitializePlayer();
        InitializeCallbacks();
    }

    /// <summary>
    /// 初始化玩家引用。
    /// 如果没有手动赋值 player，则会自动在父物体中寻找 Player 组件。
    /// </summary>
    protected virtual void InitializePlayer()
    {
        if (!player)
        {
            player = GetComponentInParent<Player>();
        }
    }

    /// <summary>
    /// 初始化玩家事件的回调。
    /// 将 Player 内部的事件（如跳跃、受伤、死亡等）转发到本地的 events 中，方便在 Inspector 中自定义逻辑。
    /// </summary>
    protected virtual void InitializeCallbacks()
    {
        player.playerEvents.OnJump.AddListener(() => events.OnJump.Invoke());
        player.playerEvents.OnHurt.AddListener(() => events.OnHurt.Invoke());
        player.playerEvents.OnDie.AddListener(() => events.OnDie.Invoke());
        player.playerEvents.OnSpin.AddListener(() => events.OnSpin.Invoke());
        player.playerEvents.OnPickUp.AddListener(() => events.OnPickUp.Invoke());
        player.playerEvents.OnThrow.AddListener(() => events.OnThrow.Invoke());
        player.playerEvents.OnStompStarted.AddListener(() => events.OnStompStarted.Invoke());
        player.playerEvents.OnStompFalling.AddListener(() => events.OnStompFalling.Invoke());
        player.playerEvents.OnStompLanding.AddListener(() => events.OnStompLanding.Invoke());
        player.playerEvents.OnStompEnding.AddListener(() => events.OnStompEnding.Invoke());
        player.playerEvents.OnLedgeGrabbed.AddListener(() => events.OnLedgeGrabbed.Invoke());
        player.playerEvents.OnLedgeClimbing.AddListener(() => events.OnLedgeClimbing.Invoke());
        player.playerEvents.OnAirDive.AddListener(() => events.OnAirDive.Invoke());
        player.playerEvents.OnBackflip.AddListener(() => events.OnBackflip.Invoke());
    }
}
