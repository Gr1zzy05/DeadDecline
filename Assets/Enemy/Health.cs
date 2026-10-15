using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Min(1)] public float maxHealth = 100f;
    public bool infiniteHealth;
    public bool oneHitWhenUnaware = true;
    public bool oneHitFromBehind = true;
    [Range(1, 180)] public float rearTakedownAngle = 120f;
    [Header("Hit feedback")]
    public bool flashOnHit = true;
    public Color hitColor = Color.red;
    [Min(0.01f)] public float flashDuration = 0.2f;
    public bool showPlayerScreenFlash = true;
    [Range(0, 1)] public float screenFlashOpacity = 0.3f;
    public Renderer[] hitRenderers;
    public UnityEvent onHit = new UnityEvent();
    public float CurrentHealth => currentHealth;
    public bool IsDead => dead;
    private float currentHealth, flashRemaining;
    private bool dead, flashing;
    private MaterialPropertyBlock[][] originalBlocks;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        if (hitRenderers == null || hitRenderers.Length == 0)
            hitRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void TakeWeaponDamage(float amount, Transform attacker)
    {
        if (dead || amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) return;
        var zombie = GetComponent<ZombieAI>();
        bool unaware = zombie != null && oneHitWhenUnaware && !zombie.IsAlerted;
        bool rearHit = zombie != null && oneHitFromBehind && IsAttackerBehind(attacker);
        TakeDamage(unaware || rearHit ? Mathf.Max(amount, currentHealth) : amount);
        if (zombie != null && !dead) zombie.NoticeAttacker(attacker);
    }

    public bool IsAttackerBehind(Transform attacker)
    {
        if (attacker == null) return false;
        Vector3 toAttacker = Vector3.ProjectOnPlane(attacker.position - transform.position, Vector3.up);
        Vector3 back = Vector3.ProjectOnPlane(-transform.forward, Vector3.up);
        if (toAttacker.sqrMagnitude < 0.0001f || back.sqrMagnitude < 0.0001f) return false;
        float threshold = Mathf.Cos(Mathf.Clamp(rearTakedownAngle, 1, 180) * 0.5f * Mathf.Deg2Rad);
        return Vector3.Dot(back.normalized, toAttacker.normalized) >= threshold;
    }

    public void TakeDamage(float amount)
    {
        if (dead || amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) return;
        if (!infiniteHealth) currentHealth = Mathf.Max(0, currentHealth - amount);
        if (flashOnHit) BeginFlash();
        onHit.Invoke();
        if (currentHealth <= 0 && !infiniteHealth)
        {
            dead = true;
            Destroy(gameObject);
        }
    }

    [ContextMenu("Restore Full Health")]
    public void RestoreFullHealth()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }

    private void BeginFlash()
    {
        flashRemaining = Mathf.Max(0.01f, flashDuration);
        if (flashing) return;
        flashing = true;
        originalBlocks = new MaterialPropertyBlock[hitRenderers.Length][];
        for (int i = 0; i < hitRenderers.Length; i++)
        {
            var renderer = hitRenderers[i];
            if (renderer == null) continue;
            var materials = renderer.sharedMaterials;
            originalBlocks[i] = new MaterialPropertyBlock[materials.Length];
            for (int slot = 0; slot < materials.Length; slot++)
            {
                var original = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(original, slot);
                originalBlocks[i][slot] = original;
                var flash = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(flash, slot);
                if (materials[slot] != null)
                {
                    if (materials[slot].HasProperty(BaseColor)) flash.SetColor(BaseColor, hitColor);
                    if (materials[slot].HasProperty(ColorId)) flash.SetColor(ColorId, hitColor);
                }
                renderer.SetPropertyBlock(flash, slot);
            }
        }
    }

    private void Update()
    {
        if (!flashing) return;
        flashRemaining -= Time.deltaTime;
        if (flashRemaining <= 0) EndFlash();
    }

    private void EndFlash()
    {
        if (!flashing) return;
        for (int i = 0; i < hitRenderers.Length; i++)
        {
            if (hitRenderers[i] == null || originalBlocks[i] == null) continue;
            for (int slot = 0; slot < originalBlocks[i].Length; slot++)
                hitRenderers[i].SetPropertyBlock(originalBlocks[i][slot], slot);
        }
        flashing = false;
        flashRemaining = 0;
    }

    private void OnDisable() { EndFlash(); }

    private void OnGUI()
    {
        if (!flashing || !showPlayerScreenFlash || GetComponent<FPSController>() == null) return;
        Color previous = GUI.color;
        GUI.color = new Color(hitColor.r, hitColor.g, hitColor.b,
            screenFlashOpacity * Mathf.Clamp01(flashRemaining / Mathf.Max(0.01f, flashDuration)));
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previous;
    }
}