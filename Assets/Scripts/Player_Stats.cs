using System.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 9_1_2024
/// 
/// Last Updated: 9_3_2024
/// 
///  <<<DON'T TOUCH MY CODE>>>
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 9_1_2024:
/// 
/// Description:
/// 
/// 
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// </summary>

public class Player_Stats : NetworkBehaviour
{
    // Attributes
    [Header("Player Stats Variables")]
    public GameObject[] HealthIndicators = new GameObject[8];
    public Material Health;
    public float revealDelay = 0.5f;
    public float playerHP;

    private float maxHP = 100f;
    private float hpPerIndicator;

    // HP Colors (Full-Low):
    private Color[] Colors =
    {
        ColorUtility.TryParseHtmlString("#5BD4DC", out var c1) ? c1 : Color.white, // Full
        ColorUtility.TryParseHtmlString("#58DE8D", out var c2) ? c2 : Color.white,
        ColorUtility.TryParseHtmlString("#7DDE59", out var c3) ? c3 : Color.white,
        ColorUtility.TryParseHtmlString("#EAD74D", out var c4) ? c4 : Color.white,
        ColorUtility.TryParseHtmlString("#EE7E48", out var c5) ? c5 : Color.white,
        ColorUtility.TryParseHtmlString("#ED1C24", out var c6) ? c6 : Color.white,
        ColorUtility.TryParseHtmlString("#880015", out var c7) ? c7 : Color.white, // Low
        ColorUtility.TryParseHtmlString("#000000", out var c8) ? c8 : Color.white  // Ignored
    };

    // Start is called before the first frame update
    void Start()
    {
        // Calculate HP per health indicator
        hpPerIndicator = maxHP / HealthIndicators.Length;
        playerHP = maxHP; // Initialize playerHP to maxHP

        ApplyMaterialToObjects();
        UpdateMaterialColor();
    }

    // Apply material to health indicators
    void ApplyMaterialToObjects()
    {
        foreach (GameObject obj in HealthIndicators)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = Health;
            }
        }
    }

    // Restore full HP gradually
    public void RestoreFullHP()
    {
        StartCoroutine(RestoreFullHPCoroutine());
    }

    private IEnumerator RestoreFullHPCoroutine()
    {
        float targetHP = maxHP;

        for (int i = 0; i < HealthIndicators.Length && playerHP < targetHP; i++)
        {
            GameObject hpIndicator = HealthIndicators[i];

            if (!hpIndicator.activeSelf)
            {
                hpIndicator.SetActive(true);
                playerHP = Mathf.Min(playerHP + hpPerIndicator, maxHP);
                Debug.Log($"Reactivated health indicator at index {i}. Player HP: {playerHP}");

                UpdateMaterialColor();
                yield return new WaitForSeconds(revealDelay);
            }
        }

        playerHP = maxHP; // Ensure HP is fully restored to 100
        UpdateMaterialColor();
    }

    // Handle taking damage
    public void TakeDamage(float damage)
    {
        playerHP = Mathf.Max(playerHP - damage, 0);
        UpdateHealthIndicators();
        UpdateMaterialColor();
    }

    // Update health indicators based on current HP
    private void UpdateHealthIndicators()
    {
        for (int i = HealthIndicators.Length - 1; i >= 0; i--)
        {
            GameObject hpIndicator = HealthIndicators[i];
            float indicatorHPMin = i * hpPerIndicator;

            // Hide all indicators if health reaches 0
            if (playerHP == 0)
            {
                hpIndicator.SetActive(false);
            }
            // Otherwise, deactivate indicators based on player HP
            else if (playerHP < indicatorHPMin)
            {
                hpIndicator.SetActive(false);
            }
        }

        UpdateMaterialColor();
    }

    // Update material color based on health
    private void UpdateMaterialColor()
    {
        float healthPercentage = playerHP / maxHP;

        Color color;
        if (healthPercentage == 1f)
        {
            color = Colors[0]; // Full health
        }
        else if (healthPercentage > 0.875f)
        {
            color = Colors[1];
        }
        else if (healthPercentage > 0.75f)
        {
            color = Colors[2];
        }
        else if (healthPercentage > 0.625f)
        {
            color = Colors[3];
        }
        else if (healthPercentage > 0.5f)
        {
            color = Colors[4];
        }
        else if (healthPercentage > 0.375f)
        {
            color = Colors[5];
        }
        else if (healthPercentage > 0.25f)
        {
            color = Colors[6];
        }
        else
        {
            color = Colors[6]; // Critical health
        }

        Health.color = color;
    }
}
