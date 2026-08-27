using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class FallingPlatform : MonoBehaviour, IEntityContact
{
    public float fallDelay = 2f;
    public bool autoReset = true;
    public float resetDelay = 5f;
    public float fallGravity = 40f;

    [Header("Shake Setting")]
    public bool shake = true;
    public float shakeSpeed = 45f;
    public float shakeHeight = 0.1f;

    public bool falling { get; protected set; }
    public bool activated { get; protected set; }

    protected Vector3 m_initialPosition;
    protected Collider m_collider;
    protected Collider[] m_overlaps = new Collider[32];


    protected void Start()
    {
        tag = GameTags.Spring;
        m_collider = GetComponent<Collider>();
        // 记录初始位置，掉落平台会掉落下去，然后如果开启了自动恢复，就能够回到原来的地方
        m_initialPosition = transform.position;
    }

    protected virtual void Update()
    {
        // 如果掉落激活了，那就每帧向下掉落
        if (falling)
        {
            transform.position += fallGravity * Vector3.down * Time.deltaTime;
        }
    }

    public void OnEntityContact(EntityBase entity)
    {
        // 与实体接触时，检测是不是player踩在它头上了
        if (entity.IsPointUnderStep(m_collider.bounds.max) && entity is Player player)
        {
            // 如果是，那就激活，要开始掉落了
            if (!activated)
            {
                activated = true;
                StartCoroutine(Routine());
            }
        }
    }
    // 掉落就开启掉落状态，打开trigger
    public virtual void Fall()
    {
        falling = true;
        m_collider.isTrigger = true;
    }

    public virtual void Restart()
    {
        // 这里是重设位置与状态，恢复原来的样子
        activated = falling = false;
        transform.position = m_initialPosition;
        m_collider.isTrigger = false;

        OffsetPlayer();
    }

    protected virtual void OffsetPlayer()
    {
        // 获取自身的center位置
        var center = m_collider.bounds.center;
        // 获取包围盒边界框的范围，总是size的一半
        var extents = m_collider.bounds.extents;
        // 最高点就是包围盒的顶部
        var maxY = m_collider.bounds.max.y;
        // 查找与给定盒体接触或位于盒体内部的所有碰撞体，并将它们存储到缓冲区中。
        var overlaps = Physics.OverlapBoxNonAlloc(center, extents, m_overlaps);
        for (int i = 0; i < overlaps; i++)
        {
            // 检查与它碰撞的物体中有没有player，不是player就跳过
            if (!m_overlaps[i].CompareTag(GameTags.Player))
            {
                continue;
            }

            // 是player，计算一下盒体顶部和player中心点的垂直距离
            var distance = maxY - m_overlaps[i].transform.position.y;
            // 获取player高度
            var height = m_overlaps[i].GetComponent<Player>().height;
            // 偏移量就是 distance + 身高一半
            var offset = Vector3.up * (distance + height * 0.5f);
            // 让player的坐标进行偏移
            m_overlaps[i].transform.position += offset;
        }
    }

    protected IEnumerator Routine()
    {
        // 前半段携程是让平台开始抖动
        var timer = fallDelay;
        while (timer >= 0)
        {
            if (shake && (timer <= fallDelay / 2f))
            {
                var shake = Mathf.Sin(Time.time * shakeSpeed) * shakeHeight;
                transform.position = m_initialPosition + Vector3.up * shake;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        // 后半段携程是为了等待恢复时间去恢复平台

        Fall();

        if (autoReset)
        {
            yield return new WaitForSeconds(resetDelay);
            Restart();
        }
    }
}
