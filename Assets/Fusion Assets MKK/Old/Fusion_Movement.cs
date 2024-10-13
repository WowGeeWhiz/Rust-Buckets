using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fusion_Movement : NetworkBehaviour
{

    private GameObject Player;
    private Camera Player_Camera;
    private Rigidbody playerRB;
    public GameObject Aimed_Container;
    public NetworkObject NetworkObject;


    private float walkSpeed = 20;
    private readonly float jumpForce = 17;
    private readonly float gravityMagnitude = 50;
    private readonly float rotationSpeed = 150;
    private readonly float sensitivityMovement = .7f;
    private float sensitivityVertical = .5f;
    private readonly float sensitivityHorizontal = .7f;
    private readonly float sprintSpeed = 35;
    private readonly float maxLookUpAngle = 25;
    private readonly float maxLookDownAngle = 20;
    private readonly float verticalOffset = -3.4f;
    private readonly float camX = 1.5f;
    private readonly float camY = 1.3f;
    private readonly float camZ = -2.3f;
    private float currentVerticalRotation = 0f;
    private float currentSpeed;
    private float initialWalkSpeed;

    private bool controlsLocked = false;
    private bool canJump = false;
    public bool invertVertical = false;
    private bool onGround = false;
    private bool isSprinting = false;
    public bool isSprintToggleMode = false;

    public override void Spawned()
    {
        NetworkObject = GetComponent<NetworkObject>();

        Debug.LogWarning($"----------------------------NetworkObject Authority: {GetComponent<NetworkObject>().HasInputAuthority}");

        Player = gameObject;
        playerRB = Player.GetComponent<Rigidbody>();
        Player_Camera = GetComponentInChildren<Camera>();
        playerRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        initialWalkSpeed = walkSpeed;

        Debug.LogWarning("----------------------------Player spawned and initialized.");
    }

    public override void Render()
    {
        if (Player_Camera != null)
        {
            Player_Camera.gameObject.SetActive(true);

        }

        Camera.main?.gameObject.SetActive(false);

        Physics_RemainUpright();

        if (!controlsLocked)
        {
            if (NetworkObject.HasInputAuthority)
            {
                //Contoller_Button_Sprint();
                Controller_Button_Jump();
                Controller_RightThumbstick_Rotation();
                //Controller_LeftThumbstick_Movement();
                //Player.GetComponent<Controller_Action>().Controller_Trigger_Attack();
                HandleMovement();
            }

            //Aim_With_Camera_Sync();
            UpdateCameraPosition();

        }

    }

    public void FixedUpdatetoNetwork()
    {
        if (!onGround)
        {
            if (!NetworkObject.HasInputAuthority) return;
            playerRB.AddForce(gravityMagnitude * playerRB.mass * Vector3.down);
        }

        ApplyFrictionToRotation();
        ApplyFrictionToMovement();
    }

    private void HandleMovement()
    {
        Vector2 input = Gamepad.current.leftStick.ReadValue(); // Get left stick input

        // Calculate movement direction relative to the camera
        Vector3 forward = Player_Camera.transform.forward;
        Vector3 right = Player_Camera.transform.right;
        forward.y = 0; // Keep movement horizontal
        right.y = 0; // Keep movement horizontal

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * input.y + right * input.x; // Determine movement direction

        if (moveDirection != Vector3.zero)
        {
            float speed = Gamepad.current.leftStickButton.isPressed ? sprintSpeed : walkSpeed; // Sprint if left stick button is pressed
            Vector3 newVelocity = moveDirection * speed;
            newVelocity.y = playerRB.velocity.y; // Preserve vertical velocity
            playerRB.velocity = newVelocity;
        }
        else
        {
            // Apply friction
            Vector3 friction = playerRB.velocity;
            friction.x *= 0.9f;
            friction.z *= 0.9f;
            playerRB.velocity = new Vector3(friction.x, playerRB.velocity.y, friction.z);
        }
    }

    private void Contoller_Button_Sprint()
    {
        if (!NetworkObject.HasInputAuthority) return;

        if (onGround)
        {
            Gamepad gamepad = Gamepad.current;
            bool sprintButtonPressed = gamepad.leftStickButton.isPressed;

            if (isSprintToggleMode)
            {

                if (!NetworkObject.HasInputAuthority) return;

                if (sprintButtonPressed && !isSprinting)
                {
                    walkSpeed = sprintSpeed;
                    isSprinting = true;
                }
                else if (sprintButtonPressed && isSprinting)
                {
                    walkSpeed = initialWalkSpeed;
                    isSprinting = false;
                }
            }
            else
            {
                if (!NetworkObject.HasInputAuthority) return;

                walkSpeed = sprintButtonPressed ? sprintSpeed : initialWalkSpeed;
            }
        }
    }

    private void Controller_Button_Jump()
    {
        if (NetworkObject.HasInputAuthority && Gamepad.current.aButton.isPressed && canJump)
        {
            playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
        }
    }

    public void Controller_RightThumbstick_Rotation()
    {
        if (!NetworkObject.HasInputAuthority) return;

        Transform playerTransform = Player.transform;
        Transform cameraTransform = Player_Camera.transform;
        Gamepad gamepad = Gamepad.current;

        int inversion = invertVertical ? -1 : 1;

        Vector2 rightStick = gamepad.rightStick.ReadValue();
        float horizontalRight = rightStick.x;
        float verticalRight = rightStick.y;

        float deadzone = 0.1f;

        if (rightStick.magnitude < deadzone)
        {
            horizontalRight = 0;
            verticalRight = 0;
        }
        else
        {
            Vector2 normalizedInput = rightStick.normalized;
            horizontalRight = normalizedInput.x * sensitivityHorizontal;
            verticalRight = normalizedInput.y * sensitivityVertical;
        }

        if (horizontalRight != 0 || verticalRight != 0)
        {
            if (Mathf.Abs(horizontalRight) > Mathf.Abs(verticalRight))
            {
                playerTransform.Rotate(Vector3.up, horizontalRight * rotationSpeed * Time.deltaTime);
            }
            else
            {
                float newVerticalRotation = currentVerticalRotation + (inversion * verticalRight * rotationSpeed * Time.deltaTime);
                newVerticalRotation = Mathf.Clamp(newVerticalRotation, -maxLookDownAngle, maxLookUpAngle);
                float rotationDifference = newVerticalRotation - currentVerticalRotation;
                cameraTransform.RotateAround(playerTransform.position, cameraTransform.right, rotationDifference);
                currentVerticalRotation = newVerticalRotation;
            }
        }

    }

    public void Controller_LeftThumbstick_Movement()
    {
        if (!NetworkObject.HasInputAuthority) return;

        Transform playerTransform = Player.transform;
        Transform cameraTransform = Player_Camera.transform;
        Gamepad gamepad = Gamepad.current;

        Vector2 leftStick = gamepad.leftStick.ReadValue();
        float horizontalLeft = leftStick.x;
        float verticalLeft = leftStick.y;

        horizontalLeft *= sensitivityMovement;
        verticalLeft *= sensitivityMovement;

        playerTransform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);

        float deltaTime = Time.deltaTime;
        float inputMagnitude = new Vector2(horizontalLeft, verticalLeft).magnitude;
        currentSpeed = inputMagnitude * walkSpeed;

        Vector3 moveDirection = horizontalLeft * cameraTransform.right + verticalLeft * cameraTransform.forward;
        moveDirection.y = 0;

        if (NetworkObject.HasStateAuthority)
        {
            playerRB.MovePosition(playerTransform.position + moveDirection * currentSpeed * deltaTime);
        }

    }


    void Aim_With_Camera_Sync()
    {

        if (!NetworkObject.HasInputAuthority) return;

        Ray ray = new Ray(Player_Camera.transform.position, Player_Camera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                Vector3 directionToHit = hit.point - Aimed_Container.transform.position;
                Vector3 offsetDirection = Quaternion.Euler(verticalOffset, 0, 0) * directionToHit;
                Aimed_Container.transform.rotation = Quaternion.LookRotation(offsetDirection);
            }
        }
    }

    public void UpdateCameraPosition()
    {

        if (!NetworkObject.HasInputAuthority) return;

        if (Player_Camera != null && Player != null)
        {
            Vector3 offset = new Vector3(camX, camY, camZ);
            Quaternion playerRotation = Player.transform.rotation;
            Vector3 rotatedOffset = playerRotation * offset;
            Vector3 desiredCameraPosition = Player.transform.position + rotatedOffset;

            Player_Camera.transform.position = desiredCameraPosition;
        }
    }

    public void ApplyFrictionToRotation()
    {

        if (!NetworkObject.HasInputAuthority) return;

        Vector3 angularVelocity = playerRB.angularVelocity;
        angularVelocity.x *= 0.9f;
        angularVelocity.z *= 0.9f;
        angularVelocity.y *= 0.9f;
        playerRB.angularVelocity = angularVelocity;
    }

    public void ApplyFrictionToMovement()
    {

        if (!NetworkObject.HasInputAuthority) return;

        Gamepad gamepad = Gamepad.current;
        Vector2 leftStickInput = gamepad.leftStick.ReadValue();

        if (leftStickInput.magnitude < 0.1f)
        {
            Vector3 velocity = playerRB.velocity;
            velocity.x *= 0.9f;
            velocity.z *= 0.9f;
            playerRB.velocity = velocity;
        }
    }

    private void Physics_RemainUpright()
    {

        if (!NetworkObject.HasInputAuthority) return;

        Vector3 currentEulerAngles = transform.rotation.eulerAngles;
        currentEulerAngles.x = 0;
        currentEulerAngles.z = 0;
        transform.rotation = Quaternion.Euler(currentEulerAngles);
    }

    public bool GetSprintToggleMode()
    {
        return isSprintToggleMode;
    }

    public void SetSprintToggleMode(bool value)
    {
        if (!NetworkObject.HasInputAuthority) return;

        isSprintToggleMode = value;
    }

    public bool GetInvertVertical()
    {
        return invertVertical;
    }

    public void SetInvertVertical(bool value)
    {
        if (!NetworkObject.HasInputAuthority) return;

        invertVertical = value;
    }

    public void LockControls(bool cooldown)
    {
        if (!NetworkObject.HasInputAuthority) return;

        controlsLocked = cooldown;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (!NetworkObject.HasInputAuthority) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = true;
            canJump = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {

        if (!NetworkObject.HasInputAuthority) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

}
