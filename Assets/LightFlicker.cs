using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{[Header("Intensity Settings")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1.5f;

    [Header("Smoothing (Higher = Smoother/Slower)")]
    [SerializeField] [Range(1, 50)] private int smoothing = 5;

    private Light targetLight;
    private Queue<float> smoothQueue;
    private float lastSum = 0f;

    void Start()
    {
        // Automatically grab the Light component attached to this GameObject
        targetLight = GetComponent<Light>();
        smoothQueue = new Queue<float>(smoothing);
    }

    void Update()
    {
        if (targetLight == null) return;

        // Pop the oldest value off the queue to maintain size
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        // Generate a new random intensity and add it to the queue
        float newIntensity = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newIntensity);
        lastSum += newIntensity;

        // Average the queue values to avoid harsh single-frame pops
        targetLight.intensity = lastSum / smoothQueue.Count;
    }
}
