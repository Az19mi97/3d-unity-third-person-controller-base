using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Mobile")]
    public MobileJoystick mobileJoystick;
    public GameObject mobileControls;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("Buttons")]
    public GameObject jumpButton;
    public GameObject sprintButton;

    [Header("Animator")]
    public Animator animator;

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
        if (Application.isMobilePlatform)
            EnhancedTouchSupport.Enable();

        bool isMobile = Application.isMobilePlatform;

        if (mobileControls != null)
            mobileControls.SetActive(isMobile);

        if (jumpButton != null)
        {
            jumpButton.SetActive(isMobile);
            if (isMobile)
                PositionButtonBottomRight(jumpButton.GetComponent<RectTransform>(), 150, 150);
        }

        if (sprintButton != null)
        {
            sprintButton.SetActive(isMobile);
            if (isMobile)
                PositionButtonBottomRight(sprintButton.GetComponent<RectTransform>(), 150, 300);
        }
    }

    private void PositionButtonBottomRight(RectTransform button, float xOffset, float yOffset)
    {
        if (button == null) return;
        button.anchorMin = new Vector2(1, 0);
        button.anchorMax = new Vector2(1, 0);
        button.pivot = new Vector2(1, 0);
        button.anchoredPosition = new Vector2(-xOffset, yOffset);
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
        HandleMovement();
        UpdateAnimator();
    }

    private void HandleMovement()
    {
        // Saml input
        Vector2 input = moveInput;
        if (mobileJoystick != null)
        {
            float h = mobileJoystick.Horizontal();
            float v = mobileJoystick.Vertical();
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
                input = new Vector2(h, v);
        }

        // Beregn retning i forhold til kamera
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 direction = (camForward * input.y + camRight * input.x);

        // Hvis ingen input, stop vandret bevægelse
        if (direction.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        direction.Normalize();
        float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // Flyt via MovePosition (stabil bevægelse)
        Vector3 move = rb.position + direction * speed * Time.fixedDeltaTime;
        move.y = rb.position.y; // Bevar y for jump/gravity
        rb.MovePosition(move);

        // Rotation mod retning
        RotateTowards(direction);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void Jump()
    {
        if (!isGrounded) return;

        // Nulstil Y velocity før hop
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animator != null)
            animator.SetTrigger("Jump");
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        if (animator != null)
            animator.SetBool("IsGrounded", isGrounded);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Brug faktisk vandret hastighed
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speedPercent = horizontalVel.magnitude / (moveSpeed * sprintMultiplier);
        animator.SetFloat("Speed", Mathf.Clamp01(speedPercent), 0.1f, Time.deltaTime);
    }

    // UI knapper
    public void JumpButton() => Jump();
    public void SprintButtonDown() => sprinting = true;
    public void SprintButtonUp() => sprinting = false;
}