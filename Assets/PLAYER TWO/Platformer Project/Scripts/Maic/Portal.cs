using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal exit;
    public bool useFlash = true;
    public float exitOffset = 1f;
    public AudioClip teleportClip;

    protected Collider m_collider;
    protected AudioSource m_audio;
    protected PlayerCamera m_camera;

    public Vector3 position => transform.position;
    public Vector3 forward => transform.forward;

    protected void Start()
    {
        m_collider = GetComponent<Collider>();
        m_audio = GetComponent<AudioSource>();
        m_camera = FindObjectOfType<PlayerCamera>();
        m_collider.isTrigger = true;
    }
    // 当进入传送门时
    protected void OnTriggerEnter(Collider other)
    {
        // 存在对向出口，且是player接触到的传送门
        if (exit && other.TryGetComponent(out Player player))
        {
            // 计算player的y值与传送门的y值之间的插值，传送过去之后要让player在正确的y值上
            var yOffset = player.unsizedPosition.y - transform.position.y;
            // 传送过去，加上这个插值
            player.transform.position = exit.position + Vector3.up * yOffset;
            // 更改player朝向，出口方向
            player.FaceDirection(exit.forward);
            // 重置相机
            m_camera.Reset();
            // 获取相机方向
            var inputDirection = player.inputs.GetMovementCameraDirection();
            // 如果相机方向和出口传送门方向反向
            if (Vector3.Dot(inputDirection, exit.forward) < 0)
            {
                // 修改玩家朝向，反过来
                player.FaceDirectionSmooth(-exit.forward);
            }
            // 把player挪出传送门的trigger范围以免无限传送
            player.transform.position += player.transform.forward * exit.exitOffset;
            // 按新方向重置速度
            player.lateralVelocity = player.transform.forward * player.lateralVelocity.magnitude;
            // 如果传送的时候要闪一下屏幕
            if (useFlash)
            {
                // 触发这个闪屏效果
                Flash.instance?.Trigger();
            }
            // 播放传送门音效
            m_audio.PlayOneShot(teleportClip);
        }
    }
}
