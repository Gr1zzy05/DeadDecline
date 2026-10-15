using UnityEngine;
using System.Collections;

public class RangedWeaphon : WeaponSystem
{   
    [Header("Audio Feedback")]
    public AudioSource weaponAudioSource;
    public AudioClip fireSound;
    public AudioClip dryFireSound; // Optional: For when the gun is jammed or empty

    [Header("Camera Recoil")]
    public float recoilPitch = 2f; // How many degrees the camera kicks up
    public float recoilDuration = 0.05f; // How fast it kicks up
    public float returnDuration = 0.2f; // How fast it settles back down
    private Coroutine recoilCoroutine;

    [Header("Gameplay gunshot noise")]
    [Min(0)] public float gunshotLoudness = 5f;
    [Min(0)] public float gunshotFullVolumeRadius = 5f;
    [Min(0.1f)] public float gunshotMaxRadius = 50f;

    [Header("Attack debug")]
    public bool showHitboxOnAttack = true;
    [Min(0.02f)] public float hitboxDisplayDuration = 0.35f;
    public Color hitboxColor = Color.yellow;
    public LayerMask hitMask = ~0;

    [Header("Setup")]
    public Transform muzzlePoint;
    public float range = 50f;
    private Transform playerCamera;

    [Header("Effects")]
    public Light muzzleFlashLight;
    public float effectDuration = 0.05f;

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
    public string unjamTrigger = "Unjam";
    private bool isJammed = false;

    protected override Animator AttackAnimator => armsAnimator != null &&
        armsAnimator.runtimeAnimatorController != null ? armsAnimator : gunAnimator;
    private Coroutine effectCoroutine;

    private void Reset()
    {
        damage = 34f;
        impactNormalizedTime = 0.05f;
    }

    private void Start()
    {
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
        
        Camera cam = GetComponentInParent<Camera>();
        playerCamera = cam != null ? cam.transform : Camera.main.transform;
    }

    public override void TryAttack()
    {
        // 1. Block firing if jammed
        if (isJammed)
        {
            Debug.Log("Gun is jammed! Press Reload to clear the chamber.");
            return;
        }

        if (!CanBeginAttack || currentAmmo <= 0) return;

        // 2. Roll for jam exactly when the player pulls the trigger
        if (Random.Range(0f, 100f) <= jamChance)
        {
            TriggerJam();
            return; 
        }

        PerformAttack();
    }

    private void TriggerJam()
    {
        isJammed = true;
        Debug.Log("Gun Jammed!");
        
        // Lock the weapon system so the player can't spam inputs while the gun jams
        BlockInputForAnimationEvaluation();

        if (armsAnimator != null && armsAnimator.runtimeAnimatorController != null) armsAnimator.SetTrigger(jamTrigger);
        if (gunAnimator != null && gunAnimator.runtimeAnimatorController != null) gunAnimator.SetTrigger(jamTrigger);
        
        UpdateUI();
    }

    public override bool TryAddAmmo(int amount)
    {
        reserveAmmo += amount;
        UpdateUI();
        return true; 
    }

    public override void TryReload()
    {
        // 3. Hijack the reload button to clear the jam
        if (isJammed)
        {
            isJammed = false;
            Debug.Log("Jam cleared!");
            
            BlockInputForAnimationEvaluation();
            
            if (armsAnimator != null && armsAnimator.runtimeAnimatorController != null) armsAnimator.SetTrigger(unjamTrigger);
            if (gunAnimator != null && gunAnimator.runtimeAnimatorController != null) gunAnimator.SetTrigger(unjamTrigger);
            
            UpdateUI();
            return; 
        }

        if (!CanBeginAttack || currentAmmo == magSize || reserveAmmo <= 0) return;

        BlockInputForAnimationEvaluation();
        
        if (armsAnimator != null && armsAnimator.runtimeAnimatorController != null) armsAnimator.SetTrigger(reloadTrigger);
        if (gunAnimator != null && gunAnimator.runtimeAnimatorController != null) gunAnimator.SetTrigger(reloadTrigger);

        int bulletsNeeded = magSize - currentAmmo;
        int bulletsToLoad = Mathf.Min(bulletsNeeded, reserveAmmo);
        currentAmmo += bulletsToLoad;
        reserveAmmo -= bulletsToLoad;
        
        Debug.Log("Reloaded!");
        UpdateUI();
    }

