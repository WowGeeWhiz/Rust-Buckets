using Fusion;
using Fusion.Addons.SimpleKCC;
using System.IO;
using UnityEngine;

public class Fusion_Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] public float speed = 500f;
    [SerializeField] private float jumpImpulse = 500f;
    [SerializeField] private bool invertVertical = true;
    [SerializeField] private float deadzone = 0.7f;

    private TMPro.TextMeshPro NameTag;//-----------
    [SerializeField] private GameObject SettingsMenu;//-----------

    [Networked] public string Name { get; private set; }
    [Networked] private NetworkButtons previousButtons { get; set; }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority) 
        {

            LoadPlayerName();//-----
            RPC_PlayerName(Name); // Call the RPC to set the name for all clients----
            //Name = "MICHAEL";
            //name = playerprefs.getstring("username");

            RPC_PlayerName(Name);
            Fusion_Camera_Follow.Singleton.SetTarget(camTarget);

            NameTag = GetComponentInChildren<TMPro.TextMeshPro>();//-----------

            if (NameTag != null)//-----------
            {
                NameTag.text = Name;
            }
            else
            {
                Debug.LogWarning("NameTag (TextMeshPro) not found on the player object.");
            }

        }

        SettingsMenu = GameObject.Find("SettingsMenu");//-----------

        if (SettingsMenu != null)//--------
        {
            SettingsMenu.SetActive(false);
            Debug.LogWarning("SettingsMenu found!");
        }
        else
        {
            Debug.LogWarning("SettingsMenu not found!");
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
                ToggleSettingsMenu();
            }
            //-----------------

            kcc.Move(worldDirection.normalized * speed, jump);
            previousButtons = input.Buttons;

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

    private void LoadPlayerName()//-------
    {
        const string filePath = "playerName.txt"; // Define the same file path

        if (File.Exists(filePath))
        {
            Name = File.ReadAllText(filePath); // Read the name from the text file
        }
        else
        {
            Name = ""; // Default name if file does not exist
            Debug.LogWarning("File Missing-PlayerName");
        }
    }

    private void ToggleSettingsMenu()//-----------
    {
        if (SettingsMenu != null)
        {
            bool isActive = SettingsMenu.activeSelf;
            SettingsMenu.SetActive(!isActive); // Toggle the menu visibility

            if (!SettingsMenu.activeSelf) // When closing the menu
            {
                // Assuming your input field is named 'inputField'
                var inputField = SettingsMenu.GetComponentInChildren<TMPro.TMP_InputField>(); // or use UnityEngine.UI.InputField for the legacy UI
                if (inputField != null)
                {
                    Name = inputField.text; // Update Name with the input field's text
                    NameTag.text = Name;

                    RPC_PlayerName(Name);
                }
                SavePlayerName(); // Now save the updated player name to the file
            }
            else
            {
                // Activate the input field when opening the menu
                var inputField = SettingsMenu.GetComponentInChildren<TMPro.TMP_InputField>();
                if (inputField != null)
                {
                    inputField.Select();
                    inputField.ActivateInputField(); // Activate the input field
                }
            }
        }
    }

    private void SavePlayerName() // Method to save player name to file//-----------------
    {
        const string filePath = "playerName.txt"; // Define the same file path
        File.WriteAllText(filePath, Name); // Write the name to the text file
    }

    // Holds Player Name:
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name) 
    {
        Name = name; // Update the player's name
        if (NameTag != null)
        {
            NameTag.text = Name; // Update the name tag for the player
        }
    }
}
