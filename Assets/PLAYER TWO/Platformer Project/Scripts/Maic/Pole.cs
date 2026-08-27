using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    // 杆子是个胶囊体碰撞体
    public new CapsuleCollider collider { get; protected set; }
    // 杆子的中心点
    public Vector3 center => transform.position;
    protected void Awake()
    {
        tag = GameTags.Pole;
        collider = GetComponent<CapsuleCollider>();
    }

    public Vector3 GetDirectionToPole(Transform other)
    {
        return GetDirectionToPole(other, out _);
    }

    public Vector3 GetDirectionToPole(Transform other, out float distance)
    {
        // 返回other到杆子中心的方向
        var target = new Vector3(center.x, other.position.y, center.z) - other.position;
        distance = target.magnitude;
        return target / distance;
    }
    // 将数据限制在杆子顶部-offset和杆子底部+offset的范围内
    public Vector3 ClampPointToPoleHeight(Vector3 point, float offset)
    {
        var minHeight = collider.bounds.min.y + offset;
        var maxHeight = collider.bounds.max.y - offset;
        var clampedHeight = Mathf.Clamp(point.y, minHeight, maxHeight);
        return new Vector3(point.x, clampedHeight, point.z);
    }
}
