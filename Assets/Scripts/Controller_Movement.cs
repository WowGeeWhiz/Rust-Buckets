using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 8_20_2024
/// 
/// Last Updated: 8_31_2024
/// 
///  <<<DON'T TOUCH MY CODE>>>
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 8_20_2024:
/// 
/// Description:
/// 
/// This control scheme is a movement system based on my interpretation 
/// of how Halo Combat Evolves movement scheme functions along with the incorporation
/// of a dead zone. This was initially designed for a PS-4 Controller.
/// 
/// This was originally designed for my personal game and was adapted for this project.
/// 
/// Package:
/// 
/// Input System
/// 
/// Note:
/// 
/// -I prefer Inverted look on my controller, just add this to the settings so it can be toggled.
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Patch 8_21_2024:
/// 
/// Description:
/// 
/// Added Networking elements to the code.
/// 
/// Package:
/// 
/// Unity Netcode for GameObjects
/// 
/// Note:
/// 
/// -When you use Unity.Netcode;, you must change MonoBehaviour to NetworkBehaviour.
/// 
/// -IsOwner is for the specific client.
/// 
/// -ClientRpc is for all clients.
/// 
/// -ServerRpc is for the host.
/// 
/// -RPC means Remote Procedure Calls.
/// 
/// -You must add a Network Object script (Network Manager) to the object that'll use any script using NetworkBehaviour.
/// 
/// -You will not be able to play unless you declare yourself a host or as a client connect to a host.
/// 
/// -You will need to add the scene to the Network Manager Script.
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Patch 8_31_2024:
/// 
/// Description:
/// 
/// Added friction, control locking for respawn, and polish to the features based on feedback.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// -Fixed the spinning issue.
/// 
/// -Fixed the sliding issue.
/// 
/// -Added Vertical look limitations.
/// 
/// -Proper deadzone added.
/// 
/// -Adjustment to values based on feedback added.
/// 
/// -Prevented player being able to sprint in the air.
/// 
/// -Added Togglable Sprint option.
/// 
/// </summary>

public class Controller_Movement : NetworkBehaviour
{

    // Attributes:=--------------------------------------------------------------------------------------------------------------

    [Header("Player Variables")]
    public GameObject Player;
    public GameObject Camera;
    public float walkSpeed;
    public float jumpForce;
    public float gravityMagnitude;
    public float rotationSpeed;
    public bool invertVertical; // Michael's preferred setting (Add to settings menu)
    public float sensitivityMovement;
    public float sensitivityVertical;
    public float sensitivityHorizontal;
    public float sprintSpeed;
    public float maxLookUpAngle;
    public float maxLookDownAngle;
    public bool isSprintToggleMode = false; // Jake's preference setting (Add to settings menu)

    private bool isSprinting = false;
    private float currentVerticalRotation = 0f;
    private Rigidbody playerRB;
    private bool onGround;
    private float currentSpeed;
    private bool canJump;
    private float initialWalkSpeed;
    private bool controlsLocked;


    // Networking:
    private readonly NetworkVariable<Vector3> position = new();

    /// <summary>
    /// 
    /// Network Variable handles the automatic synchronization of position data in this case and requires no RPC's.
    /// 
    /// </summary>

    // Life_Cycle Methods:-----------------------------------------------------------------------------------------------------

    private void Start()
    {
        // Player:
        playerRB = Player.GetComponent<Rigidbody>();
        playerRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        onGround = false;
        canJump = true;
        initialWalkSpeed = walkSpeed;

        // Lock:
        controlsLocked = false;

        // Networking:
        if (IsOwner) 
        {
            OwnerClientID();
            position.Value = Player.transform.position;
        }

        position.OnValueChanged += OnNetworkPositionChanged;
    }

