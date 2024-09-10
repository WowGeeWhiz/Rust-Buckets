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
/// Parent script for all weapons.
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
public class Weapon_Stats : NetworkBehaviour
{
    // Common variables for all weapons:------------------------------------
    [SerializeField] protected GameObject ammo;
    [SerializeField] protected Texture weaponSkin;
    [SerializeField] protected float range;
    [SerializeField] protected float damageProjectile;
    [SerializeField] protected float damageMelee;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected int capacity;
    [SerializeField] protected string slot;
    [SerializeField] protected string description;
    [SerializeField] protected GameObject muzzleFlash;
    [SerializeField] protected GameObject impactEffect;
    [SerializeField] protected AudioClip[] fireSounds;
    [SerializeField] protected AudioSource[] audioSource_Projectile;
    [SerializeField] protected GameObject launchPoint;

    // Properties for accessing fields
    public GameObject Ammo => ammo;
    public Texture WeaponSkin => weaponSkin;
    public float Range => range;
    public float DamageProjectile => damageProjectile;
    public float DamageMelee => damageMelee;
    public float ReloadTime => reloadTime;
    public int Capacity => capacity;
    public string Slot => slot;
    public string Description => description;
    public GameObject MuzzleFlash => muzzleFlash;
    public GameObject ImpactEffect => impactEffect;
    public AudioClip[] FireSounds => fireSounds;
    public AudioSource[] AudioSource_P => audioSource_Projectile;
    public GameObject LaunchPoint => launchPoint;

    // Method to be overridden by child classes:----------------------------
    public virtual void Fire()
    {
        Debug.Log("Firing weapon with damage: " + damageProjectile);
    }

    public virtual void Reload()
    {
        Debug.Log("Reloading weapon with capacity: " + capacity);
    }

    public virtual void Melee() 
    {
        Debug.Log("Melee damage: " + damageMelee);
    }

    public virtual void PlaySoundProjectile()
    {
        if (fireSounds.Length > 0 && audioSource_Projectile.Length > 0)
        {
            // Select a random sound from the array
            AudioClip clip1 = fireSounds[Random.Range(0, fireSounds.Length)];

            // Find an available AudioSource that is not currently playing
            AudioSource availableAudioSource1 = null;
            foreach (AudioSource audioSource1 in audioSource_Projectile)
            {
                if (!audioSource1.isPlaying)
                {
                    availableAudioSource1 = audioSource1;
                    break; // Exit loop once an available AudioSource is found
                }
            }

            // If no AudioSource is found, use the first one (you could also handle this differently)
            if (availableAudioSource1 == null)
            {
                availableAudioSource1 = audioSource_Projectile[0];
            }

            // Set the audio source clip
            availableAudioSource1.clip = clip1;

            // Play the sound from the start point
            availableAudioSource1.Play();
        }
    }


}

