using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    private enum ZombieState
    {
        Idle,
        Chase
    }

    [Header("Target")]
    [Tooltip("The zombie will find the object tagged \"Player\" automatically.")]
    [SerializeField] private Transform player;

    [Header("Detection Close Range")]
    [Tooltip("If the player is within this distance, the zombie notices them instantly, regardless of facing direction.")]
    [SerializeField] private float closeRangeRadius = 2f;

    [Header("Detection (Vision Cone)")]
    [Tooltip("How far the zombie can see.")]
    [SerializeField] private float eyeRange = 10f;

    [Tooltip("FOV angle, in degrees, centered on the direction the zombie is facing.")]
    [SerializeField] private float eyeFieldOfView = 90f;

    [Tooltip("How many raycasts to fan across the field of view. More rays = finer detection, more expensive.")]
    [SerializeField] private int eyeRayCount = 5;

    [Tooltip("Height offset (local) from which vision rays are cast, so they don't hug the ground.")]
    [SerializeField] private float eyeHeight = 1.6f;

    [Tooltip("Layers that block the zombie's line of sight (walls, obstacles, etc).")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("Layer the player is on. Used to filter raycast hits.")]
    [SerializeField] private LayerMask playerMask;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private NavMeshAgent agent;
    private ZombieState currentState = ZombieState.Idle;

    private List<Vector3> eyeRayLocalDirections;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        // agent.isStopped = true; 

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        BuildEyeRayDirections();
    }

    private void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case ZombieState.Idle:
                TickIdle();
                break;

            case ZombieState.Chase:
                TickChase();
                break;
        }
    }
    private void TickIdle()
    {
        if (CanDetectPlayer())
        {
            EnterChaseState();
        }
    }

    private bool CanDetectPlayer()
    {
        return IsPlayerTooClose() || CanSeePlayer();
    }

    private bool IsPlayerTooClose()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= closeRangeRadius;
    }
    private bool CanSeePlayer()
    {
        Vector3 eyeOrigin = transform.position + Vector3.up * eyeHeight;

        foreach (Vector3 localDir in eyeRayLocalDirections)
        {
            Vector3 worldDir = transform.TransformDirection(localDir);

            if (Physics.Raycast(eyeOrigin, worldDir, out RaycastHit hit, eyeRange, obstacleMask | playerMask))
            {
                if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                {
                    return true; 
                }
            }
        }
        return false;
    }
    private void BuildEyeRayDirections()
    {
        eyeRayLocalDirections = new List<Vector3>();

        if (eyeRayCount <= 1)
        {
            eyeRayLocalDirections.Add(Vector3.forward);
            return;
        }
        float halfFov = eyeFieldOfView * 0.5f;
        float step = eyeFieldOfView / (eyeRayCount - 1);

        for (int i = 0; i < eyeRayCount; i++)
        {
            float angle = -halfFov + step * i;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            eyeRayLocalDirections.Add(rotation * Vector3.forward);
        }
    }
    private void EnterChaseState()
    {
        currentState = ZombieState.Chase;
        agent.isStopped = false;
    }

    private void TickChase()
    {
        agent.SetDestination(player.position);
    }
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeRangeRadius);

        Gizmos.color = Color.yellow;
        Vector3 eyeOrigin = transform.position + Vector3.up * eyeHeight;

        if (eyeRayLocalDirections == null || eyeRayLocalDirections.Count == 0)
        {
            BuildEyeRayDirections();
        }
        foreach (Vector3 localDir in eyeRayLocalDirections)
        {
            Vector3 worldDir = transform.TransformDirection(localDir);
            Gizmos.DrawRay(eyeOrigin, worldDir * eyeRange);
        }
    }
}
