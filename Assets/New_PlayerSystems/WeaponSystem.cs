using UnityEngine;
using UnityEngine.InputSystem;
public abstract class WeaponSystem : MonoBehaviour
{
public float damage = 10f;
    public float attackCooldown = 0.5f;
    protected float nextAttackTime;
public virtual void TryReload() { }
public virtual bool TryAddAmmo(int amount) { return false; }
public virtual void UpdateUI() { }
    public virtual void TryAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    protected abstract void PerformAttack();
}