    private void Update()
    {
        if (IsOwner)
        {

            Physics_RemainUpright();

            if (!controlsLocked)
            {

                Contoller_Button_Sprint();
                Controller_Button_Jump();
                Controller_RightThumbstick_Rotation();
                Controller_LeftThumbstick_Movement();

            }
            else 
            {
                // Controls are locked.
            }

            // Host Network Update:
            UpdateNetworkPosition();
        }

        //Client Network Update:
        UpdateLocalPosition();

    }

    // Custom Gravity:
    private void FixedUpdate()
    {
        if (IsOwner && !onGround)
        {
            playerRB.AddForce(Vector3.down * gravityMagnitude * playerRB.mass);
        }

        // Apply friction to reduce spin:
        ApplyFrictionToRotation();

        // Apply linear friction to prevent sliding:
        ApplyFrictionToMovement();
    }

    // Locked/Unlock Controls Method:--------------------------------------------------------------------------------------

    public void LockControls(bool cooldown) 
    {
        controlsLocked = cooldown;
    }

    // Movement Methods:---------------------------------------------------------------------------------------------------

    private void Contoller_Button_Sprint()
    {
        if (IsOwner && onGround)
        {
            Gamepad gamepad = Gamepad.all[0];
            bool sprintButtonPressed = gamepad.leftStickButton.isPressed;

            if (isSprintToggleMode)
            {
                // Toggle mode:
                if (sprintButtonPressed && !isSprinting)
                {
                    // Start sprinting:
                    walkSpeed = sprintSpeed;
                    isSprinting = true;
                }
                else if (sprintButtonPressed && isSprinting)
                {
                    // Stop sprinting:
                    walkSpeed = initialWalkSpeed;
                    isSprinting = false;
                }
            }
            else
            {
                // Hold down mode:
                if (sprintButtonPressed)
                {
                    // Start sprinting:
                    walkSpeed = sprintSpeed;
                }
                else
                {
                    // Stop sprinting:
                    walkSpeed = initialWalkSpeed;
                }
            }
        }
    }


private void Controller_Button_Jump()
    {
        if (IsOwner && Gamepad.all[0].aButton.isPressed && canJump) // Cross (X) button by default
        {
            playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
        }
    }

    public void Controller_RightThumbstick_Rotation()
    {
        if (IsOwner)
        {
            Transform playerTransform = Player.transform;
            Transform cameraTransform = Camera.transform;
            Gamepad gamepad = Gamepad.all[0];

            int inversion = invertVertical ? 1 : -1;

            // Right Thumbstick:
            Vector2 rightStick = gamepad.rightStick.ReadValue();
            float horizontalRight = rightStick.x;
            float verticalRight = rightStick.y;

            // Deadzone value:
            float deadzone = 0.1f;

            // Apply deadzone:
            if (rightStick.magnitude < deadzone)
            {
                horizontalRight = 0;
                verticalRight = 0;
            }
            else
            {
                // Normalize the input to ensure consistent speed in all directions
                Vector2 normalizedInput = rightStick.normalized;
                horizontalRight = normalizedInput.x * sensitivityHorizontal;
                verticalRight = normalizedInput.y * sensitivityVertical;
            }

            // Check if the magnitude of right thumbstick input is above the deadzone:
            if (horizontalRight != 0 || verticalRight != 0)
            {
                // Rotate the player around its own up axis based on horizontal right thumbstick input:
                if (Mathf.Abs(horizontalRight) > Mathf.Abs(verticalRight))
                {
                    playerTransform.Rotate(Vector3.up, horizontalRight * rotationSpeed * Time.deltaTime);
                }
                else
                {
                    // Update the vertical rotation angle and clamp it:
                    float newVerticalRotation = currentVerticalRotation + (inversion * verticalRight * rotationSpeed * Time.deltaTime);
                    newVerticalRotation = Mathf.Clamp(newVerticalRotation, -maxLookDownAngle, maxLookUpAngle);

                    // Calculate the rotation difference:
                    float rotationDifference = newVerticalRotation - currentVerticalRotation;

                    // Apply the clamped rotation difference:
                    cameraTransform.RotateAround(playerTransform.position, cameraTransform.right, rotationDifference);

                    // Update the current vertical rotation:
                    currentVerticalRotation = newVerticalRotation;
                }
            }
        }
    }

