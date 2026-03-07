using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;   // spilleren
    public float distance = 6f;
    public float height = 2f;
    public float mouseSensitivity = 0.2f;
    public float keyRotationSpeed = 120f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleMouseRotation();
        HandleKeyboardRotation();
        FollowTarget();
    }

    void HandleMouseRotation()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouse = Mouse.current.delta.ReadValue() * mouseSensitivity;
            yaw += mouse.x;
            pitch -= mouse.y;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }
    }

    void HandleKeyboardRotation()
    {
        if (Keyboard.current.zKey.isPressed)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                yaw -= keyRotationSpeed * Time.deltaTime;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                yaw += keyRotationSpeed * Time.deltaTime;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                pitch -= keyRotationSpeed * Time.deltaTime * 0.5f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                pitch += keyRotationSpeed * Time.deltaTime * 0.5f;

            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }
    }

    void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.back * distance;
        Vector3 finalPos = target.position + Vector3.up * height + direction;
        transform.position = finalPos;
        transform.rotation = rotation;
    }
}