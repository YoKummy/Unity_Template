using UnityEngine;

public class WalkingState : BaseState
{
    public override void EnterState(PlayerState player)
    {
        player.currentSpeed = player.walkSpeed;
    }

    public override void UpdateState(PlayerState player)
    {
        if(player.isRunning) player.SwitchState(player.runningState);
        else return;
    }

    public override void ExitState(PlayerState player)
    {

    }
}