using UnityEngine;

public abstract class BaseState
{
    public abstract void EnterState(PlayerState player);
    public abstract void ExitState(PlayerState player);
    public abstract void UpdateState(PlayerState player);
}