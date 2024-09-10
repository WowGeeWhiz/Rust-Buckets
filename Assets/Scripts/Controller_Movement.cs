using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 8_20_2024
/// 
/// Last Updated: 9_5_2024
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
/// --------------------------------------------------------------------------------------------------------
/// 
/// Patch 9_5_2024:
/// 
///  Description:
///  
/// -Added aim assist.
/// -Added triangulated gun to camera sync.
/// -Added camera positioning system.
///  
///  Package:
///  
///  N/A
/// 
///  Note:
/// 
///  N/A
/// 
/// --------------------------------------------------------------------------------------------------------
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
    public GameObject Aimed_Container;
    public float verticalOffset;

    [Header("Player-Camera Offset Variables")]
    public float camX;
    public float camY;
    public float camZ;

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
                // Controls:
                Contoller_Button_Sprint();
                Controller_Button_Jump();
                Controller_RightThumbstick_Rotation();
                Controller_LeftThumbstick_Movement();
                Player.GetComponent<Controller_Action>().Controller_Trigger_Attack();

                // Settings:
                Aim_With_Camera_Sync();
                UpdateCameraPosition();

            }

            // Host Network Update:
            UpdateNetworkPosition();
        }

        // Client Network Update:
        UpdateLocalPosition();
    }

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
                // Hold down mode:
                if (sprintButtonPressed)
                {
                    walkSpeed = sprintSpeed;
                }
                else
                {
                    walkSpeed = initialWalkSpeed;
                }
            }
        }
    }

    private void Controller_Button_Jump()
    {
        if (IsOwner && Gamepad.all[0].aButton.isPressed && canJump)
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
                // Rotate the player around its own up axis based on horizontal right thumbstick input:
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
    }


    public void Controller_LeftThumbstick_Movement()
    {
        if (IsOwner)
        {
            Transform playerTransform = Player.transform;
            Transform cameraTransform = Camera.transform;
            Gamepad gamepad = Gamepad.all[0];

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

            playerTransform.Translate(moveDirection.normalized * currentSpeed * deltaTime, Space.World);
        }
    }

    // Aim/Camera:-----------------------------------------------------------------------------------------------------------------

    //void Aim_With_Camera_Sync()
    //{
    //    Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
    //    RaycastHit hit;

    //    if (Physics.Raycast(ray, out hit))
    //    {
    //        if (!hit.collider.CompareTag("Player"))
    //        {
    //            Vector3 directionToHit = hit.point - Aimed_Container.transform.position;
    //            Aimed_Container.transform.rotation = Quaternion.LookRotation(directionToHit);
    //        }
    //    }
    //}

    void Aim_With_Camera_Sync()
    {

        Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                // Calculate the direction to the hit point
                Vector3 directionToHit = hit.point - Aimed_Container.transform.position;

                // Apply the vertical offset
                // Calculate offset direction in the vertical plane
                Vector3 offsetDirection = Quaternion.Euler(verticalOffset, 0, 0) * directionToHit;

                // Set the rotation of Aimed_Container
                Aimed_Container.transform.rotation = Quaternion.LookRotation(offsetDirection);
            }
        }
    }

    public void UpdateCameraPosition()
    {
        if (Camera != null && Player != null)
        {
            Vector3 offset = new Vector3(camX, camY, camZ);
            Quaternion playerRotation = Player.transform.rotation;
            Vector3 rotatedOffset = playerRotation * offset;
            Vector3 desiredCameraPosition = Player.transform.position + rotatedOffset;

            Camera.transform.position = desiredCameraPosition;
        }
    }

    // Physics:-------------------------------------------------------------------------------------------------------------

    public void ApplyFrictionToRotation()
    {
        Vector3 angularVelocity = playerRB.angularVelocity;
        angularVelocity.x *= 0.9f;
        angularVelocity.z *= 0.9f;
        angularVelocity.y *= 0.9f;
        playerRB.angularVelocity = angularVelocity;
    }

    public void ApplyFrictionToMovement()
    {
        Gamepad gamepad = Gamepad.all[0];
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
        Vector3 currentEulerAngles = transform.rotation.eulerAngles;
        currentEulerAngles.x = 0;
        currentEulerAngles.z = 0;
        transform.rotation = Quaternion.Euler(currentEulerAngles);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = true;
            canJump = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

    // Networking:-----------------------------------------------------------------------------------------------------------

    private void OwnerClientID()
    {
        Debug.Log($"Player {NetworkObject.OwnerClientId} is you Client ID");
    }

    public void UpdateNetworkPosition()
    {
        position.Value = Player.transform.position;
    }

    private void UpdateLocalPosition()
    {
        Player.transform.position = position.Value;
    }

    private void OnNetworkPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        if (!IsOwner)
        {
            Player.transform.position = newValue;
        }
    }
}
