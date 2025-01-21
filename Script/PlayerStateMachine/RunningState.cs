using UnityEngine;

public class RunningState : BaseState
{
    public override void EnterState(PlayerState player)
    {
        player.currentSpeed = player.runSpeed;
    }

    public override void UpdateState(PlayerState player)
    {
        if (!player.isRunning) player.SwitchState(player.walkingState);
        else return;
    }

    public override void ExitState(PlayerState player)
    {
        
    }
}