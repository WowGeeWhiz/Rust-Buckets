using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Fusion_Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float speed = 500f;
    [SerializeField] private float jumpImpulse = 500f;
    [SerializeField] private bool invertVertical = true;
    [SerializeField] private float deadzone = 0.7f;

    [SerializeField] private Canvas SettingsMenu;//-----------
    [SerializeField] private TMP_InputField InputField;//-----------
    [SerializeField] private EventSystem eventSystem;//-----------

    private float menuToggleCooldown = 0.5f; // 0.5 seconds cooldown
    private float lastMenuToggleTime = 0f;
    private bool isMenuActive;

    private PlayerDataComponent playerDataComponent;

    [Networked] private NetworkButtons previousButtons { get; set; }

    public override void Spawned()
    {
         if (HasInputAuthority && eventSystem == null)
        {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }

        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority)
        {
            previousButtons = new NetworkButtons();
            playerDataComponent = GetComponent<PlayerDataComponent>(); // Get the PlayerDataComponent
            InitializeSettingsMenu();
            Fusion_Camera_Follow.Singleton.SetTarget(camTarget);
        }
    }

    private void InitializeSettingsMenu()
    {
        if (SettingsMenu != null)
        {
            SettingsMenu.gameObject.SetActive(false);
            InputField.onValueChanged.AddListener(OnValueChanged);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out Fusion_NetInput input))
        {
            HandleMovement(input);
            HandleMenuToggle(input);
        }
    }

    private void HandleMovement(Fusion_NetInput input)
    {
        if (isMenuActive) return;

        int inversion = invertVertical ? -1 : 1;

        if (Mathf.Abs(input.LookDelta.x) < deadzone) input.LookDelta.x = 0f;
        else input.LookDelta.x *= inversion;

        if (Mathf.Abs(input.LookDelta.y) < deadzone) input.LookDelta.y = 0f;

        kcc.AddLookRotation(input.LookDelta);
        UpdateCamTarget();

        Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
        float jump = input.Buttons.WasPressed(previousButtons, InputButton.Jump) && kcc.IsGrounded ? jumpImpulse : 0f;

        kcc.Move(worldDirection.normalized * speed, jump);
        //previousButtons = input.Buttons;
    }

    //private void HandleMenuToggle(Fusion_NetInput input)
    //{
    //    if (input.Buttons.WasPressed(previousButtons, InputButton.Options))
    //    {
    //        ToggleMenu();
    //    }

    //    //previousButtons = input.Buttons;
    //}

    private void HandleMenuToggle(Fusion_NetInput input)
    {
        if (HasInputAuthority && input.Buttons.WasPressed(previousButtons, InputButton.Options))
        {
            // Check if enough time has passed since the last toggle
            if (Time.time >= lastMenuToggleTime + menuToggleCooldown)
            {
                ToggleMenu();
                lastMenuToggleTime = Time.time; // Update the last toggle time
            }
        }
    }



    private void OnValueChanged(string value)
    {
        if (InputField != null)
        {
            InputField.text = value.ToUpper();
            InputField.caretPosition = InputField.text.Length;
        }

        if (HasInputAuthority && playerDataComponent != null)
        {
            playerDataComponent.PlayerData.Name = InputField.text; // Set name in PlayerData
        }
    }

    //private void ToggleMenu()
    //{
    //    bool isActive = SettingsMenu.gameObject.activeSelf;
    //    SettingsMenu.gameObject.SetActive(!isActive);

    //    Debug.LogWarning(isActive ? "Closing Menu" : "Opening Menu");

    //    if (HasInputAuthority && !isActive)
    //    {
    //        EventSystem.current.SetSelectedGameObject(InputField.gameObject);
    //        InputField.ActivateInputField();
    //        Debug.Log("InputField activated.");
    //    }
    //    else
    //    {
    //        EventSystem.current.SetSelectedGameObject(null);
    //        Debug.Log("InputField deactivated.");
    //    }
    //}

    private void ToggleMenu()
    {
        if (!HasInputAuthority) return; // Only allow the local player to toggle

        isMenuActive = !isMenuActive; // Toggle the state
        SettingsMenu.gameObject.SetActive(isMenuActive);

        Debug.LogWarning(isMenuActive ? "Opening Menu" : "Closing Menu");

        if (isMenuActive)
        {
            EventSystem.current.SetSelectedGameObject(InputField.gameObject);
            InputField.ActivateInputField();
            Debug.Log("InputField activated.");
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            InputField.DeactivateInputField(); // Deactivate the input field
            Debug.Log("InputField deactivated.");
        }
    }



    public override void Render()
    {
        UpdateCamTarget();
    }

    private void UpdateCamTarget() 
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }
}
