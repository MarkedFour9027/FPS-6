using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float maxSpeed = 7f;
    public float crouchSpeed = 3f;
    public float runSpeed = 14f;
    public float groundAccel = 14f;
    public float airAccel = 2f;
    public float groundFriction = 6f;
    public float gravity = -20f;
    public float jumpForce = 8f;
    public float jumpCamInitial = -3f;
    public float jumpCamhold = 8f;
    public float jumpCamRecoil = -5f;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float camBobbingFreq = .1f;
    public float camBobbingAmp = 0.1f;
    public float wpnBobbingFreq = .1f;
    public float wpnBobbingAmp = .1f;
    public float wpnRotBobFreq = 1f;
    public float wpnRotBobAmp = 1f;
    public float runBobAmpMultiplier = 1.5f;
    public float runBobMultiplier = 1.5f;
    public float crouchBobDecreaser = 0.1f;
    [SerializeField] private float grenadeThrowForce = 400f;
    public Transform playerCamPos;
    public Transform jumpCam;
    public Transform weaponPos;
    public GameObject[] grenadeObj;
    private bool running = false;
    private bool jumpFalling = false;
    private bool isGrenadeThrowed = false;
    float currentHeight;
    float currentSpeed;
    float cameraBob;
    float weaponBob;
    float weaponRotBob;
    int grenadePollCounter;
    // float currentCamBobAmp;
    // float currentCamBobFreq;
    // float currentWpnBobAmp;
    // float currentWpnBobFreq;
    float jumpXCam;

    CharacterController controller;
    Vector3 velocity;
    Vector3 CamStartRot;

    public Transform cameraPivot;
    public float mouseSensitivity = 2.5f;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentHeight = controller.height;
        currentSpeed = maxSpeed;
        CamStartRot = playerCamPos.localEulerAngles;
        // currentCamBobAmp = camBobbingAmp;
        // currentCamBobFreq = camBobbingFreq;
        // currentWpnBobAmp = wpnBobbingAmp;
        // currentCamBobFreq = wpnBobbingFreq;
        jumpXCam = 0f;
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

        running = Keyboard.current.leftShiftKey.isPressed;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) x -= 1;
            if (Keyboard.current.dKey.isPressed) x += 1;
            if (Keyboard.current.sKey.isPressed) z -= 1;
            if (Keyboard.current.wKey.isPressed) z += 1;
        }
        if (Keyboard.current == null)
        {
            running = false;
        }

        if(Keyboard.current.gKey.isPressed && !isGrenadeThrowed)
        {
            if(grenadePollCounter == 5)
            {
                grenadePollCounter = 0;
            }
            StartCoroutine(ThrowGrenade());
        }

        bool crouching = Keyboard.current.ctrlKey.isPressed;        
        float targetHeight = crouching ? crouchHeight : standHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, dt * 10f);
        controller.height = currentHeight;
        maxSpeed = crouching ? crouchSpeed : currentSpeed;

        if(running == true)
        {
            crouching = false;
            maxSpeed = runSpeed;
        }
        else if(crouching == false)
        {
            maxSpeed = currentSpeed;
        }

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
                jumpXCam = jumpCamInitial;
                velocity.y = jumpForce;
                crouching = false;
                StartCoroutine(JumpCamAnimation());
            }
        }
        else
        {
            Accelerate(wishDir, wishSpeed, airAccel, dt);
            jumpFalling = true;
        }

        velocity.y += gravity * dt;

        controller.Move(velocity * dt);

        bool isMoving = (x != 0 || z != 0) && grounded;

        float pitch = 0f, roll = 0f;
        float vert = 0f, hori = 0f;
        float wpnPitch = 0f, wpnYaw = 0f;

        if (isMoving)
        {
            float freqMul = 1f;
            float ampMul = 1f;
        
            if (running)
            {
                freqMul = runBobMultiplier;
                ampMul = runBobAmpMultiplier;
            }
            else if (crouching)
            {
                freqMul = 0.7f; // optional slower crouch rhythm
                ampMul = crouchBobDecreaser;
            }
        
            cameraBob += camBobbingFreq * freqMul * dt;
            weaponBob += wpnBobbingFreq * freqMul * dt;
            weaponRotBob += wpnRotBobFreq * freqMul * dt;
        
            pitch = Mathf.Sin(cameraBob) * camBobbingAmp * 2f * ampMul;
            roll  = Mathf.Cos(cameraBob * 0.5f) * camBobbingAmp * 4f * ampMul;

            vert = Mathf.Sin(weaponBob) * wpnBobbingAmp * 2f * ampMul;
            hori = Mathf.Cos(weaponBob * .5f) * wpnBobbingAmp * 4f * ampMul;

            //wpnPitch = Mathf.Sin(weaponRotBob * .1f) * wpnRotBobAmp * 1f * ampMul;
            wpnYaw = Mathf.Cos(-weaponRotBob + 2f) * wpnRotBobAmp * 2f * ampMul;
        }
        else
        {
            cameraBob = 0f;
            pitch = 0f;
            roll = 0f;
            vert = 0f;
            hori = 0f;
            wpnPitch = 0f;
            wpnYaw = 0f;
        }
        // Rotation offsets

        if(grounded && jumpFalling)
        {
            jumpFalling = false;
            StartCoroutine(JumpCamAnimRecoil());
        }


        Quaternion targetRot = Quaternion.Euler(pitch, 0f, roll);
        Vector3 targetPos = new Vector3(hori, vert, 0);
        Quaternion targetWpnRot = Quaternion.Euler(wpnPitch, wpnYaw, 0f);

        playerCamPos.localRotation = Quaternion.Lerp(
            playerCamPos.localRotation,
            targetRot,
            dt * 10f
        );
        weaponPos.localPosition = Vector3.Lerp(
            weaponPos.localPosition,
            targetPos,
            dt * 10f
        );
        weaponPos.localRotation = Quaternion.Lerp(
            weaponPos.localRotation,
            targetWpnRot,
            dt * 10f
        );

        Quaternion jumpRotCam = Quaternion.Euler(jumpXCam, 0, 0);
        jumpCam.localRotation = Quaternion.Lerp(jumpCam.localRotation, jumpRotCam, dt * 5f);



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

    IEnumerator JumpCamAnimation()
    {
        
        yield return new WaitForSeconds (.2f);
        jumpFalling = true;
        jumpXCam = jumpCamhold;
    }
    IEnumerator JumpCamAnimRecoil()
    {
        jumpXCam = jumpCamRecoil;
        yield return new WaitForSeconds (.1f);
        jumpXCam = 0f; 
    }
    IEnumerator ThrowGrenade()
    {
        grenadeObj[grenadePollCounter].transform.parent = null;
        grenadeObj[grenadePollCounter].SetActive(true);
        Transform grenadePos = grenadeObj[grenadePollCounter].GetComponent<Transform>();
        Rigidbody grenadeRb = grenadeObj[grenadePollCounter].GetComponent<Rigidbody>();
        Collider grenadeCol = grenadeObj[grenadePollCounter].GetComponent<Collider>();
        grenadeCol.isTrigger = true;
        grenadeRb.AddForce(cameraPivot.forward * grenadeThrowForce);
        isGrenadeThrowed = true;
        yield return new WaitForSeconds (.2f);
        grenadeCol.isTrigger = false;
        yield return new WaitForSeconds (1f);
        grenadePollCounter++;
        isGrenadeThrowed = false;
    }
}
