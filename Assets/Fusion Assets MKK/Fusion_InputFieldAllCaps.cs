using UnityEngine;
using TMPro;
using System.IO;
using Fusion;

public class Fusion_InputFieldAllCaps : MonoBehaviour 
{
    private TMP_InputField tmpInputField; // For TMP Input Field
    public string playerName;

    private const string filePath = "playerName.txt"; // Define the file path

    private void Awake()
    {
        // Try to find TMP_InputField in this GameObject or its children
        tmpInputField = GetComponentInChildren<TMP_InputField>();
    }

    private void Start()
    {
        if (tmpInputField != null)
        {
            tmpInputField.onValueChanged.AddListener(OnValueChanged);
            LoadPlayerName(); // Load the player name from the file
        }
    }

    private void OnValueChanged(string value)
    {
        // Convert input to uppercase
        if (tmpInputField != null)
        {
            tmpInputField.text = value.ToUpper();
            // Set the caret position to the end of the text
            tmpInputField.caretPosition = tmpInputField.text.Length;
        }

        playerName = tmpInputField.text; // Update playerName with the current input
        SavePlayerName(playerName); // Save the name to the file
    }

    private void SavePlayerName(string name)
    {
        File.WriteAllText(filePath, name); // Write the name to the text file
    }

    private void LoadPlayerName()
    {
        if (File.Exists(filePath))
        {
            playerName = File.ReadAllText(filePath); // Read the name from the text file
            tmpInputField.text = playerName; // Set the TMP_InputField text
        }
        else
        {
            // Create the file with the default player name if it doesn't exist
            playerName = ""; // Set default name
            File.WriteAllText(filePath, playerName); // Create the file with the default name
            tmpInputField.text = playerName; // Set the TMP_InputField text
        }
    }

    private void OnDestroy()
    {
        if (tmpInputField != null)
        {
            tmpInputField.onValueChanged.RemoveListener(OnValueChanged);
        }
    }
}
