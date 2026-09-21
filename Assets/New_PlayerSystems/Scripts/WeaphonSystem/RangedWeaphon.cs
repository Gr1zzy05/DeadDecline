using UnityEngine;
using System.Collections;
public class RangedWeaphon : WeaponSystem
{
[Header("Gun Setup")]
    public Transform muzzlePoint;
    public float range = 50f;

    [Header("Ammo System")]
    public int magSize = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 90;

    [Header("Animation Sync")]
    public Animator armsAnimator;
    public Animator gunAnimator;
    public string fireTrigger = "Fire";
    public string reloadTrigger = "Reload";

   [Header("Jamming Mechanics")]
    [Range(0f, 100f)] public float jamChance = 5f;
    public string jamTrigger = "Jam";
    public string unjamTrigger = "Unjam"; // Add this new trigger
    private bool isJammed = false;

    public override void TryAttack()
    {
        // 1. Block firing if already jammed
        if (isJammed)
        {
            Debug.Log("Gun is jammed! Press Reload to clear the chamber.");
            return; 
        }

        if (Time.time >= nextAttackTime && currentAmmo > 0)
        {
            // 2. Roll the dice for a malfunction
            float roll = Random.Range(0f, 100f);
            if (roll <= jamChance)
            {
                TriggerJam();
                return; // Abort the shot
            }

            // 3. Normal Firing
            PerformAttack();
            currentAmmo--;
            UpdateUI();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void TriggerJam()
    {
        isJammed = true;
        Debug.Log("Gun Jammed!");
        
        if (armsAnimator != null) armsAnimator.SetTrigger(jamTrigger);
        if (gunAnimator != null) gunAnimator.SetTrigger(jamTrigger);
        
        UpdateUI();
        nextAttackTime = Time.time + attackCooldown; // Prevent instant input spam
    }

    public override void TryReload()
    {
       // 1. If jammed, play the specific UNJAM animation
        if (isJammed)
        {
            isJammed = false;
            Debug.Log("Jam cleared!");
            
            if (armsAnimator != null) armsAnimator.SetTrigger(unjamTrigger);
            if (gunAnimator != null) gunAnimator.SetTrigger(unjamTrigger);

            UpdateUI();
            return; 
        }

        // 2. Normal Reload Logic
        if (currentAmmo == magSize || reserveAmmo <= 0) return;

        if (armsAnimator != null) armsAnimator.SetTrigger(reloadTrigger);
        if (gunAnimator != null) gunAnimator.SetTrigger(reloadTrigger);

        int bulletsNeeded = magSize - currentAmmo;
        int bulletsToLoad = Mathf.Min(bulletsNeeded, reserveAmmo);
        currentAmmo += bulletsToLoad;
        reserveAmmo -= bulletsToLoad;
        
        UpdateUI();
    }
    public override bool TryAddAmmo(int amount)
    {
        reserveAmmo += amount;
        
        Debug.Log($"Added {amount} ammo. New Reserve: {reserveAmmo}");
        
        // This is the critical line that forces the screen text to change!
        UpdateUI(); 
        
        return true; 
    }
    protected override void PerformAttack()
    {
        if (armsAnimator != null) armsAnimator.SetTrigger(fireTrigger);
        if (gunAnimator != null) gunAnimator.SetTrigger(fireTrigger);
        Debug.DrawRay(muzzlePoint.position, muzzlePoint.forward * range, Color.red, 2f);

        if (Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out RaycastHit hit, range))
        {   
            Debug.Log($"Raycast hit: {hit.collider.name}");
            Health targetHealth = hit.collider.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                Debug.Log($"Dealt {damage} damage to {hit.collider.name}");
                targetHealth.TakeDamage(damage);
            }
            else
            {
                Debug.Log("Raycast hit nothing.");
            }
        }
    }
    
    public override void UpdateUI()
    {
        if (PlayerUI.Instance != null)
        {
            
            // Optional: You could update the UI script to show a red "JAMMED" warning text here
            PlayerUI.Instance.UpdateAmmoDisplay(currentAmmo, reserveAmmo);
            PlayerUI.Instance.SetJamWarning(isJammed);
        }
    }
}
