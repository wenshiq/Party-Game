using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EntityVolumeEffect : MonoBehaviour
{
    public float velocityConversion = 1f;
    public float accelerationMultiplier = 1f;
    public float topSpeedMultiplier = 1f;
    public float decelerationMultiplier = 1f;
    public float turningDragMultiplier = 1f;
    public float gravityMultiplier = 1f;

    protected Collider m_collider;

    protected void Start()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    // 这里就是定义了一堆在液体中需要改变的阻力等等，在沼泽中要有费力的感觉
    protected void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out EntityBase entity))
        {
            entity.velocity *= velocityConversion;
            entity.accelerationMultiplier = accelerationMultiplier;
            entity.topSpeedMultiplier = topSpeedMultiplier;
            entity.decelerationMultiplier = decelerationMultiplier;
            entity.turningDragMultiplier = turningDragMultiplier;
            entity.gravityMultiplier = gravityMultiplier;
        }
    }
    // 离开了自然要恢复
    protected void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out EntityBase entity))
        {
            entity.accelerationMultiplier = 1f;
            entity.topSpeedMultiplier = 1f;
            entity.decelerationMultiplier = 1f;
            entity.turningDragMultiplier = 1f;
            entity.gravityMultiplier = 1f;
        }
    }
}