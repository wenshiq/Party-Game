using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlayerState : PlayerState
{
    public override void OnContact(Player player, Collider other)
    {
        player.WallDrag(other);
        player.GrabPole(other);
        player.PushRigidbody(other);
    }

    protected override void OnEnter(Player player)
    {

    }

    protected override void OnExit(Player player)
    {

    }

    protected override void OnStep(Player player)
    {
        player.SnapToGround();
        player.Gravity();
        player.FaceDirectionSmooth(player.lateralVelocity);
        player.AccelerateToInputDirection();
        player.Spin();
        player.Jump();
        player.Dash();
        player.Glide();
        player.PickAndThrow();
        player.AirDive();//ø’÷–∏©≥Â
        player.LedgeGrab();

        player.StompAttack();
        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }
    }
}
