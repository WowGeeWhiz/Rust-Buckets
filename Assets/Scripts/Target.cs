using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 9_10_2024
/// 
/// Last Updated: 9_10_2024
/// 
///  <<<DON'T TOUCH MY CODE>>>
/// 
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 9_10_2024:
/// 
/// Description:
/// 
/// Simple target practice health feedback system.
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
public class Target : MonoBehaviour
{
    public float health = 100f;

    // Method to apply damage
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Target health: " + health);
        if (health <= 0f)
        {
            Die();
        }
    }

    // Method to handle the target's death
    void Die()
    {
        //Don't destroy;
    }
}
