using System;
using UnityEngine;
public static class NoiseSystem
{
    public enum NoiseKind { Footstep, Gunshot, Other }
    public readonly struct Noise
    {
        public readonly Vector3 Position;
        public readonly float Loudness, FullVolumeRadius, MaxRadius;
        public readonly Transform Source;
        public readonly NoiseKind Kind;
        public Noise(Vector3 position, float loudness, float fullVolumeRadius, float maxRadius,
            Transform source, NoiseKind kind)
        {
            Position = position;
            Loudness = Mathf.Max(0, loudness);
            FullVolumeRadius = Mathf.Max(0, fullVolumeRadius);
            MaxRadius = Mathf.Max(FullVolumeRadius + 0.01f, maxRadius);
            Source = source;
            Kind = kind;
        }
        public float AtDistance(float distance)
        {
            return Loudness * (1f - Mathf.InverseLerp(FullVolumeRadius, MaxRadius, distance));
        }
    }
    public static event Action<Noise> Emitted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { Emitted = null; }

    public static void Emit(Vector3 position, float loudness, float fullVolumeRadius,
        float maxRadius, Transform source = null, NoiseKind kind = NoiseKind.Other)
    {
        if (loudness <= 0 || maxRadius <= 0) return;
        Emitted?.Invoke(new Noise(position, loudness, fullVolumeRadius, maxRadius, source, kind));
    }
}