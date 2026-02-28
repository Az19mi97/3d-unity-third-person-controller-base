using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Mobile Joystick (optional)")]
    public MobileJoystick joystick;

    [Header("Optional Mobile Buttons")]
    public bool jumpPressed = false;    // Til UI-knap
    public bool sprintPressed = false;  // Til UI-knap

    private Rigidbody rb;
    private PlayerControls controls;

    private Vector3 currentVelocity;
    private Vector3 inputDirection;

    private bool isGrounded;
    private bool sprinting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // Aktivér joystick kun på mobil
#if UNITY_STANDALONE || UNITY_EDITOR
        if (joystick != null)
            joystick.gameObject.SetActive(false);
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (joystick != null)
            joystick.gameObject.SetActive(true);
#endif
    }

    private void OnEnable()
    {
        // WASD / Arrow keys
        controls.Player.Move.performed += ctx => SetKeyboardInput(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => SetKeyboardInput(Vector2.zero);

        // Jump & Sprint (PC)
        controls.Player.Jump.performed += _ => Jump();
        controls.Player.Sprint.performed += _ => sprinting = true;
        controls.Player.Sprint.canceled += _ => sprinting = false;

        // Touch input (Input System)
        controls.Player.Touch.performed += OnTouchPerformed;
        controls.Player.Touch.canceled += ctx => sprinting = false;

        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
    }

    private void SetKeyboardInput(Vector2 input)
    {
        inputDirection = new Vector3(input.x, 0f, input.y);
    }

    private void HandleMovement()
    {
        Vector3 direction = inputDirection;

        // Mobil joystick override
        if (joystick != null && joystick.gameObject.activeSelf)
        {
            Vector3 joyDir = new Vector3(joystick.Horizontal(), 0f, joystick.Vertical());
            if (joyDir.magnitude > 0.1f)
                direction = joyDir;
        }

        // Sprint via joystick eller UI-knap
        float speed = sprinting || sprintPressed ? maxSpeed * 1.5f : maxSpeed;

        direction = direction.normalized;
        Vector3 targetVelocity = direction * speed;

        // Acceleration / Deceleration
        if (direction.magnitude > 0.1f)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            RotateTowards(direction);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);

        // Jump via UI-knap
        if (jumpPressed)
        {
            Jump();
            jumpPressed = false;
        }
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothRotation);
    }

    public void Jump()
    {
        if (!isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
    }

    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        Vector2 touchPos = context.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(touchPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            inputDirection = (targetPos - transform.position).normalized;
        }
    }

    // Offentlige metoder til UI-knapper
    public void JumpButtonPressed() => Jump();
    public void SprintButtonDown() => sprintPressed = true;
    public void SprintButtonUp() => sprintPressed = false;
}