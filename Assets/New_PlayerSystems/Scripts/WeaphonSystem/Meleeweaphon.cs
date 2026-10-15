using System.Collections.Generic;
using UnityEngine;

public class Meleeweaphon : WeaponSystem
{
    [Header("Hitbox")]
    public Transform strikePoint;
    public Vector3 hitboxOffset;
    [Min(0.01f)] public float hitRadius = 1.5f;
    public LayerMask hitMask = ~0;
    public bool showHitboxOnAttack = true;
    [Min(0.02f)] public float hitboxDisplayDuration = 0.35f;
    public Color hitboxColor = Color.yellow;
    [Header("Animation")]
    public Animator weaponAnimator;
    public string swingTriggerName = "Swing";
    protected override Animator AttackAnimator => weaponAnimator;
    private CharacterController playerController;
    private Vector3 lastPlayerPosition;
    private Vector3 HitCenter => (strikePoint != null ? strikePoint : transform).TransformPoint(hitboxOffset);

    private void Start()
    {
        playerController = GetComponentInParent<CharacterController>();
        if (playerController != null) lastPlayerPosition = playerController.transform.position;
    }

    private void Update()
    {
        if (weaponAnimator == null || playerController == null) return;
        Vector3 current = playerController.transform.position;
        Vector3 movement = current - lastPlayerPosition;
        movement.y = 0;
        weaponAnimator.SetBool("isWalking", movement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f) > 0.1f);
        lastPlayerPosition = current;
    }

    protected override void PerformAttack()
    {
        if (weaponAnimator != null) weaponAnimator.SetTrigger(swingTriggerName);
        QueueImpact(ApplyHit);
    }

    private void ApplyHit()
    {
        Vector3 center = HitCenter;
        float radius = Mathf.Max(0.01f, hitRadius);
        if (showHitboxOnAttack) AttackDebug.Sphere(center, radius, hitboxColor, hitboxDisplayDuration);
        Transform owner = AttackDebug.FindOwner(this);
        var damaged = new HashSet<Health>();
        foreach (var hit in Physics.OverlapSphere(center, radius, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (AttackDebug.IsOwner(hit, owner)) continue;
            var health = hit.GetComponentInParent<Health>();
            if (health == null || health.transform == owner || !damaged.Add(health)) continue;
            health.TakeWeaponDamage(damage, owner);
        }
    }

    public override void UpdateUI()
    {
        if (PlayerUI.Instance != null) PlayerUI.Instance.ShowMeleeDisplay();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = hitboxColor;
        Gizmos.DrawWireSphere(HitCenter, Mathf.Max(0.01f, hitRadius));
    }
}