    protected override void PerformAttack()
    {   
        if (armsAnimator != null && armsAnimator.runtimeAnimatorController != null) armsAnimator.SetTrigger(fireTrigger);
        if (gunAnimator != null && gunAnimator.runtimeAnimatorController != null) gunAnimator.SetTrigger(fireTrigger);
        QueueImpact(ApplyShot);
    }

    private void ApplyShot()
    {
        if (currentAmmo <= 0) {
            if (weaponAudioSource != null && dryFireSound != null) weaponAudioSource.PlayOneShot(dryFireSound);
            return;
        }
        currentAmmo--;
        UpdateUI();
        
        Transform muzzle = muzzlePoint != null ? muzzlePoint : transform;
        Vector3 rayOrigin = playerCamera != null ? playerCamera.position : muzzle.position;
        Vector3 rayDirection = playerCamera != null ? playerCamera.forward : muzzle.forward;
        Vector3 endPoint = rayOrigin + rayDirection * range;
        
        Transform owner = AttackDebug.FindOwner(this);
        RaycastHit closest = default;
        float closestDistance = float.PositiveInfinity;
        
        foreach (var candidate in Physics.RaycastAll(rayOrigin, rayDirection, range,
            hitMask, QueryTriggerInteraction.Ignore))
        {
            if (AttackDebug.IsOwner(candidate.collider, owner) || candidate.distance >= closestDistance) continue;
            closest = candidate;
            closestDistance = candidate.distance;
        }
        
        if (!float.IsPositiveInfinity(closestDistance))
        {
            endPoint = closest.point;
            var targetHealth = closest.collider.GetComponentInParent<Health>();
            if (targetHealth != null && targetHealth.transform != owner) targetHealth.TakeWeaponDamage(damage, owner);
        }
        
        NoiseSystem.Emit(muzzle.position, gunshotLoudness, gunshotFullVolumeRadius,
            gunshotMaxRadius, owner, NoiseSystem.NoiseKind.Gunshot);
            
        if (showHitboxOnAttack)
        {
            AttackDebug.Ray(muzzle.position, endPoint, hitboxColor, hitboxDisplayDuration);
            AttackDebug.Sphere(endPoint, 0.08f, hitboxColor, hitboxDisplayDuration);
        }
        
        if (effectCoroutine != null) StopCoroutine(effectCoroutine);
        effectCoroutine = StartCoroutine(PlayShootEffects(muzzle.position, endPoint));
       
        if (weaponAudioSource != null && fireSound != null)
        {
            weaponAudioSource.PlayOneShot(fireSound);
        }
        if (recoilCoroutine != null) StopCoroutine(recoilCoroutine);
        recoilCoroutine = StartCoroutine(ApplyCameraRecoil());
            
        if (showHitboxOnAttack)
        {
            AttackDebug.Ray(muzzle.position, endPoint, hitboxColor, hitboxDisplayDuration);
            AttackDebug.Sphere(endPoint, 0.08f, hitboxColor, hitboxDisplayDuration);
        }
        
        if (effectCoroutine != null) StopCoroutine(effectCoroutine);
        effectCoroutine = StartCoroutine(PlayShootEffects(muzzle.position, endPoint));
    }

    private IEnumerator ApplyCameraRecoil()
    {
        if (playerCamera == null) yield break;

        Quaternion originalRotation = playerCamera.localRotation;
        // Kick upwards (negative X axis in Unity) and slightly randomize horizontal kick
        Quaternion recoilRotation = originalRotation * Quaternion.Euler(-recoilPitch, Random.Range(-0.5f, 0.5f), 0f);

        // Snap up
        float time = 0;
        while (time < recoilDuration)
        {
            playerCamera.localRotation = Quaternion.Slerp(originalRotation, recoilRotation, time / recoilDuration);
            time += Time.deltaTime;
            yield return null;
        }

        // Settle back down
        time = 0;
        while (time < returnDuration)
        {
            playerCamera.localRotation = Quaternion.Slerp(recoilRotation, originalRotation, time / returnDuration);
            time += Time.deltaTime;
            yield return null;
        }

        playerCamera.localRotation = originalRotation;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
        effectCoroutine = null;
    }

    public override void UpdateUI()
    {
        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.UpdateAmmoDisplay(currentAmmo, reserveAmmo);
            // Assuming you added SetJamWarning to your PlayerUI script as discussed earlier
            PlayerUI.Instance.SetJamWarning(isJammed);
        }
    }

    private IEnumerator PlayShootEffects(Vector3 start, Vector3 end)
    {
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = true;
        yield return new WaitForSeconds(effectDuration);
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
    }
}