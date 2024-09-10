using UnityEngine;
using Unity.Netcode;
/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 9_5_2024
/// 
/// Last Updated: 9_5_2024
/// 
///  <<<DON'T TOUCH MY CODE>>>
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 9_5_2024:
/// 
/// Description:
/// 
/// Visualizes raycasts.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// -This gives visualization to Raycastors;
/// 
/// -Turn this off via bool condition before building exe.
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// </summary>


public class NetworkedRaycastVisualizer : NetworkBehaviour
{
    public float rayLength = 300f; // Length of the ray
    public Color rayColor = Color.green; // Color of the ray
    public bool isVisualizationEnabled = false; // Boolean to control visualization

    void Update()
    {
        if (isVisualizationEnabled) // Only visualize ray if enabled
        {
            // Perform the raycast
            RaycastHit hit;
            Vector3 rayStart = transform.position; // Start ray from the object's position
            Vector3 rayDirection = transform.forward; // Use the object's forward direction
            Vector3 rayEnd = rayStart + rayDirection * rayLength;

            // Check for hits
            if (Physics.Raycast(rayStart, rayDirection, out hit, rayLength))
            {
                rayEnd = hit.point; // Update end point to hit point
            }

            // Draw the ray in the Game view for debugging
            Debug.DrawRay(rayStart, rayEnd - rayStart, rayColor);
        }
    }

    // Method to enable or disable raycast visualization
    public void SetRaycastVisualization(bool isEnabled)
    {
        isVisualizationEnabled = isEnabled;
    }
}





