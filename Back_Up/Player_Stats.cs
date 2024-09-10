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
/// Initialization 8_20_2024:
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
    // Attributes:=--------------------------------------------------------------------------------------------------------------

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

    // Life_Cycle Methods:-------------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Calculate HP per health indicator
        hpPerIndicator = maxHP / HealthIndicators.Length;
        playerHP = maxHP; // Initialize playerHP to maxHP

        ApplyMaterialToObjects();

        // Set initial material color
        UpdateMaterialColor();

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Health Methods:-------------------------------------------------------------------------------------------------------

    void ApplyMaterialToObjects()
    {
        foreach (GameObject obj in HealthIndicators)
        {
            // Check if the object has a Renderer component
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Apply the material to the object's renderer
                renderer.material = Health;
            }
        }
    }

    public void RestoreFullHP()
    {
        StartCoroutine(RestoreFullHPCoroutine());
    }

    private IEnumerator RestoreFullHPCoroutine()
    {
        float targetHP = maxHP;

        while (playerHP < targetHP)
        {
            for (int i = HealthIndicators.Length - 1; i >= 0; i--)
            {
                GameObject hpIndicator = HealthIndicators[i];

                // Check if the indicator is inactive and if the playerHP is less than maxHP
                if (!hpIndicator.activeSelf)
                {
                    // Reactivate the health indicator
                    hpIndicator.SetActive(true);

                    // Increase playerHP by the HP value represented by this indicator
                    playerHP = Mathf.Min(playerHP + hpPerIndicator, maxHP);
                    Debug.Log($"Reactivated health indicator at index {i}: {hpIndicator.name}. Player HP: {playerHP}");

                    // Wait for the specified delay before processing the next indicator
                    yield return new WaitForSeconds(revealDelay);
                }
            }
            UpdateMaterialColor(); // Update color after reactivating indicators
            yield return null; // Yield control to the next frame
        }
    }

    public void TakeDamage(float damage)
    {
        // Apply damage to player HP
        playerHP = Mathf.Max(playerHP - damage, 0);

        // Deactivate health indicators based on damage
        UpdateHealthIndicators();
        UpdateMaterialColor(); // Update color after deactivating indicators
    }

    private void UpdateHealthIndicators()
    {
        float currentHP = playerHP;

        for (int i = HealthIndicators.Length - 1; i >= 0; i--)
        {
            GameObject hpIndicator = HealthIndicators[i];

            // Calculate the HP range for this indicator
            float indicatorHPMin = i * hpPerIndicator;
            float indicatorHPMax = indicatorHPMin + hpPerIndicator;

            // Check if current HP falls within this indicator's range
            if (currentHP < indicatorHPMin)
            {
                // Deactivate if HP is below this indicator's range
                hpIndicator.SetActive(false);
            }
            else
            {
                // Reactivate if HP is above this indicator's range
                hpIndicator.SetActive(true);
            }
        }
    }

    private void UpdateMaterialColor()
    {
        int activeIndicators = 0;
        for (int i = 0; i < HealthIndicators.Length; i++)
        {
            if (HealthIndicators[i].activeSelf)
            {
                activeIndicators++;
            }
        }

        // Determine color based on the number of active indicators
        Color color;
        if (activeIndicators == HealthIndicators.Length)
        {
            color = Colors[0]; // Full health
        }
        else if (activeIndicators > 5)
        {
            color = Colors[3]; // More than 5 indicators
        }
        else if (activeIndicators > 1)
        {
            color = Colors[4]; // Between 2 and 5 indicators
        }
        else
        {
            color = Colors[6]; // Only 1 indicator left
        }

        // Apply the color to the material
        Health.color = color;
    }
}