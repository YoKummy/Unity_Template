using UnityEngine;

public class ConvoState : BaseState
{
    
    public override void EnterState(PlayerState player)
    {
        player.StartCoroutine(player.TalkToNpc());
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.movement.action.Disable();
        player.run.action.Disable();
        player.interact.action.Disable();
        player.look.action.Disable();
        player.drop.action.Disable();
    }

    public override void UpdateState(PlayerState player)
    {
        Debug.Log($"Cursor State: {Cursor.lockState}, Visible: {Cursor.visible}");
        if(!DialogueManager.GetInstance().DialogueisPlaying)
        {
            
            player.SwitchState(player.walkingState);
            player.canInteract = true;
        }
    }

    public override void ExitState(PlayerState player)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.movement.action.Enable();
        player.run.action.Enable();
        player.interact.action.Enable();
        player.look.action.Enable();
        player.drop.action.Enable();
    }
}