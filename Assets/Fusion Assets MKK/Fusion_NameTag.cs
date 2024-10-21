using UnityEngine;

public class Fusion_NameTag : MonoBehaviour
{
    private Camera mainCamera;  // Reference to the main camera

    void Start()
    {
        // Find the main camera
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Make the text always face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}
