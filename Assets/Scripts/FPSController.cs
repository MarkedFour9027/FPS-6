using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float maxSpeed = 7f;
    public float crouchSpeed = 3f;
    public float groundAccel = 14f;
    public float airAccel = 2f;
    public float groundFriction = 6f;
    public float gravity = -20f;
    public float jumpForce = 8f;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    float currentHeight;
    float currentSpeed;

    CharacterController controller;
    Vector3 velocity;

    public Transform cameraPivot;
    public float mouseSensitivity = 2.5f;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentHeight = controller.height;
        currentSpeed = maxSpeed;
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) x -= 1;
            if (Keyboard.current.dKey.isPressed) x += 1;
            if (Keyboard.current.sKey.isPressed) z -= 1;
            if (Keyboard.current.wKey.isPressed) z += 1;
        }

        bool crouching = Keyboard.current.ctrlKey.isPressed;        
        float targetHeight = crouching ? crouchHeight : standHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, dt * 10f);
        controller.height = currentHeight;
        maxSpeed = crouching ? crouchSpeed : currentSpeed;

        float mouseX = Mouse.current.delta.ReadValue().x * mouseSensitivity;
        float mouseY = Mouse.current.delta.ReadValue().y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        Vector3 inputDir = transform.right * x + transform.forward * z;
        Vector3 wishDir = inputDir.normalized;
        float wishSpeed = inputDir.magnitude * maxSpeed;

        bool grounded = controller.isGrounded;

        if (grounded)
        {
            // stick to ground
            if (velocity.y < 0)
                velocity.y = -2f; // small downward force to keep grounded


            ApplyFriction(groundFriction, dt);
            Accelerate(wishDir, wishSpeed, groundAccel, dt);

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                velocity.y = jumpForce;
                crouching = false;
            }
        }
        else
        {
            Accelerate(wishDir, wishSpeed, airAccel, dt);
        }

        velocity.y += gravity * dt;

        controller.Move(velocity * dt);
    }

    void ApplyFriction(float friction, float dt)
    {
        Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
        float speed = horizontal.magnitude;

        if (speed < 0.01f) return;

        float drop = speed * friction * dt;
        float newSpeed = Mathf.Max(speed - drop, 0);

        if (newSpeed != speed)
        {
            newSpeed /= speed;
            velocity.x *= newSpeed;
            velocity.z *= newSpeed;
        }
    }

    void Accelerate(Vector3 wishDir, float wishSpeed, float accel, float dt)
    {
        float currentSpeed = Vector3.Dot(velocity, wishDir);
        float addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0) return;

        float accelSpeed = accel * dt * wishSpeed;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        velocity += wishDir * accelSpeed;
    }
}
