using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个脚本的思路和PlayerAnimator一样，就是把动画机里面实际的变量名称转换成Hash值，通过状态机改变状态来改变动画
[RequireComponent(typeof(Enemy))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Parameters Names")]
    public string stateName = "State";
    public string lastStateName = "Last State";
    public string lateralSpeedName = "Lateral Speed";
    public string verticalSpeedName = "Vertical Speed";
    public string healthName = "Health";
    public string isGroundedName = "Is Grounded";
    public string onStateChangeName = "On State Changed";

    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_healthHash;
    protected int m_isGroundedHash;
    protected int m_onStateChangeHash;

    public Animator animator;

    protected Enemy m_enemy;
    // 初始化enemy
    protected virtual void InitializeEnemy() => m_enemy = GetComponent<Enemy>();
    // 初始化动画机的hash
    protected virtual void InitializeParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_onStateChangeHash = Animator.StringToHash(onStateChangeName);
    }
    // 初始化动画机的 初始状态
    protected virtual void InitializeAnimatorTriggers() =>
        m_enemy.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangeHash));

    protected void Start()
    {
        InitializeEnemy();
        InitializeParametersHash();
        InitializeAnimatorTriggers();
    }
    // 每帧检测怪物的状态变化
    protected void LateUpdate()
    {
        var lateralSpeed = m_enemy.lateralVelocity.magnitude;
        var verticalSpeed = m_enemy.verticalVelocity.y;
        // 用到的参数都是之前定义好的
        animator.SetInteger(m_stateHash, m_enemy.states.index);
        animator.SetInteger(m_lastStateHash, m_enemy.states.lastIndex);
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        animator.SetInteger(m_healthHash, m_enemy.health.current);
        animator.SetBool(m_isGroundedHash, m_enemy.isGrounded);
    }
}
