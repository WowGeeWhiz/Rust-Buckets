using UnityEngine;
using TMPro;
using System.IO;
using Fusion;

public class Fusion_InputFieldAllCaps : NetworkBehaviour
{
    private TMP_InputField tmpInputField; // For TMP Input Field
    public string playerName;

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
    }

    private void OnDestroy()
    {
        if (tmpInputField != null)
        {
            tmpInputField.onValueChanged.RemoveListener(OnValueChanged);
        }
    }
}
