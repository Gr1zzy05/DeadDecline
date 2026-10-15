using System;
using System.Collections;
using UnityEngine;

public abstract class WeaponSystem : MonoBehaviour
{
    public float damage = 10f;
    [Min(0.01f)] public float attackCooldown = 0.5f;
    [Header("Viewmodel attack timing")]
    [Range(0, 1)] public float impactNormalizedTime = 0.1f;
    [Min(0.1f)] public float animationStartTimeout = 1f;
    protected float nextAttackTime;
    private bool attackPending;
    private int enabledFrame;
    protected virtual Animator AttackAnimator => null;
    protected virtual void OnEnable() { enabledFrame = Time.frameCount; }
    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        attackPending = false;
    }
    protected void BlockInputForAnimationEvaluation() { enabledFrame = Time.frameCount; }
    protected bool CanBeginAttack => !attackPending && Time.time >= nextAttackTime &&
        Time.frameCount > enabledFrame + 1 && !AnimationBlocksAttack();

    private bool AnimationBlocksAttack()
    {
        foreach (var animator in GetComponentsInChildren<Animator>(true))
        {
            if (!animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) continue;
            for (int layer = 0; layer < animator.layerCount; layer++)
            {
                var current = animator.GetCurrentAnimatorStateInfo(layer);
                if (current.IsTag("Equip") || current.IsTag("Reload") || current.IsTag("Attack")) return true;
                if (animator.IsInTransition(layer))
                {
                    var next = animator.GetNextAnimatorStateInfo(layer);
                    if (next.IsTag("Equip") || next.IsTag("Reload") || next.IsTag("Attack")) return true;
                }
            }
        }
        return false;
    }

    protected void QueueImpact(Action impact)
    {
        attackPending = true;
        nextAttackTime = Time.time + Mathf.Max(0.01f, attackCooldown);
        var animator = AttackAnimator;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
        {
            impact();
            attackPending = false;
            return;
        }
        StartCoroutine(WaitForImpact(animator, impact));
    }

    private IEnumerator WaitForImpact(Animator animator, Action impact)
    {
        float deadline = Time.time + Mathf.Max(0.1f, animationStartTimeout);
        bool entered = false;
        while (animator != null && animator.isActiveAndEnabled)
        {
            yield return null;
            var current = animator.GetCurrentAnimatorStateInfo(0);
            bool transitioning = animator.IsInTransition(0);
            var next = transitioning ? animator.GetNextAnimatorStateInfo(0) : default;
            if (current.IsTag("Equip") || current.IsTag("Reload") ||
                (transitioning && (next.IsTag("Equip") || next.IsTag("Reload")))) break;
            bool currentAttack = current.IsTag("Attack");
            bool incomingAttack = transitioning && next.IsTag("Attack");
            if (currentAttack || incomingAttack)
            {
                entered = true;
                float progress = incomingAttack ? next.normalizedTime : current.normalizedTime;
                if (progress >= Mathf.Clamp01(impactNormalizedTime))
                {
                    impact();
                    break;
                }
            }
            else if (entered || Time.time >= deadline)
            {
                Debug.LogWarning($"{name}: Attack ended before impact. Check the Attack state tag, exit time and Impact Normalized Time.", this);
                break;
            }
        }
        attackPending = false;
    }
    public virtual void TryReload() { }
    public virtual bool TryAddAmmo(int amount) { return false; }
    public virtual void UpdateUI() { }
    public virtual void TryAttack()
    {
        if (!CanBeginAttack) return;
        PerformAttack();
    }
    protected abstract void PerformAttack();
}