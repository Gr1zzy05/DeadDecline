using UnityEngine;

public class Health : MonoBehaviour
{public float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} was destroyed!");
        // For a blockout, simply destroy the object. 
        // In production, you would trigger a ragdoll, death animation, or object pooling here.
        Destroy(gameObject);
    }
}
