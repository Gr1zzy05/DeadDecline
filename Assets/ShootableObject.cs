using UnityEngine;

public class ShootableObject : MonoBehaviour
{
   [Header("Stats")]
    [Tooltip("Starting health of the object.")]
    public float health = 100f;

    [Header("Effects (Optional)")]
    [Tooltip("Particle effect spawned when the object is hit.")]
    public GameObject hitEffect;
    [Tooltip("Particle effect spawned when the object is destroyed.")]
    public GameObject destroyEffect;

    [Header("Audio (Optional)")]
    [Tooltip("Attach an AudioSource component if you want hit sounds.")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip destroySound;

    // Your weapon script will call this method when its bullet hits this object
    public void TakeDamage(float amount)
    {
        health -= amount;

        // Play the hit sound if assigned
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Spawn hit particle effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Check if the object has lost all its health
        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        // Spawn destruction particle effect
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // Play destruction sound at the object's location before it disappears
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        // Destroy the object from the scene
        Destroy(gameObject);
    }
}
