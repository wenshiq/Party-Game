using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public InputActionAsset actions;

    protected float m_movementDirectionUnlockTime;

    protected InputAction m_movement;
    protected InputAction m_look;
    protected InputAction m_jump;
    protected InputAction m_crouch;
    protected InputAction m_dash;
    protected InputAction m_stomp;
    protected InputAction m_spin;
    protected InputAction m_airDive;
    protected InputAction m_dive;
    protected InputAction m_glide;
    protected InputAction m_grindBrake;
    protected InputAction m_releaseledge;
    protected InputAction m_pause;
    protected InputAction m_run;
    protected InputAction m_pickAndDrop;
    protected Camera m_camera;

    protected const string k_mouseDeviceName = "Mouse";

    protected float? m_lastJumpTime;

    protected const float k_jumpBuffer = 0.15f;
    protected virtual void Awake()
    {
        CacheAction();
    }
    protected virtual void Start()
    {
        m_camera = Camera.main;
        actions.Enable();
    }

    protected virtual void Update()
    {
        // 记录跳跃按下时间，用于实现跳跃缓冲
        if (m_jump.WasPressedThisFrame())
        {
            m_lastJumpTime = Time.time;
        }
    }

    protected virtual void OnDisable() => actions?.Disable();
    protected virtual void OnEnable() => actions?.Enable();


    protected virtual void CacheAction()
    {
        m_movement = actions.FindAction("Movement");
        m_look = actions.FindAction("Look");
        m_jump = actions.FindAction("Jump");
        m_crouch = actions.FindAction("Crouch");
        m_dash = actions.FindAction("Dash");
        m_stomp = actions.FindAction("Stomp");
        m_spin = actions.FindAction("Spin");
        m_airDive = actions.FindAction("AirDive");
        m_dive = actions.FindAction("Dive");
        m_glide = actions.FindAction("Glide");
        m_grindBrake = actions.FindAction("Grind Brake");
        m_releaseledge = actions.FindAction("ReleaseLedge");
        m_pause = actions.FindAction("Pause");
        m_run = actions.FindAction("Run");
        m_pickAndDrop = actions.FindAction("PickAndDrop");
    }

    /// <summary>
    /// 获取观察方向输入（鼠标时直接返回，手柄时使用死区修正）
    /// </summary>
    public virtual Vector3 GetLookDirection()
    {
        var value = m_look.ReadValue<Vector2>();
        if (IsLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }
        return GetAxisWithCrossDeadZone(value);
    }

    /// <summary>
    /// 判断是否通过鼠标进行观察输入
    /// </summary>
    public virtual bool IsLookingWithMouse()
    {
        if (m_look.activeControl == null)
        {
            return false;
        }
        return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
    }

    /// <summary>
    /// 临时锁定移动方向输入
    /// </summary>
    /// <param name="duration">锁定时长（秒）</param>
    public virtual void LockMovementDirection(float duration = 0.25f)
    {
        m_movementDirectionUnlockTime = Time.time + duration;
    }
    public virtual Vector3 GetMovementDirection()
    {
        // ① 先检查：现在能不能移动？
        if (Time.time < m_movementDirectionUnlockTime)
            return Vector3.zero; // 还在硬直/锁定时间里，直接返回“不移动”的方向

        // ② 读你当前的移动输入（WASD/左摇杆）
        var value = m_movement.ReadValue<Vector2>();
        // 这里的var自动识别成Vector2，不用手写Vector2，简化代码

        // ③ 把读到的原始输入，传给下面的方法处理死区，再返回
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        // 读Unity默认的死区阈值（比如0.1）
        var deadzone = InputSystem.settings.defaultDeadzoneMin;

        // 处理X轴（左右：A/D键）的输入
        axis.x = Mathf.Abs(axis.x) > deadzone
            ? RemapToDeadzone(axis.x, deadzone) // 超过死区：重新映射输入值
            : 0; // 没超过死区：直接当成没按，设为0

        // 处理Y轴（前后：W/S键）的输入，逻辑和X轴一样
        axis.y = Mathf.Abs(axis.y) > deadzone
            ? RemapToDeadzone(axis.y, deadzone)
            : 0;

        // 把处理好的2D输入，转成3D游戏能用的Vector3
        // （3D游戏里“前后”是Z轴，所以把Y轴的值放到Z轴上，Y轴设为0，只处理水平移动）
        return new Vector3(axis.x, 0, axis.y);
    }

    protected float RemapToDeadzone(float value, float deadzone)
    => (value - (value > 0 ? deadzone : -deadzone)) / (1 - deadzone);


    public virtual Vector3 GetMovementCameraDirection()
    {
        // 1. 获取移动方向（通常是玩家输入的水平/垂直方向，比如 WSAD 或摇杆）
        var direction = GetMovementDirection();

        // 2. 如果有输入（不是零向量）
        if (direction.sqrMagnitude > 0)
        {
            // 3. 构建一个旋转；根据摄像机的 Y 轴角度（水平朝向）
            // Quaternion.AngleAxis(angle, axis) 表示绕某个轴旋转一个角度
            var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);

            // 4. 把原始输入方向旋转到摄像机的朝向下
            direction = rotation * direction;

            // 5. 归一化，保持方向向量的长度为 1（只保留方向）
            direction = direction.normalized;
        }

        // 6. 返回最终的世界空间移动方向
        return direction;
    }

    /// <summary>
    /// 判断是否触发跳跃（支持跳跃缓冲）
    /// </summary>
    public virtual bool GetJumpDown()
    {
        if (m_lastJumpTime != null &&
            Time.time - m_lastJumpTime < k_jumpBuffer)
        {
            m_lastJumpTime = null;
            return true;
        }
        return false;
    }

    public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();//玩家是否松开跳跃键
    public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();
    public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();
    public virtual bool GetSpinDown() => m_spin.WasReleasedThisFrame();
    public virtual bool GetAirDiveDown() => m_airDive.IsPressed();//玩家是否按下空中俯冲键
    public virtual bool GetCrouchAndCraw() => m_crouch.IsPressed();//玩家是否按下蹲伏键（包括蹲伏和匍匐）
    public virtual bool GetDive() => m_dive.IsPressed();//玩家是否按下潜水键（包括潜水和下潜）
    public virtual bool GetGrindBrake() => m_grindBrake.IsPressed();
    public virtual bool GetGlide() => m_glide.IsPressed();//玩家是否按下滑翔键（包括滑翔和下潜）
    public virtual bool GetReleaseLedgeDown() => m_releaseledge.WasReleasedThisFrame();
    public virtual bool GetPauseDown()=> m_pause.WasReleasedThisFrame();
    public virtual bool GetRun()=>m_run.IsPressed();
    public virtual bool GetRunUp()=>m_run.WasReleasedThisFrame();
    public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();
}
