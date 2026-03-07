using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

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
        if (Keyboard.current.zKey.isPressed)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = camForward * moveInput.y + camRight * moveInput.x;

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
}