using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    public TextAsset inkJSON;

    public void Interact(PlayerState playerState)
    {
        Transform npcHead = transform.Find("neck");
        if (npcHead != null)
        {
            playerState.ZoomVcam.LookAt = npcHead; 
        }

        DialogueManager.GetInstance().EnterDialogue(inkJSON);

        playerState.SwitchState(playerState.convoState);
        playerState.canInteract = false;
    }
}