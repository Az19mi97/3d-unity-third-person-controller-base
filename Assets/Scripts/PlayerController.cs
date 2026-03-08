using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
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
    // Aktiver Enhanced Touch (kun relevant på mobile)
    if (Application.isMobilePlatform)
        EnhancedTouchSupport.Enable();

    bool isMobile = Application.isMobilePlatform;

    // Vis / skjul mobile controls
    if (mobileControls != null)
        mobileControls.SetActive(isMobile);

    if (jumpButton != null)
    {
        jumpButton.SetActive(isMobile);
        if (isMobile)
            PositionButtonBottomRight(jumpButton.GetComponent<RectTransform>(), 150, 150); // distance fra bottom/right
    }

    if (sprintButton != null)
    {
        sprintButton.SetActive(isMobile);
        if (isMobile)
            PositionButtonBottomRight(sprintButton.GetComponent<RectTransform>(), 150, 300); // lidt ovenfor jump
    }

    // Fjerne den hvide firkant hvis den eksisterer
    GameObject whiteBox = GameObject.Find("WhiteBox");
    if (whiteBox != null)
        Destroy(whiteBox);

    // Skjul joystick på PC/Mac
    if (!isMobile && mobileJoystick != null)
    {
        if (mobileJoystick.joystickBackground != null)
            mobileJoystick.joystickBackground.gameObject.SetActive(false);
        if (mobileJoystick.joystickKnob != null)
            mobileJoystick.joystickKnob.gameObject.SetActive(false);
    }
}
    /// Positioner knapper nederst til højre på skærmen
    private void PositionButtonBottomRight(RectTransform button, float xOffset, float yOffset)
    {
        if (button == null) return;

        button.anchorMin = new Vector2(1, 0);  // nederst højre
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
        MovePlayer();
    }

    void MovePlayer()
    {
        // stop movement ved 2 finger kamera rotation
        if (Touch.activeTouches.Count >= 2)
            return;
            
        if (Keyboard.current != null && Keyboard.current.zKey.isPressed)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector2 input = moveInput;

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