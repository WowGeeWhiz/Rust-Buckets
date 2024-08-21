using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 8_20_2024
/// 
/// Last Updated: 8_21_2024
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
/// I prefer Inverted look on my controller, just add this to the settings so it can be toggled.
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
/// When you use Unity.Netcode;, you must change MonoBehaviour to NetworkBehaviour.
/// 
/// IsOwner is for the specific client.
/// 
/// ClientRpc is for all clients.
/// 
/// ServerRpc is for the host.
/// 
/// RPC means Remote Procedure Calls.
/// 
/// You must add a Network Object script (Network Manager) to the object that'll use any script using NetworkBehaviour.
/// 
/// You will not be able to play unless you declare yourself a host or as a client connect to a host.
/// 
/// You will need to add the scene to the Network Manager Script.
/// 
/// --------------------------------------------------------------------------------------------------------
/// </summary>

public class Controller_Movement : NetworkBehaviour
{

    // Attributes:=--------------------------------------------------------------------------------------------------------------

    // Player:
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

    private Rigidbody playerRB;
    private bool onGround;
    private float currentSpeed;
    private bool canJump;
    private float initialWalkSpeed;

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

            Contoller_Button_Sprint();
            Controller_Button_Jump();
            Controller_RightThumbstick_Rotation();
            Controller_LeftThumbstick_Movement();

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
    }

    // Movement Methods:---------------------------------------------------------------------------------------------------

    private void Contoller_Button_Sprint()
    {

        if (IsOwner && Gamepad.all[0].leftStickButton.isPressed)
        {
            walkSpeed = sprintSpeed;
        }
        else
        {
            walkSpeed = initialWalkSpeed;
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

            // Adjust sensitivity
            horizontalRight *= sensitivityHorizontal;
            verticalRight *= sensitivityVertical;

            // Check if the magnitude of right thumbstick input is greater than a threshold
            if (rightStick.magnitude > 0.1f) // Adjust the threshold value as needed
            {
                // Check if the horizontal magnitude is greater than the vertical magnitude
                if (Mathf.Abs(horizontalRight) > Mathf.Abs(verticalRight))
                {
                    // Rotate the player around its own up axis based on horizontal right thumbstick input
                    playerTransform.Rotate(Vector3.up, horizontalRight * rotationSpeed * Time.deltaTime);
                }
                else
                {
                    // Rotate the camera around the player based on vertical right thumbstick input
                    cameraTransform.RotateAround(playerTransform.position, cameraTransform.right, (inversion * verticalRight) * rotationSpeed * Time.deltaTime);
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

    private void Physics_RemainUpright()
    {
        Vector3 currentEulerAngles = transform.rotation.eulerAngles; // Keep upright
        currentEulerAngles.x = 0;
        currentEulerAngles.z = 0;

        transform.rotation = Quaternion.Euler(currentEulerAngles); // Apply corrected rotation
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

    private void UpdateNetworkPosition() 
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
