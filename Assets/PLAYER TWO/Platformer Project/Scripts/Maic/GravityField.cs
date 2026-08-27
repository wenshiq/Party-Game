using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    public float upwardForce = 75f;

    protected Collider m_collider;

    protected void Start()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    protected void OnTriggerStay(Collider other)
    {
        // 就是在trigger范围内时，不在地面上时加一个向上的力，如果你想走进去就吹起来就不判断在不在地面上了
        if (other.CompareTag(GameTags.Player))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                if (player.isGrounded)
                {
                    player.verticalVelocity = Vector3.zero;
                }

                player.velocity += transform.up * upwardForce * Time.deltaTime;
            }
        }
    }
}
