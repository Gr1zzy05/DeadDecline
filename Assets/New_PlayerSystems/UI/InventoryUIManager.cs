using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject inventoryPanel; // Drag your entire Inventory Panel here

    [Header("Input")]
    public InputActionReference toggleInventoryAction; // Assign the 'Tab' or 'I' key
    private bool isInventoryOpen = false;

    private void Start()
    {
        // Ensure UI is hidden and cursor is locked for gameplay on start
        CloseInventory();
    }

    private void OnEnable()
    {
        toggleInventoryAction.action.Enable();
        toggleInventoryAction.action.performed += ctx => ToggleInventory();
    }

    private void OnDisable()
    {
        toggleInventoryAction.action.Disable();
        toggleInventoryAction.action.performed -= ctx => ToggleInventory();
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (isInventoryOpen)
        {
            OpenInventory();
        }
        else
        {
            CloseInventory();
        }
    }

    private void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        
        // Free the mouse so the player can drag items
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Pause the game world so enemies don't kill the player while sorting items
    }

    private void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        
        // Lock the mouse back to the center of the screen for FPS aiming
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Resume time
        Time.timeScale = 1f;
    }
}