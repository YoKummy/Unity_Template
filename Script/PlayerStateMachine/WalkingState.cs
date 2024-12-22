using UnityEngine;

public class WalkingState : BaseState
{
    public override void EnterState(PlayerState player)
    {
        
    }

    public override void UpdateState(PlayerState player)
    {
        player.Move();
        player.Look();
    }

    public override void ExitState(PlayerState player)
    {

    }
}