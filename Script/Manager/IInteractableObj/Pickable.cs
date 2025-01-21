using UnityEngine;
using UnityEngine.XR;

public class Pickable : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName; // Unique name to identify the item in hand

    public void Interact(PlayerState playerState)
    {
        if(playerState.heldObj == null)
        {
            PickUpObject(gameObject);
        }

        void PickUpObject(GameObject pickUpObj)
        {
            if (pickUpObj.GetComponent<Rigidbody>()) //make sure the object has a RigidBody
            {
                playerState.heldObj = pickUpObj; //assign heldObj to the object that was hit by the raycast (no longer == null)
                playerState.heldObjRb = pickUpObj.GetComponent<Rigidbody>(); //assign Rigidbody
                playerState.heldObjRb.isKinematic = true;
                playerState.heldObjRb.transform.parent = playerState.holdPos.transform; //parent object to holdposition
                playerState.heldObj.layer = playerState.LayerNumber; //change the object layer to the holdLayer
                //make sure object doesnt collide with player, it can cause weird bugs
                Physics.IgnoreCollision(playerState.heldObj.GetComponent<Collider>(), playerState.player.GetComponent<Collider>(), true);
            }
        }
    }
}