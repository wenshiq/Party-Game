using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 踩了按钮要触发一些东西，在这里触发
public class Toggle : MonoBehaviour
{
    // 触发延迟是多少
    public float delay;
    // 触发状态
    public bool state = true;
    // 如果需要控制多个触发器，就通过这个列表管理
    public Toggle[] multiTrigger;
    // 触发时发送事件
    public UnityEvent OnActivate;
    // 关闭时发送事件
    public UnityEvent OnDeactivate;
    // 为了避免递归死循环调用，我这里加了一个判断条件，用来判断当前物体的toggle携程是否在运行
    protected bool m_isRunning = false;

    protected virtual IEnumerator SetRoutine(bool value)
    {
        yield return new WaitForSeconds(delay);

        // 判断的逻辑我就不多说了，主要是递归的逻辑

        if (value)
        {
            if (!state)
            {
                state = true;

                // 循环遍历列表里面物体的toggle，并修改它们的状态
                foreach (var toggle in multiTrigger)
                {
                    // 注意，这里开启的是列表里面物体的toggle携程，和我们正在运行的这个携程没有关系，set方法里面的 停止所有携程 不会影响到我们这里运行的这个携程。
                    toggle.Set(state);
                }

                OnActivate?.Invoke();
            }
        }
        else if (state)
        {
            state = false;

            foreach (var toggle in multiTrigger)
            {
                toggle.Set(state);
            }

            OnDeactivate?.Invoke();
        }
        m_isRunning = false;
    }
    // 这个m_isRunning是防啥的呢？假如你在场景里挂脚本，在A物体上面加了toggle，列表里面包含B物体，然后B物体上也挂了Toggle，里面列表包含A物体，就会循环调用。虽然可以在拖拽的时候避免这个问题，但是你防不住粗心，所以在这里加一个条件，如果A物体的Toggle携程正在运行的时候，B物体就不能修改它的状态。
    public void Set(bool value)
    {
        if (!m_isRunning)
        {
            m_isRunning = true;
            StopAllCoroutines();
            // 开启携程，开启的是挂载这个脚本的物体的携程
            StartCoroutine(SetRoutine(value));
        }
    }
}
