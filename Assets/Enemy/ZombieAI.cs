using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent), typeof(SightDebug))]
public class ZombieAI : MonoBehaviour
{
    // เพิ่มสถานะ Scream
    public enum ZombieState { Idle, Investigate, Scream, Chase, Attack, Flee }
    
    [Header("Target")]
    [SerializeField] private Transform player;
    [Header("Drag a personality asset here")]
    [SerializeField] private ZombiePersonality personality;
    [Header("Fallback vision (when no asset is assigned)")]
    [SerializeField, Min(0)] private float eyeRange = 10f;
    [SerializeField, Range(0, 360)] private float eyeFieldOfView = 90f;
    [Header("Line of sight")]
    [SerializeField] private Transform eyes;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private LayerMask obstacleMask;
    [Header("Attack hitbox")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector3 attackOffset = new Vector3(0, 0, 0.9f);
    [SerializeField, Min(0.01f)] private float attackRadius = 1f;
    [SerializeField] private LayerMask attackMask = ~0;
    [SerializeField] private bool showHitboxOnAttack = true;
    [SerializeField, Min(0.02f)] private float hitboxDisplayDuration = 0.35f;
    [SerializeField] private Color hitboxColor = Color.red;
    [SerializeField, Min(0)] private float attackWindup = 0.5f;
    [SerializeField, Min(0.1f)] private float fallbackAttackCooldown = 2f;
    [Header("Attack callbacks")]
    public UnityEvent onAttack = new UnityEvent();
    [SerializeField] private bool drawGizmos = true;

    [Header("Animation & Audio")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioSource movementAudioSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] idleClips;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip screamClip;
    [SerializeField] private AudioClip attackClip;
    
    [Header("Animation Settings")]
    [SerializeField, Min(0)] private float screamDuration = 2f;
    [SerializeField] private Vector2 idleSoundInterval = new Vector2(3f, 8f);

    public ZombieState CurrentState { get; private set; }
    public bool IsAlerted => CanSeePlayer() ||
        (CurrentState != ZombieState.Idle && Time.time < interestExpires);

    public void NoticeAttacker(Transform attacker)
    {
        if (attacker == null) return;
        interestPosition = attacker.position;
        interestExpires = Time.time + (personality != null ? personality.memoryDuration : 5f);
        CurrentState = ZombieState.Investigate;
    }

    public float LastHeardLoudness { get; private set; }
    private NavMeshAgent agent;
    private FPSController playerMovement;
    private Vector3 interestPosition;
    private float interestExpires, nextAttackTime;
    private bool preparingAttack;
    private float attackReadyTime;
    
    private float screamEndTime;
    private float nextIdleSoundTime;

    private Vector3 AttackCenter => (attackPoint != null ? attackPoint : transform).TransformPoint(attackOffset);
    private float SightRange => personality != null ? personality.sightRange : eyeRange;
    private float Fov => personality != null ? personality.fieldOfView : eyeFieldOfView;
    private float AttackRange => personality != null ? Mathf.Max(0.1f, personality.attackRange) : 1.8f;
    private Vector3 EyePosition => eyes != null ? eyes.position : transform.position + Vector3.up * eyeHeight;
    public Vector3 SightOrigin => EyePosition;
    public Vector3 SightForward => eyes != null ? eyes.forward : transform.forward;
    public float DebugSightRange => SightRange;
    public float DebugFieldOfView => Fov;
    public LayerMask SightObstacles => obstacleMask;
    public Transform SightTarget => player;
    private bool CanNavigate => agent != null && agent.enabled && agent.isOnNavMesh;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            var target = GameObject.FindGameObjectWithTag("Player");
            if (target != null) player = target.transform;
        }
        nextIdleSoundTime = Time.time + Random.Range(idleSoundInterval.x, idleSoundInterval.y);
    }

    private float MovementSpeed
    {
        get
        {
            if (playerMovement == null && player != null)
                playerMovement = player.GetComponentInParent<FPSController>();
            bool relative = personality == null || personality.usePlayerRelativeSpeed;
            if (relative)
            {
                float humanSpeed = playerMovement != null ? playerMovement.moveSpeed : 5f;
                return Mathf.Max(0, humanSpeed * (personality != null ? personality.playerSpeedMultiplier : 0.6f));
            }
            return Mathf.Max(0, personality.movementSpeed);
        }
    }

    private void OnEnable() { NoiseSystem.Emitted += Hear; }
    private void OnDisable()
    {
        NoiseSystem.Emitted -= Hear;
        preparingAttack = false;
        StopMoving();
    }

    private void Hear(NoiseSystem.Noise noise)
    {
        if (noise.Source != null && (noise.Source == transform || noise.Source.IsChildOf(transform))) return;
        float distance = Vector3.Distance(transform.position, noise.Position);
        if (distance > (personality != null ? personality.hearingRange : 30f)) return;
        float perceived = noise.AtDistance(distance) * (personality != null ? personality.hearingSensitivity : 1f);
        if (perceived < (personality != null ? Mathf.Max(0.001f, personality.hearingThreshold) : 0.6f)) return;
        LastHeardLoudness = perceived;
        if (CanSeePlayer()) return;
        
        var reaction = personality != null ? personality.ReactToNoise(noise, perceived)
            : ZombiePersonality.NoiseReaction.Investigate;
        if (reaction == ZombiePersonality.NoiseReaction.Ignore) return;
        
        interestPosition = noise.Position;
        if (CurrentState == ZombieState.Idle) CurrentState = ZombieState.Investigate;
        
        if (reaction == ZombiePersonality.NoiseReaction.Flee)
        {
            Vector3 away = transform.position - noise.Position;
            away.y = 0;
            if (away.sqrMagnitude < 0.01f) away = -transform.forward;
            interestPosition = transform.position + away.normalized * (personality != null ? personality.fleeDistance : 8f);
            CurrentState = ZombieState.Flee;
        }
        interestExpires = Time.time + (personality != null ? personality.investigationDuration : 8f);
    }

    private bool CanSeePlayer()
    {
        return SightQuery.Evaluate(EyePosition, SightForward, SightRange, Fov, obstacleMask,
            transform, player, out _) == SightQuery.Result.Visible;
    }

    private void Update()
    {
        UpdateAnimations();
        UpdateAudio();

        if (CanNavigate) agent.speed = MovementSpeed;
        bool visible = CanSeePlayer();

        if (visible)
        {
            // หากเพิ่งเห็นผู้เล่นครั้งแรก ให้เปลี่ยนเป็นสถานะ Scream
            if (CurrentState == ZombieState.Idle || CurrentState == ZombieState.Investigate)
            {
                CurrentState = ZombieState.Scream;
                screamEndTime = Time.time + screamDuration;
                StopMoving();
                
                if (animator != null) animator.SetTrigger("Scream");
                PlayVoice(screamClip);
                return;
            }

            // จัดการสถานะ Scream (รอจนกว่าจะคำรามเสร็จ)
            if (CurrentState == ZombieState.Scream)
            {
                Vector3 facingScream = player.position - transform.position;
                facingScream.y = 0;
                if (facingScream.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facingScream), 360f * Time.deltaTime);

                if (Time.time < screamEndTime) return; // ยังคำรามไม่เสร็จ
                CurrentState = ZombieState.Chase;
            }

            interestPosition = player.position;
            interestExpires = Time.time + (personality != null ? personality.memoryDuration : 5f);
            
            if (Vector3.Distance(transform.position, player.position) <= AttackRange)
            {
                CurrentState = ZombieState.Attack;
                StopMoving();
                Vector3 facing = player.position - transform.position;
                facing.y = 0;
                if (facing.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facing), 360f * Time.deltaTime);
                
                if (!preparingAttack && Time.time >= nextAttackTime)
                {
                    preparingAttack = true;
                    attackReadyTime = Time.time + Mathf.Max(0, attackWindup);
                    
                    if (animator != null) animator.SetTrigger("Attack");
                    PlayVoice(attackClip);
                }
                
                if (preparingAttack && Time.time >= attackReadyTime)
                {
                    preparingAttack = false;
                    nextAttackTime = Time.time + (personality != null ?
                        Mathf.Max(0.1f, personality.attackCooldown) : fallbackAttackCooldown);
                    Vector3 center = AttackCenter;
                    if (showHitboxOnAttack)
                        AttackDebug.Sphere(center, attackRadius, hitboxColor, hitboxDisplayDuration);
                    var health = player.GetComponentInParent<Health>();
                    if (health != null)
                    {
                        foreach (var contact in Physics.OverlapSphere(center, Mathf.Max(0.01f, attackRadius),
                            attackMask, QueryTriggerInteraction.Ignore))
                        {
                            if (contact.GetComponentInParent<Health>() != health) continue;
                            health.TakeDamage(personality != null ? personality.attackDamage : 10f);
                            break;
                        }
                    }
                    onAttack.Invoke();
                }
                return;
            }
            
            preparingAttack = false;
            CurrentState = ZombieState.Chase;
        }
        else if (CurrentState == ZombieState.Attack || CurrentState == ZombieState.Scream)
        {
            if (CurrentState == ZombieState.Scream && Time.time < screamEndTime) return;
            
            preparingAttack = false;
            CurrentState = ZombieState.Chase;
        }

        if (CurrentState == ZombieState.Idle || Time.time >= interestExpires)
        {
            CurrentState = ZombieState.Idle;
            StopMoving();
            return;
        }
        
        if (CanNavigate)
        {
            agent.stoppingDistance = visible ? AttackRange * 0.8f : 0.3f;
            agent.isStopped = false;
            
            // ขยายรัศมีเป็น 10f เผื่อผู้เล่นยืนบนกล่อง/เนิน หรือกระโดดอยู่
            if (NavMesh.SamplePosition(interestPosition, out var hit, 10f, agent.areaMask))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // Fallback: หากหา NavMesh ใกล้ๆ ผู้เล่นไม่เจอจริงๆ ให้ฝืนสั่งเดินไปที่จุดนั้นโดยตรง
                // Unity จะพยายามลากเส้นทางไปให้ใกล้จุดเป้าหมายที่สุดเท่าที่ทำได้
                agent.SetDestination(interestPosition); 
            }
        }
        else StopMoving();
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;
        // กำหนดค่า Speed ให้ Animator เพื่อใช้ใน Blend Tree (Idle -> Walk/Run)
        float speed = (CanNavigate && !agent.isStopped) ? agent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed);
    }

    private void UpdateAudio()
    {
        // เสียงเดิน (แยก AudioSource เพื่อไม่ให้ทับกับเสียงร้อง)
        if (movementAudioSource != null)
        {
            bool isMoving = CanNavigate && !agent.isStopped && agent.velocity.sqrMagnitude > 0.1f;
            if (isMoving && !movementAudioSource.isPlaying && walkClip != null)
            {
                movementAudioSource.clip = walkClip;
                movementAudioSource.loop = true;
                movementAudioSource.Play();
            }
            else if (!isMoving && movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }
        }

        // เสียงคำรามตอน Idle แบบสุ่มเวลา
        if (CurrentState == ZombieState.Idle || CurrentState == ZombieState.Investigate)
        {
            if (Time.time >= nextIdleSoundTime)
            {
                if (idleClips != null && idleClips.Length > 0)
                {
                    AudioClip clip = idleClips[Random.Range(0, idleClips.Length)];
                    PlayVoice(clip);
                }
                nextIdleSoundTime = Time.time + Random.Range(idleSoundInterval.x, idleSoundInterval.y);
            }
        }
    }

    private void PlayVoice(AudioClip clip)
    {
        if (voiceAudioSource != null && clip != null)
        {
            voiceAudioSource.PlayOneShot(clip);
        }
    }

    private void StopMoving()
    {
        if (!CanNavigate) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = hitboxColor;
        Gizmos.DrawWireSphere(AttackCenter, Mathf.Max(0.01f, attackRadius));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, personality != null ? personality.hearingRange : 30f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
        if (Application.isPlaying && CurrentState != ZombieState.Idle)
            Gizmos.DrawLine(transform.position, interestPosition);
    }
}