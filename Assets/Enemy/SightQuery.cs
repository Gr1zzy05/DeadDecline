using UnityEngine;
public static class SightQuery
{
    public enum Result { Visible, Blocked, OutOfRange, OutsideFOV, Inactive }
    public static Vector3 TargetPoint(Transform target)
    {
        var body = target.GetComponentInChildren<Collider>();
        return body != null ? body.bounds.center : target.position + Vector3.up;
    }
    public static Result Evaluate(Vector3 origin, Vector3 forward, float range, float fov,
        LayerMask obstacles, Transform observer, Transform target, out RaycastHit blocker)
    {
        blocker = default;
        if (target == null || !target.gameObject.activeInHierarchy) return Result.Inactive;
        Vector3 direction = TargetPoint(target) - origin;
        if (direction.magnitude > Mathf.Max(0, range)) return Result.OutOfRange;
        if (Vector3.Angle(Vector3.ProjectOnPlane(forward, Vector3.up),
            Vector3.ProjectOnPlane(direction, Vector3.up)) > Mathf.Clamp(fov, 0, 360) * 0.5f)
            return Result.OutsideFOV;
        return TryGetBlocker(origin, direction.normalized, direction.magnitude, obstacles,
            observer, target, out blocker) ? Result.Blocked : Result.Visible;
    }
    public static bool TryGetBlocker(Vector3 origin, Vector3 direction, float distance,
        LayerMask obstacles, Transform observer, Transform target, out RaycastHit blocker)
    {
        blocker = default;
        float nearest = float.PositiveInfinity;
        foreach (var hit in Physics.RaycastAll(origin, direction, distance, obstacles, QueryTriggerInteraction.Ignore))
        {
            Transform t = hit.transform;
            if (observer != null && (t == observer || t.IsChildOf(observer))) continue;
            if (target != null && (t == target || t.IsChildOf(target))) continue;
            if (hit.distance >= nearest) continue;
            nearest = hit.distance;
            blocker = hit;
        }
        return !float.IsPositiveInfinity(nearest);
    }
}