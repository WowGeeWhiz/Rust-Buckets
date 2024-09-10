using UnityEngine;
using Unity.Netcode;
/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 9_5_2024
/// 
/// Last Updated: 9_10_2024
/// 
///  <<<DON'T TOUCH MY CODE>>>
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 9_5_2024:
/// 
/// Description:
/// 
/// Assault Rifle Script.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// N/A
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Patch: 9_10_2024:
/// 
/// Description:
/// 
/// Added hitscan to the assault rifle as its primary attack.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// N/A
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// </summary>
public class Weapon_AssaultRifle : Weapon_Stats
{

    // Constructor:---------------------------------------------------------
    void Start() 
    {
        //Assigned in the inspector only. If done here it will override the inspector.
        muzzleFlash.SetActive(false);
    }

    // Method to be overridden by child classes:----------------------------
    public override void Fire()
    {
        // If available, show the muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
        }

        // If available, play the sound FX
        PlaySoundProjectile();

        // Ensure LaunchPoint is set
        if (LaunchPoint == null)
        {
            Debug.LogWarning("LaunchPoint is not assigned!");
            return;
        }

        // Create a ray from the LaunchPoint's position forward
        Ray ray = new Ray(LaunchPoint.transform.position, LaunchPoint.transform.forward);
        RaycastHit hit;

        // Use a LineRenderer to visualize the ray
        LineRenderer lineRenderer = LaunchPoint.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            // If no LineRenderer is attached, add one
            lineRenderer = LaunchPoint.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f; // Set line thickness
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
            lineRenderer.material.color = Color.red; // Color of the line
        }

        // Initialize line positions
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, LaunchPoint.transform.position); // Start at launch point

        // Check if the ray hits anything within range
        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Try to get a health component on the hit object
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damageProjectile); // Apply damage to the target
            }

            // Set the second point of the line at the hit point
            lineRenderer.SetPosition(1, hit.point);

            // Optional: Create an impact effect at the hit point
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // If nothing is hit, set the line to the max range
            lineRenderer.SetPosition(1, ray.GetPoint(range));
        }

        // Hide the line after a short duration
        StartCoroutine(HideLine(lineRenderer, 0.1f));
    }

    // Coroutine to hide the line renderer after a short duration
    private System.Collections.IEnumerator HideLine(LineRenderer lineRenderer, float delay)
    {
        yield return new WaitForSeconds(delay);
        lineRenderer.positionCount = 0; // Hide the line
        muzzleFlash.SetActive(false);
    }

    public override void Reload()
    {

    }

    public override void Melee()
    {

    }

    public override void PlaySoundProjectile()
    {
        base.PlaySoundProjectile();
    }

}
