using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine;

public class Fusion_Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float speed = 500f;
    [SerializeField] private float jumpImpulse = 500f;
    [SerializeField] private bool invertVertical = true;
    [SerializeField] private float deadzone = 0.7f;

    [SerializeField] private TMPro.TextMeshPro NameTag;//-----------
    [SerializeField] private Canvas SettingsMenu;//-----------
    [SerializeField] private TMP_InputField InputField;//-----------

    [Networked] public string Name { get; private set; }

    [Networked] private NetworkButtons previousButtons { get; set; }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority) 
        {

            Fusion_Camera_Follow.Singleton.SetTarget(camTarget);

            NameTag = GetComponentInChildren<TMPro.TextMeshPro>();//-----------

            SettingsMenu = GetComponentInChildren<Canvas>();

            if (HasInputAuthority && NameTag != null)//-----------
            {
                Name = "Player Name";
                NameTag.text = Name;
            }

            if (HasInputAuthority && SettingsMenu != null)//-----------
            {
                SettingsMenu.gameObject.SetActive(false);
                InputField = GetComponentInChildren<TMP_InputField>();
                InputField.onValueChanged.AddListener(OnValueChanged);
            }
        }
    }

    public override void FixedUpdateNetwork() 
    {
        if (GetInput(out Fusion_NetInput input)) 
        {

            //Look inversion:
            int inversion = invertVertical ? -1 : 1;

            //Deadzone:
            if (Mathf.Abs(input.LookDelta.x) < deadzone)
            {
                input.LookDelta.x = 0f; // Ignore input if within deadzone

            }
            else
            {
                input.LookDelta.x *= inversion;
            }

            if (Mathf.Abs(input.LookDelta.y) < deadzone)
            {
                input.LookDelta.y = 0f; // Ignore input if within deadzone

            }

            kcc.AddLookRotation(input.LookDelta);
            UpdateCamTarget();

            Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
            float jump = 0f;

            if (input.Buttons.WasPressed(previousButtons, InputButton.Jump) && kcc.IsGrounded) 
            {
                Debug.LogWarning("Jump");
                jump = jumpImpulse;
            }

            //-----------------------------------------------Settings Menu:

            if (input.Buttons.WasPressed(previousButtons, InputButton.Options))//-----------
            {
                Debug.LogWarning("Open/Close Menu");
                ToggleMenu();
            }
            //-----------------

            kcc.Move(worldDirection.normalized * speed, jump);
            previousButtons = input.Buttons;

        }

        if (HasInputAuthority) 
        {
            UpdatePlayerName();
        }

    }

    private void UpdatePlayerName() 
    {
        NameTag.text = Name;
    }

    private void OnValueChanged(string value)
    {
        // Convert input to uppercase
        if (InputField != null)
        {
            InputField.text = value.ToUpper();
            // Set the caret position to the end of the text
            InputField.caretPosition = InputField.text.Length;
        }

        Name = InputField.text;
    }

    private void ToggleMenu()//-----------
    {
        // Toggle the active state of the settings menu
        bool isActive = SettingsMenu.gameObject.activeSelf;
        SettingsMenu.gameObject.SetActive(!isActive); // Toggle between active and inactive
        Debug.LogWarning(isActive ? "Closing Menu" : "Opening Menu");
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
