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
    private CharacterController playerController;
    private Vector3 lastPlayerPosition;
    private void Start()
    {
        // Automatically find the player's movement controller when spawned
       playerController = GetComponentInParent<CharacterController>();
        if (playerController != null)
        {
            lastPlayerPosition = playerController.transform.position;
        }
        else
        {
            Debug.LogError("CRITICAL: Weapon cannot find CharacterController on the Player! Walk animation will fail.");
        }
    }
    private void Update()
    {
        if (weaponAnimator != null && playerController != null)
        {
            // Calculate actual distance moved on the X and Z axes only (ignoring vertical jumping/falling)
            Vector3 currentPos = playerController.transform.position;
            Vector3 horizontalMove = new Vector3(currentPos.x - lastPlayerPosition.x, 0, currentPos.z - lastPlayerPosition.z);
            
            float speed = horizontalMove.magnitude / Time.deltaTime;
            bool isMoving = speed > 0.1f;
            
            weaponAnimator.SetBool("isWalking", isMoving);
            lastPlayerPosition = currentPos;
        }
    }
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
