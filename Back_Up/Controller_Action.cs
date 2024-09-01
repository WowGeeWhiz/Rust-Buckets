using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 9_1_2024
/// 
/// Last Updated: NULL
/// 
///  <<<DON'T TOUCH MY CODE>>>
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 8_20_2024:
/// 
/// Description:
/// Target Reticle set-up switches between blue as neutral to red if there is an enemy to attack.
/// All enemies will be located in the layer mask called: Interactable, This reduces the amount of objects in the pool to check.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// -Place all targets in the appropriate layer mask: Interactable
/// 
/// -All enemies must be given the tag: Enemy
/// 
/// -The Rawimages used for the reticle parts must be used from the seen, maybe it should have a search/adder implemented in start to self set-up.
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// </summary>

public class Controller_Action : NetworkBehaviour
{

    //UI
    [Header("UI Variables")]
    public RawImage ReticleDynamic;
    public RawImage ReticleStatic;
    public Texture ReticleStaticBlue;
    public Texture ReticleStaticRed;
    public Texture ReticleDynamicBlue;
    public Texture ReticleDynamicRed;
    public GameObject Camera;
    public float detectionNarrowSite;
    public float maxOffset;

    private RectTransform ReticleRectTransform;

    // Life_Cycle Methods:-------------------------------------------------------------------------------------------------------

    void Start()
    {

        // UI:
        ReticleRectTransform = ReticleDynamic.GetComponent<RectTransform>();
        ReticleStatic.texture = ReticleStaticBlue;
        ReticleDynamic.texture = ReticleDynamicBlue;
    }

    void Update()
    {
        UI_ReticleDynamics();
        UI_ReticleColorReactor();
    }

    // Reticle Methods:=--------------------------------------------------------------------------------------------------------------

    private void UI_ReticleDynamics()
    {

        // Get the horizontal and vertical input from the right thumbstick:
        float horizontalRight = Gamepad.all[0].rightStick.ReadValue().x;
        float verticalRight = Gamepad.all[0].rightStick.ReadValue().y;

        // Invert the horizontal axis:
        horizontalRight *= -1f;

        // Calculate the new position of the reticle:
        Vector2 targetPosition = ReticleRectTransform.anchoredPosition + new Vector2(horizontalRight, verticalRight) * 5f;

        // Clamp the position of the reticle to a certain range:
        targetPosition.x = Mathf.Clamp(targetPosition.x, -maxOffset, maxOffset);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -maxOffset, maxOffset);

        // Smoothly move the reticle towards the target position:
        ReticleRectTransform.anchoredPosition = Vector2.Lerp(ReticleRectTransform.anchoredPosition, targetPosition, Time.deltaTime * 5f);

        // Check if both horizontal and vertical input magnitudes are zero:
        if (Mathf.Approximately(horizontalRight, 0f) && Mathf.Approximately(verticalRight, 0f))
        {
            // Gradually move the reticle back to the center position:
            Vector2 centerPosition = Vector2.zero; // (Center position)
            ReticleRectTransform.anchoredPosition = Vector2.Lerp(ReticleRectTransform.anchoredPosition, centerPosition, Time.deltaTime * 5f);
        }
    }


    private void UI_ReticleColorReactor()
    {
        int interactableLayerIndex = LayerMask.NameToLayer("Interactable");
        LayerMask interactableLayerMask = 1 << interactableLayerIndex;

        // Cast a ray in the forward direction of the player:
        Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);

        // Check if the ray hits any objects within the detection distance:
        if (Physics.Raycast(ray, out RaycastHit hit, detectionNarrowSite, interactableLayerMask))
        {
            // Check if the hit object has the desired tag:
            if (hit.collider.CompareTag("Enemy"))
            {

                ReticleStatic.texture = ReticleStaticRed;
                ReticleDynamic.texture = ReticleDynamicRed;

            }
        }
        else
        {
            ReticleStatic.texture = ReticleStaticBlue;
            ReticleDynamic.texture = ReticleDynamicBlue;
        }
    }
}
