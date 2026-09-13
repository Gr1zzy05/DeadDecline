using UnityEngine;
using System.Collections;
public class Meleeweaphon : WeaponSystem
{
[Header("Setup")]
    public Transform strikePoint;
    public float hitRadius = 1.5f;

    [Header("Animation")]
    public Animator weaponAnimator;
    public string swingTriggerName = "Swing";

    protected override void PerformAttack()
    {
        // 1. Play Animation
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(swingTriggerName);
        }

        // 2. Perform Logic
        Collider[] hitColliders = Physics.OverlapSphere(strikePoint.position, hitRadius);
        foreach (var hit in hitColliders)
        {
            Health targetHealth = hit.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }
    }
    public override void UpdateUI()
{
    if (PlayerUI.Instance != null)
    {
        PlayerUI.Instance.ShowMeleeDisplay();
    }
}
    private void OnDrawGizmosSelected()
    {
        if (strikePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(strikePoint.position, hitRadius);
        }
    }
}
