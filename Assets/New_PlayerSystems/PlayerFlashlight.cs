using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFlashlight : MonoBehaviour
{
    [Header("Setup")]
    public Light spotlight; // Drag your child Spotlight here
    
    [Header("Input")]
    public InputActionReference toggleFlashlightAction; // Assign 'F' key

    private void Start()
    {
        // Ensure it starts off
        if (spotlight != null) spotlight.enabled = false;
    }

    private void OnEnable()
    {
        toggleFlashlightAction.action.Enable();
        toggleFlashlightAction.action.performed += ctx => ToggleLight();
    }

    private void OnDisable()
    {
        toggleFlashlightAction.action.Disable();
        toggleFlashlightAction.action.performed -= ctx => ToggleLight();
    }

    private void ToggleLight()
    {
        if (spotlight != null)
        {
            spotlight.enabled = !spotlight.enabled;
            Debug.Log($"Flashlight is now {(spotlight.enabled ? "ON" : "OFF")}");
        }
    }
}