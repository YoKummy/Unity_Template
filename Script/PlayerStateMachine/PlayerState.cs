using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Cinemachine;
using Ink.Runtime;
using Unity.VisualScripting;

public class PlayerState : MonoBehaviour
{
    public LookAtPlayer lookAtPlayer;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] public InputActionReference movement;
    [SerializeField] public InputActionReference run;
    [SerializeField] public InputActionReference interact;
    [SerializeField] public InputActionReference look;
    [SerializeField] public InputActionReference drop;
    [SerializeField] public InputActionReference tool;
    [SerializeField] private Transform Camera;

    [Header("Movement")]
    [SerializeField] public float walkSpeed{ get; private set;} = 1.5f;
    [SerializeField] public float runSpeed{ get; private set;} = 2f;
    public float currentSpeed;
    [SerializeField] private float sensitivity = 10f;
    public bool isMoving{ get; private set;} = false;
    public bool isRunning{ get; private set;} = false;
    public bool isDrop{get; private set;} = false;
    public bool isToggle{get; private set;} = false;
    private Rigidbody rb;
    private float mouseX;
    private float mouseY;

    public bool canInteract = true;
    private float interactDistance = 2.3f;

    [Header("Cursor")]
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private Sprite defaultCursor;
    [SerializeField] private Sprite hitCursor;
    [SerializeField] private Image cursor;
    [Header("Hand")]
    public GameObject player;
    public Transform holdPos;

    public float throwForce = 500f;
    public float pickUpRange = 5f;
    public GameObject heldObj; //object which we pick up
    public Rigidbody heldObjRb; //rigidbody of object we pick up
    public int LayerNumber;
    [SerializeField] public GameObject flashlight;
    
    [Header("Other")]
    public CinemachineVirtualCamera PlayerVcam;
    public CinemachineVirtualCamera ZoomVcam;
    Story Narrative;
    [SerializeField] private TextAsset narrativeJSON;

    BaseState currentState;
    public WalkingState walkingState = new WalkingState();
    public RunningState runningState = new RunningState();
    public InteractingState interactingState = new InteractingState();
    public OnPcState onPcState = new OnPcState();
    public ConvoState convoState = new ConvoState();

    /* private void OnEnable()
    {
        Ticker.OnTickAction += Tick;
    }

    private void OnDisable()
    {
        Ticker.OnTickAction -= Tick;
    } */

    private void Awake()
    {
        drop.action.Enable();
        movement.action.Enable();
        run.action.Enable();
        interact.action.Enable();
        look.action.Enable();
        tool.action.Enable();
        Narrative = new Story(narrativeJSON.text);
    }

    void Start()
    {
        LayerNumber = LayerMask.NameToLayer("HoldItem");
        cursor.sprite = defaultCursor;
        rb = GetComponent<Rigidbody>();
        currentState = walkingState;
        currentState.EnterState(this);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        currentState.UpdateState(this);
        Move();
        Look();
        HandleRun();
        DropPress();
        DropItem();
        HandleToggle();
        if(isToggle) ToggleTool();
        if(canInteract) HandleRaycast();
    }

    /* private void Tick()
    {
        
    } */

    public void SwitchState(BaseState state)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }

        currentState = state;
        state.EnterState(this);
    }

    public void Move()
    {
        Vector2 val = movement.action.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(val.x, 0, val.y);

        isMoving = val.magnitude > 0.1f;

        if (isMoving)
        {
            moveDirection = moveDirection.normalized;
            moveDirection = transform.TransformDirection(moveDirection);
            rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
        }
    }

    public void HandleRun()
    {

        isRunning = run.action.ReadValue<float>() > 0;
    }
    public void DropPress()
    {
        isDrop = drop.action.ReadValue<float>() > 0;
    }

    public void HandleToggle()
    {
        isToggle = tool.action.ReadValue<float>() > 0;
    }

    public void ToggleTool()
    {
        isToggle = flashlight.activeSelf;
        flashlight.SetActive(!isToggle);
    }

    public void Look()
    {
        Vector2 lookInput = look.action.ReadValue<Vector2>();
//  * Time.deltaTime
        mouseX += lookInput.y * sensitivity;
        mouseY += lookInput.x * sensitivity;

        mouseX = Mathf.Clamp(mouseX, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(0, mouseY, 0);
        
        Camera.localRotation = Quaternion.Euler(-mouseX, 0, 0);
/* 
        Vector3 handOffset = new Vector3(0.2f, -0.2f, 0.4f);
        Hand.position = Camera.position + Camera.rotation * handOffset;

        Hand.rotation = Quaternion.Euler(-mouseX, Camera.eulerAngles.y, 0); */
    }

    public void DropItem()
    {
        if(heldObj != null)
        {   
            MoveObject();
            if(isDrop)
            {
                StopClipping();
                ThrowObject();
            }
        }
    }

    public void HandleRaycast()
    {
        Ray ray = new Ray(Camera.position, Camera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                cursor.sprite = hitCursor;

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    interactable.Interact(this);
                }
            }
            else
            {
                cursor.sprite = defaultCursor;
            }
        }
        else
        {
            cursor.sprite = defaultCursor;
        }
    }


    public IEnumerator TalkToNpc()
    {
        ZoomVcam.enabled = true;
        PlayerVcam.enabled = false;

        lookAtPlayer.lookAt();
        yield return StartCoroutine(WaitForConversationToEnd());

        ZoomVcam.enabled = false;
        PlayerVcam.enabled = true;
    }


    public IEnumerator WaitForConversationToEnd()
    {
        while (DialogueManager.GetInstance().DialogueisPlaying)
        {
            yield return null;
        }
    }

    public void DropObject()
    {
        //re-enable collision with player
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 6; //object assigned back to default layer
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null; //unparent object
        heldObj = null; //undefine game object
    }

    public void MoveObject()
    {
        //keep object position the same as the holdPosition position
        heldObj.transform.position = holdPos.transform.position;
    }
    
    public void ThrowObject()
    {
        //same as drop function, but add force to object before undefining it
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 6;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }
    public void StopClipping() //function only called when dropping/throwing
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position); //distance from holdPos to the camera
        //have to use RaycastAll as object blocks raycast in center screen
        //RaycastAll returns array of all colliders hit within the cliprange
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
        //if the array length is greater than 1, meaning it has hit more than just the object we are carrying
        if (hits.Length > 1)
        {
            //change object position to camera position 
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f); //offset slightly downward to stop object dropping above player 
            //if your player is small, change the -0.5f to a smaller number (in magnitude) ie: -0.1f
        }
    }
}