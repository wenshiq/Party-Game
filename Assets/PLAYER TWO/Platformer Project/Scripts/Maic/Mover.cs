using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 按钮踩下去要移动
public class Mover : MonoBehaviour
{
    // 移动的距离
    public Vector3 offset;
    // 移动的时间
    public float duration;
    // 恢复的时间
    public float resetDuration;
    // 初始的位置
    protected Vector3 m_initialPosition;

    protected virtual IEnumerator ApplyOffsetRoutine(Vector3 from, Vector3 to, float duration)
    {
        // 已经移动了的时间
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // 移动的比例
            var t = elapsedTime / duration;
            // 插值运算，从起始点到结束点，按比例移动
            transform.localPosition = Vector3.Lerp(from, to, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 最后到达目的地
        transform.localPosition = to;
    }

    public virtual void ApplyOffset()
    {
        StopAllCoroutines();
        StartCoroutine(ApplyOffsetRoutine(m_initialPosition, m_initialPosition + offset, duration));
    }

    public virtual void Reset()
    {
        StopAllCoroutines();
        // 开启携程移动按钮
        StartCoroutine(ApplyOffsetRoutine(transform.localPosition, m_initialPosition, resetDuration));
    }

    protected void Start()
    {
        // 初始化初始位置，别写错了，世界坐标的话按钮就飞了，因为后面处理的都是本地坐标
        m_initialPosition = transform.localPosition;
    }
}
