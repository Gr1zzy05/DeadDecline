using UnityEngine;
[AddComponentMenu("Dead Decline/Debug/Sight Debug")]
public class SightDebug : MonoBehaviour
{
    public enum ActorKind { Human, NPC, Zombie }
    public ActorKind actorKind = ActorKind.NPC;
    public bool showSight = true;
    public bool onlyWhenSelected;
    public bool showSampleRays = true;
    public bool showLabel = true;
    [Range(3, 64)] public int rayCount = 17;
    [Header("Generic human / NPC vision (zombies use their AI settings)")]
    public Transform eyes;
    public Transform target;
    [Min(0)] public float eyeHeight = 1.6f;
    [Min(0)] public float sightRange = 10f;
    [Range(0, 360)] public float fieldOfView = 90f;
    public LayerMask obstacleMask = ~0;
    [Tooltip("Optional human camera. Uses its position, direction and horizontal camera FOV.")]
    public Camera viewCamera;
    [Header("Colors")]
    public Color humanColor = new Color(0.2f, 0.7f, 1f);
    public Color npcColor = new Color(1f, 0.75f, 0.1f);
    public Color zombieColor = new Color(0.8f, 0.2f, 1f);
    public Color visibleColor = Color.green;
    public Color blockedColor = Color.red;
    public Color outsideColor = Color.gray;

    private void OnDrawGizmos() { if (!onlyWhenSelected) DrawSight(); }
    private void OnDrawGizmosSelected() { if (onlyWhenSelected) DrawSight(); }

    private void DrawSight()
    {
        if (!showSight || !enabled) return;
        ZombieAI zombie = GetComponent<ZombieAI>();
        Vector3 origin = eyes != null ? eyes.position : transform.position + Vector3.up * eyeHeight;
        Vector3 forward = eyes != null ? eyes.forward : transform.forward;
        float range = sightRange, fov = fieldOfView;
        LayerMask mask = obstacleMask;
        Transform observed = target;
        ActorKind kind = actorKind;
        if (zombie != null)
        {
            origin = zombie.SightOrigin;
            forward = zombie.SightForward;
            range = zombie.DebugSightRange;
            fov = zombie.DebugFieldOfView;
            mask = zombie.SightObstacles;
            observed = zombie.SightTarget;
            kind = ActorKind.Zombie;
        }
        else if (viewCamera != null)
        {
            origin = viewCamera.transform.position;
            forward = viewCamera.transform.forward;
            fov = Camera.VerticalToHorizontalFieldOfView(viewCamera.fieldOfView, viewCamera.aspect);
        }
        range = Mathf.Max(0, range);
        fov = Mathf.Clamp(fov, 0, 360);
        Vector3 flatForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 0.001f) flatForward = transform.forward;
        Color actorColor = kind == ActorKind.Human ? humanColor : kind == ActorKind.Zombie ? zombieColor : npcColor;
        Color oldColor = Gizmos.color;
        Gizmos.color = actorColor;
        Gizmos.DrawSphere(origin, 0.06f);
        int segments = Mathf.Max(12, Mathf.CeilToInt(fov / 5f));
        Vector3 previous = origin + Quaternion.AngleAxis(-fov * 0.5f, Vector3.up) * flatForward * range;
        Gizmos.DrawLine(origin, previous);
        for (int i = 1; i <= segments; i++)
        {
            Vector3 point = origin + Quaternion.AngleAxis(-fov * 0.5f + fov * i / segments, Vector3.up) * flatForward * range;
            Gizmos.DrawLine(previous, point);
            previous = point;
        }
        Gizmos.DrawLine(origin, previous);
        if (showSampleRays)
        {
            int count = Mathf.Clamp(rayCount, 3, 64);
            for (int i = 0; i < count; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(-fov * 0.5f + fov * i / (count - 1f), Vector3.up) * flatForward;
                bool blocked = SightQuery.TryGetBlocker(origin, direction, range, mask, transform, observed, out var hit);
                Gizmos.color = blocked ? blockedColor : new Color(actorColor.r, actorColor.g, actorColor.b, 0.35f);
                Gizmos.DrawLine(origin, blocked ? hit.point : origin + direction * range);
                if (blocked) Gizmos.DrawSphere(hit.point, 0.045f);
            }
        }
        string status = "No target";
        if (observed != null)
        {
            var result = SightQuery.Evaluate(origin, forward, range, fov, mask, transform, observed, out var block);
            Vector3 targetPoint = SightQuery.TargetPoint(observed);
            status = result.ToString();
            Gizmos.color = result == SightQuery.Result.Visible ? visibleColor :
                result == SightQuery.Result.Blocked ? blockedColor : outsideColor;
            Gizmos.DrawLine(origin, targetPoint);
            Gizmos.DrawWireSphere(targetPoint, 0.15f);
            if (result == SightQuery.Result.Blocked) Gizmos.DrawSphere(block.point, 0.09f);
        }
#if UNITY_EDITOR
        if (showLabel)
        {
            UnityEditor.Handles.Label(origin + Vector3.up * 0.25f,
                $"{name} [{kind}] | Range {range:0.0}m | FOV {fov:0} degrees | {status}" +
                (zombie != null && Application.isPlaying ? $" | State: {zombie.CurrentState}" : ""));
        }
#endif
        Gizmos.color = oldColor;
    }
}