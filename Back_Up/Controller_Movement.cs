using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// Author:Michael Knighen
/// 
/// Date Started: 8_20_2024
/// 
/// Date Updated: Null
/// 
/// Description:
/// 
/// This control scheme is a movement system based on my interpretation 
/// of how Halo Combat Evolves movement scheme functions along with the incorporation
/// of a dead zone. This was initially designed for a PS-4 Controller.
/// 
/// This was originally designed for my personal game and was adapted for this project.
/// 
/// Note:
/// I prefer Inverted look on my controller, just add this to the settings so it can be toggled.
/// 
/// </summary>

public class Controller_Movement : MonoBehaviour
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

    // Life_Cycle Methods:-----------------------------------------------------------------------------------------------------

    private void Start()
    {
        // Player:
        playerRB = Player.GetComponent<Rigidbody>();
        playerRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        onGround = false;
        canJump = true;
        initialWalkSpeed = walkSpeed;
    }

    private void Update()
    {
        Physics_RemainUpright();

        Contoller_Button_Sprint();
        Controller_Button_Jump();
        Controller_RightThumbstick_Rotation();
        Controller_LeftThumbstick_Movement();
    }

    // Custom Gravity:
    private void FixedUpdate()
    {
        if (!onGround)
        {
            playerRB.AddForce(Vector3.down * gravityMagnitude * playerRB.mass);
        }
    }

    // Debugger Methods (DO NOT LEAVE IN LIFE_CYCLE, USE AS NEEDED):-------------------------------------------------------
    private void Debug_ControllerDetection()
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Debug.Log(Gamepad.all[i].name);
        }
    }

    private void Debug_Speedometer()
    {
        Debug.Log(currentSpeed);
    }

    // Movement Methods:---------------------------------------------------------------------------------------------------

    private void Contoller_Button_Sprint()
    {

        if (Gamepad.all[0].leftStickButton.isPressed)
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
        if (Gamepad.all[0].aButton.isPressed && canJump) // Cross (X) button by default
        {
            playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
        }
    }

    public void Controller_RightThumbstick_Rotation()
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


    public void Controller_LeftThumbstick_Movement()
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
}
