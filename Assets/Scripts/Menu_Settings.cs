using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 9_17_2024
/// 
/// Last Updated: 9_17_2024
/// 
///  <<<DON'T TOUCH MY CODE>>>
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 9_17_2024:
/// 
/// Description:
/// 
/// Options menu.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// -Toggle coontrolled by the canvas toggles.
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// </summary>
public class Menu_Settings : NetworkBehaviour
{
    public GameObject Menu_Setting;
    public GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        Menu_Setting.SetActive(false);  // Start with the menu hidden
    }

    // Update is called once per frame
    void Update()
    {
        // Handle the input detection inside Update
        Controller_Options();
    }

    // Settings Menu Control:-----------------------------------------------------------------

    public void Controller_Options()
    {
        if (IsOwner)
        {
            // Check if any gamepads are connected
            if (Gamepad.all.Count > 0)
            {
                Gamepad gamepad = Gamepad.all[0];  // Access the first connected gamepad

                // Check if the Options (start) button is pressed
                if (gamepad.startButton.wasPressedThisFrame)
                {
                    // Toggle the visibility of the Menu_Setting GameObject
                    Menu_Setting.SetActive(!Menu_Setting.activeSelf);
                }
            }
        }
    }

}
