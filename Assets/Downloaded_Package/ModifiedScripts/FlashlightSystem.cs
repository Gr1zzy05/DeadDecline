using UnityEngine;
using UnityEngine.InputSystem;
public class FlashlightSystem : MonoBehaviour
{
 [Header("References")]
    [Tooltip("Drag your Spotlight object here.")]
    public Light flashlight;
    
    [Tooltip("Optional: Drag an AudioSource here to play a click sound when toggling.")]
    public AudioSource clickSound;

    [Header("Settings")]
    public bool startTurnedOn = false;

    void Start()
    {
        // Set the initial state of the flashlight
        if (flashlight != null)
        {
            flashlight.enabled = startTurnedOn;
        }
    }

    void Update()
    {
        // Check if a keyboard exists, then check if 'F' was pressed this frame
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
    }

    public void ToggleFlashlight()
    {
        if (flashlight == null) return;

        // Flip the current enabled state
        flashlight.enabled = !flashlight.enabled;

        // Play the click sound if you assigned one
        if (clickSound != null)
        {
            clickSound.Play();
        }
    }
}