    public void Controller_LeftThumbstick_Movement()
    {
        if (IsOwner) 
        {
            Transform playerTransform = Player.transform;
            Transform cameraTransform = Camera.transform;
            Gamepad gamepad = Gamepad.all[0];

            // Left Thumbstick:
            Vector2 leftStick = gamepad.leftStick.ReadValue();
            float horizontalLeft = leftStick.x;
            float verticalLeft = leftStick.y;

            // Adjust sensitivity
            horizontalLeft *= sensitivityMovement;
            verticalLeft *= sensitivityMovement;

            // Rotate the player based on camera rotation
            playerTransform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);

            // Store Time.deltaTime in a variable to reduce redundant calls
            float deltaTime = Time.deltaTime;

            // Accelerate or decelerate based on thumbstick input magnitude
            float inputMagnitude = new Vector2(horizontalLeft, verticalLeft).magnitude;
            currentSpeed = inputMagnitude * walkSpeed;

            // Calculate move direction relative to camera's forward direction (ignore Y axis)
            Vector3 moveDirection = horizontalLeft * cameraTransform.right + verticalLeft * cameraTransform.forward;
            moveDirection.y = 0; // Ignore Y axis

            // Move the player based on input direction relative to camera's forward direction
            playerTransform.Translate(moveDirection.normalized * currentSpeed * deltaTime, Space.World);
        }
        
    }

    // Physics:-------------------------------------------------------------------------------------------------------------

    private void ApplyFrictionToRotation()
    {
        // Get current angular velocity:
        Vector3 angularVelocity = playerRB.angularVelocity;

        // Apply damping/friction to reduce unwanted spinning:
        angularVelocity.x *= 0.9f; // (Adjust the factor as needed (0.9 means 10% reduction))
        angularVelocity.z *= 0.9f; // (Dampen x and z to reduce horizontal spinning)
        angularVelocity.y *= 0.9f; // (Optional: Dampen y if needed)

        // Update the Rigidbody's angular velocity with damped values:
        playerRB.angularVelocity = angularVelocity;
    }

    private void ApplyFrictionToMovement()
    {
        // Check if there's no movement input from the player:
        Gamepad gamepad = Gamepad.all[0];
        Vector2 leftStickInput = gamepad.leftStick.ReadValue();

        // If no input on left thumbstick, apply friction:
        if (leftStickInput.magnitude < 0.1f) // (Adjust the threshold as needed)
        {
            // Get current velocity:
            Vector3 velocity = playerRB.velocity;

            // Apply damping/friction to horizontal velocity (X and Z):
            velocity.x *= 0.9f; // (Reduce sliding on the X-axis)
            velocity.z *= 0.9f; // (Reduce sliding on the Z-axis)

            // Update the Rigidbody's velocity with damped values:
            playerRB.velocity = velocity;
        }
    }

    private void Physics_RemainUpright()
    {
        // Keep upright:
        Vector3 currentEulerAngles = transform.rotation.eulerAngles;
        currentEulerAngles.x = 0;
        currentEulerAngles.z = 0;

        // Apply corrected rotation:
        transform.rotation = Quaternion.Euler(currentEulerAngles);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // On Ground:
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = true;
            canJump = true;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        // Off Ground:
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

    // Networking:-----------------------------------------------------------------------------------------------------------

    // Client ID:

    private void OwnerClientID() 
    {
        Debug.Log($"Player {NetworkObject.OwnerClientId} is you Client ID");
    }

    // Network Position Data:

    public void UpdateNetworkPosition() 
    {
        position.Value = Player.transform.position;
    }

    private void UpdateLocalPosition() 
    {
        Player.transform.position = position.Value;
    }

    // Network Synchronization of Data:

    private void OnNetworkPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        if (!IsOwner)
        {
            Player.transform.position = newValue;
        }
    }

}
