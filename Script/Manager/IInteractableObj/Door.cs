using UnityEngine;
using Ink.Runtime;
public class Door : MonoBehaviour, IInteractable
{
    public void Interact(PlayerState playerState)
    {
        bool pickup = ((BoolValue)DialogueManager.GetInstance().GetVariableState("pickVhs")).value;
        if (pickup)
        {
            GameManager.GetInstance().loadScene(1);
        }
        else
        {
            NarrativeManager.GetInstance().EnterNarrative();
        }
    }
}
