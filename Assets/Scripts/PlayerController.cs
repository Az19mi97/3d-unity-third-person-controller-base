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

    [Header("Mobile Controls (optional)")]
    public MobileJoystick joystick;
    public GameObject jumpButton;
    public GameObject sprintButton;

    private Rigidbody rb;
    private PlayerControls controls;

    private Vector3 currentVelocity;
    private Vector3 inputDirection;

    private bool isGrounded;
    private bool sprinting;
    private bool jumpPressed;
    private bool sprintPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // Mobile-only UI visibility
#if UNITY_ANDROID || UNITY_IOS
        if (joystick != null) joystick.gameObject.SetActive(true);
        if (jumpButton != null) jumpButton.SetActive(true);
        if (sprintButton != null) sprintButton.SetActive(true);
#else
        if (joystick != null) joystick.gameObject.SetActive(false);
        if (jumpButton != null) jumpButton.SetActive(false);
        if (sprintButton != null) sprintButton.SetActive(false);
#endif
    }

    private void OnEnable()
    {
        // Keyboard input (PC/Mac)
        controls.Player.Move.performed += ctx => SetKeyboardInput(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => SetKeyboardInput(Vector2.zero);

        controls.Player.Jump.performed += _ => Jump();
        controls.Player.Sprint.performed += _ => sprinting = true;
        controls.Player.Sprint.canceled += _ => sprinting = false;

        // Touch input (Input System, optional)
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

        // Mobil joystick input
        if (joystick != null && joystick.gameObject.activeSelf)
        {
            Vector3 joyDir = new Vector3(joystick.Horizontal(), 0f, joystick.Vertical());
            if (joyDir.magnitude > 0.1f)
                direction = joyDir;
        }

        // Sprint logik (PC eller mobil UI)
        float speed = sprinting || sprintPressed ? maxSpeed * 1.5f : maxSpeed;

        direction = direction.normalized;
        Vector3 targetVelocity = direction * speed;

        // Acceleration / deceleration
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

        // Jump via mobil UI
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
    public void JumpButtonPressed() => jumpPressed = true;
    public void SprintButtonDown() => sprintPressed = true;
    public void SprintButtonUp() => sprintPressed = false;
}