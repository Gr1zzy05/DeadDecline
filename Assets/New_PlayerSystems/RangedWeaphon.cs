using UnityEngine;
using System.Collections;
public class RangedWeaphon : WeaponSystem
{
[Header("Setup")]
    public Transform muzzlePoint;
    public float range = 50f;

    [Header("Effects")]
    public LineRenderer tracerLine;
    public Light muzzleFlashLight;
    public float effectDuration = 0.05f; // Short duration for a blockout

    [Header("Ammo System")]
    public int magSize = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 90;

    private Coroutine effectCoroutine;

    private void Start()
    {
        if (tracerLine != null) tracerLine.enabled = false;
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
    }

    public override void TryAttack()
    {
        if (Time.time >= nextAttackTime && currentAmmo > 0)
        {
            PerformAttack();
            currentAmmo--;
            UpdateUI();
            nextAttackTime = Time.time + attackCooldown;
        }
        else if (currentAmmo <= 0)
        {
            Debug.Log("Click! Out of ammo.");
        }
    }

    public override bool TryAddAmmo(int amount)
{
    reserveAmmo += amount;
    UpdateUI();
    return true; 
}
    public override void TryReload()
    {
        if (currentAmmo == magSize || reserveAmmo <= 0) return;

        int bulletsNeeded = magSize - currentAmmo;
        int bulletsToLoad = Mathf.Min(bulletsNeeded, reserveAmmo);

        currentAmmo += bulletsToLoad;
        reserveAmmo -= bulletsToLoad;
        
        Debug.Log("Reloaded!");
        UpdateUI();
    }

    protected override void PerformAttack()
    {
        // 1. Perform Logic (to know where to draw the tracer)
        Vector3 endPoint;
        if (Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out RaycastHit hit, range))
        {
            endPoint = hit.point;
            
            Health targetHealth = hit.collider.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }
        else
        {
            endPoint = muzzlePoint.position + (muzzlePoint.forward * range);
        }

        // 2. Play Effects using the calculated endPoint
        if (effectCoroutine != null) StopCoroutine(effectCoroutine);
        effectCoroutine = StartCoroutine(PlayShootEffects(muzzlePoint.position, endPoint));
    }

    public override void UpdateUI()
    {
        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.UpdateAmmoDisplay(currentAmmo, reserveAmmo);
        }
    }

    private IEnumerator PlayShootEffects(Vector3 start, Vector3 end)
    {
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = true;

        if (tracerLine != null)
        {
            tracerLine.enabled = true;
            tracerLine.SetPosition(0, start);
            tracerLine.SetPosition(1, end);
        }

        yield return new WaitForSeconds(effectDuration);

        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
        if (tracerLine != null) tracerLine.enabled = false;
    }
}
