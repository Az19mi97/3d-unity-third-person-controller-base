using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("Mobile")]
    public MobileJoystick mobileJoystick;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("PC/Mac Buttons (optional)")]
    public GameObject jumpButton;
    public GameObject sprintButton;

    private Rigidbody rb;
    private PlayerControls controls;

    private Vector2 moveInput;
    private bool sprinting;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        rb.freezeRotation = true;
    }

    void Start()
    {
        // Vis kun knapper på mobile (Android/iOS)
        if (Application.isMobilePlatform)
        {
            if (jumpButton != null) jumpButton.SetActive(true);
            if (sprintButton != null) sprintButton.SetActive(true);
        }
        else
        {
            if (jumpButton != null) jumpButton.SetActive(false);
            if (sprintButton != null) sprintButton.SetActive(false);
        }

        // Fjerne den hvide firkant
        GameObject whiteBox = GameObject.Find("WhiteBox");
        if (whiteBox != null)
            Destroy(whiteBox);
    }

    void OnEnable()
    {
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Sprint.performed += ctx => sprinting = true;
        controls.Player.Sprint.canceled += ctx => sprinting = false;

        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void FixedUpdate()
    {
        CheckGround();
        MovePlayer();
    }

    void MovePlayer()
    {
        // PC kamera rotation stop
        if (Keyboard.current != null && Keyboard.current.zKey.isPressed)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector2 input = moveInput;

        // MOBILE joystick override
        if (mobileJoystick != null)
        {
            float h = mobileJoystick.Horizontal();
            float v = mobileJoystick.Vertical();

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
                input = new Vector2(h, v);
        }

        Vector3 direction = camForward * input.y + camRight * input.x;

        float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();

            Vector3 move = direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            RotateTowards(direction);
        }
    }

    void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smooth = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smooth);
    }

    void Jump()
    {
        if (!isGrounded) return;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
    }

    // UI BUTTONS (PC + MOBILE)

    public void JumpButton()
    {
        Jump();
    }

    public void SprintButtonDown()
    {
        sprinting = true;
    }

    public void SprintButtonUp()
    {
        sprinting = false;
    }
}