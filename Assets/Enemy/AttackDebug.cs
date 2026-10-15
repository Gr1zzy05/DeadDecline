using UnityEngine;

public static class AttackDebug
{
    public static Transform FindOwner(Component attacker)
    {
        var health = attacker.GetComponentInParent<Health>();
        if (health != null) return health.transform;
        var controller = attacker.GetComponentInParent<CharacterController>();
        return controller != null ? controller.transform : attacker.transform.root;
    }

    public static bool IsOwner(Collider collider, Transform owner)
    {
        return collider.transform == owner || collider.transform.IsChildOf(owner);
    }

    public static void Sphere(Vector3 center, float radius, Color color, float duration)
    {
        for (int ring = 0; ring < 3; ring++)
        {
            var points = new Vector3[33];
            for (int step = 0; step <= 32; step++)
            {
                float angle = step * Mathf.PI * 2f / 32f;
                float a = Mathf.Cos(angle) * radius, b = Mathf.Sin(angle) * radius;
                points[step] = center + (ring == 0 ? new Vector3(a, b, 0) :
                    ring == 1 ? new Vector3(a, 0, b) : new Vector3(0, a, b));
            }
            Draw(points, color, duration);
        }
    }
    public static void Ray(Vector3 start, Vector3 end, Color color, float duration)
    {
        Draw(new[] { start, end }, color, duration);
    }

    private static void Draw(Vector3[] points, Color color, float duration)
    {
        var shader = Shader.Find("HDRP/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
        if (shader == null) return;
        var go = new GameObject("Attack Hitbox Debug");
        go.layer = 2;
        var line = go.AddComponent<LineRenderer>();
        var material = new Material(shader);
        material.SetColor("_UnlitColor", color);
        material.SetColor("_BaseColor", color);
        material.SetColor("_Color", color);
        line.sharedMaterial = material;
        line.useWorldSpace = true;
        line.positionCount = points.Length;
        line.SetPositions(points);
        line.startWidth = line.endWidth = 0.025f;
        line.startColor = line.endColor = color;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        Object.Destroy(go, Mathf.Max(0.02f, duration));
        Object.Destroy(material, Mathf.Max(0.02f, duration));
    }
}