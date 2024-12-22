using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerState : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputActionReference movement;
    [SerializeField] private InputActionReference run;
    [SerializeField] private InputActionReference interact;
    [SerializeField] private InputActionReference look;
    [SerializeField] private Transform Camera;
    
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float runSpeed = 1.5f;
    [SerializeField] private float sensitivity = 10f; // Added for mouse sensitivity

    private float currentSpeed;
    private bool isMoving = false;
    private bool isRunning = false;
    private bool isInteracting = false;
    private Rigidbody rb;
    private float mouseX;
    private float mouseY;

    BaseState currentState;
    public WalkingState WalkingState = new WalkingState();
    public RunningState RunningState = new RunningState();
    public InteractingState InteractingState = new InteractingState();
    public OnPcState OnPcState = new OnPcState();
    public ConvoState ConvoState = new ConvoState();

    private void Awake()
    {
        // Make sure to properly enable the input actions
        movement.action.Enable();
        run.action.Enable();
        interact.action.Enable();
        look.action.Enable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        
    }

    public void SwitchState(BaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    // Handle movement using Rigidbody and AddForce
    public void Move()
    {
        Vector2 val = movement.action.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(val.x, 0, val.y);

        // Update isMoving based on movement input magnitude
        isMoving = val.magnitude > 0.1f;

        if (isMoving)
        {
            moveDirection = moveDirection.normalized;
            moveDirection = transform.TransformDirection(moveDirection);
            rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
        }
    }

    // Handle running by switching speed
    public void Run()
    {
        isRunning = run.action.ReadValue<float>() > 0;
        currentSpeed = isRunning ? runSpeed : walkSpeed;
    }


    // Interact action (no logic added here, can be expanded)
    public void Interact(InputAction.CallbackContext context)
    {
        isInteracting = interact.action.ReadValue<float>() > 0;
    }

    public void Look()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Vector2 lookInput = look.action.ReadValue<Vector2>();

        mouseX += lookInput.y * sensitivity * Time.deltaTime;
        mouseY += lookInput.x * sensitivity * Time.deltaTime;

        transform.localRotation = Quaternion.Euler(0, mouseY, 0);
        mouseX = Mathf.Clamp(mouseX, -90f, 90f);
        Camera.localRotation = Quaternion.Euler(-mouseX, 0, 0);
    }

}