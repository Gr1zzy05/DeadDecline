using UnityEngine;
[CreateAssetMenu(menuName = "Dead Decline/Zombie Personality")]
public class ZombiePersonality : ScriptableObject
{
    public enum NoiseReaction { Ignore, Investigate, Flee }
    [Header("Vision")]
    [Min(0)] public float sightRange = 10f;
    [Range(0, 360)] public float fieldOfView = 90f;
    [Header("Hearing (360 degrees)")]
    [Min(0)] public float hearingRange = 30f;
    [Min(0.001f)] public float hearingThreshold = 0.6f;
    [Min(0)] public float hearingSensitivity = 1f;
    public NoiseReaction noiseReaction = NoiseReaction.Investigate;
    [Header("Movement and memory")]
    public bool usePlayerRelativeSpeed = true;
    [Min(0)] public float playerSpeedMultiplier = 0.6f;
    [Min(0)] public float movementSpeed = 3f;
    [Min(0.1f)] public float memoryDuration = 5f;
    [Min(0.1f)] public float investigationDuration = 8f;
    [Min(0.1f)] public float fleeDistance = 8f;
    [Header("Attack")]
    [Min(0.1f)] public float attackRange = 1.8f;
    [Min(0)] public float attackDamage = 10f;
    [Min(0.1f)] public float attackCooldown = 2f;

    public virtual NoiseReaction ReactToNoise(NoiseSystem.Noise noise, float perceivedLoudness)
    {
        return noiseReaction;
    }
}