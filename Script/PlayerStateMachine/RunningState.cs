using UnityEngine;

public class RunningState : BaseState
{
    public override void EnterState(PlayerState player)
    {
        
    }

    public override void UpdateState(PlayerState player)
    {
        player.Move();
        player.Run();
        player.Look();
    }

    public override void ExitState(PlayerState player)
    {
        
    }
}